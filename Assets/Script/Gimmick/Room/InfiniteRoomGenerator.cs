//部屋が無限に続くようにみせる
using System.Collections.Generic;
using UnityEngine;
public class InfiniteRoomGenerator : MonoBehaviour
{
    [Header("生成する部屋Prefab")]
    public GameObject[] roomPrefabs;

    [Header("扉Prefab 木/石/金属など")]
    public GameObject[] doorPrefabs;

    [Header("パズルPrefab")]
    public GameObject[] puzzlePrefabs;

    [Header("部屋サイズ")]
    public float roomSizeX = 20f;
    public float roomSizeZ = 20f;

    [Header("生成高さ")]
    public float roomHeight = 0f;

    [Header("最初の部屋")]
    public RoomCell startRoom;

    [Header("出口設定 North/East/Westのみ")]
    [Range(0f, 1f)]
    public float exitChance = 0.5f;

    public bool guaranteeAtLeastOneExit = true;

    [Header("座標反転設定")]
    [Tooltip("ONなら North を -Z 側、South を +Z 側として扱う")]
    public bool northSouthInverted = true;

    [Tooltip("ONなら East と West を反転する")]
    public bool eastWestInverted = true;

    [Header("古い部屋削除")]
    [Tooltip("ONなら、現在部屋から一定距離以上離れた部屋を削除する")]
    public bool deleteFarRooms = true;

    [Tooltip("現在部屋からこの距離より遠い部屋を削除する。5ならマンハッタン距離5以上を削除")]
    public int deleteDistance = 5;

    [Header("既存部屋上書き")]
    [Tooltip("ONなら、新しく部屋を生成する位置に既存部屋があった場合、それを削除して新規生成する")]
    public bool overwriteExistingRoomOnGenerate = true;

    [Header("デバッグ")]
    public bool debugLog = true;

    private Dictionary<Vector2Int, RoomCell> generatedRooms =
        new Dictionary<Vector2Int, RoomCell>();

    void Start()
    {
        //部屋のIDを設定と生成
        if (startRoom != null)
        {
            startRoom.gridPosition = Vector2Int.zero;
            startRoom.generator = this;

            RoomIdentity identity = startRoom.GetComponent<RoomIdentity>();
            if (identity != null)
            {
                identity.SetupId(startRoom.gridPosition);
            }

            RegisterRoom(startRoom);
            SetupRoom(startRoom);
            GenerateAround(startRoom);


        }

    }

    public void RegisterRoom(RoomCell room)
    {
        if (room == null)
            return;

        if (generatedRooms.ContainsKey(room.gridPosition))
            return;

        generatedRooms.Add(room.gridPosition, room);

        if (debugLog)
            Debug.Log("部屋登録: " + room.gridPosition);
    }

    public void GenerateAround(RoomCell centerRoom)
    {
        if (centerRoom == null)
            return;

        //現在部屋を基準に古い部屋削除
        if (deleteFarRooms)
            DeleteRoomsFarFrom(centerRoom.gridPosition);

        //Southは入口専用なので生成方向から除外
        TryGenerateFromLocalExit(centerRoom, RoomDirection.North);
        TryGenerateFromLocalExit(centerRoom, RoomDirection.East);
        TryGenerateFromLocalExit(centerRoom, RoomDirection.West);

        RefreshRoomDoors(centerRoom);
        ApplyWallSwitcher(centerRoom);
    }

    void TryGenerateFromLocalExit(RoomCell fromRoom, RoomDirection localExitDirection)
    {
        RoomExitPatternGroup fromGroup =
            fromRoom.GetExitGroup(localExitDirection);

        if (fromGroup == null || !fromGroup.enableExit)
            return;

        RoomDirection worldDirection =
            LocalDirectionToWorldDirection(fromRoom.transform, localExitDirection);

        Vector2Int targetGrid =
            fromRoom.gridPosition + DirectionToGridOffset(worldDirection);

        //既存部屋がある場合
        if (generatedRooms.ContainsKey(targetGrid))
        {
            if (!overwriteExistingRoomOnGenerate)
            {
                EnsureExitVisible(fromRoom, localExitDirection);
                RefreshRoomDoors(fromRoom);
                ApplyWallSwitcher(fromRoom);

                if (debugLog)
                {
                    Debug.Log(
                        "既存部屋あり: "
                        + targetGrid
                        + " / 上書きなし"
                    );
                }

                return;
            }

            DeleteRoomAt(targetGrid);
        }

        GameObject prefab = PickRandomRoomPrefab();

        if (prefab == null)
            return;

        Quaternion rotation =
            GetRotationForSouthEntrance(worldDirection);

        GameObject obj =
            Instantiate(prefab, GridToWorld(targetGrid), rotation);

        RoomCell newRoom = obj.GetComponent<RoomCell>();

        if (newRoom == null)
        {
            Debug.LogError(prefab.name + " に RoomCell がありません");
            return;
        }

        newRoom.gridPosition = targetGrid;
        newRoom.generator = this;

        //RoomIdentity設定
        RoomIdentity identity = newRoom.GetComponent<RoomIdentity>();

        if (identity != null)
        {
            identity.SetupId(targetGrid);
        }

        if (newRoom.exitGroups == null || newRoom.exitGroups.Length == 0)
        {
            newRoom.exitGroups =
                newRoom.GetComponentsInChildren<RoomExitPatternGroup>(true);
        }

        RegisterRoom(newRoom);

        SetupRoom(newRoom);

        EnsureExitVisible(fromRoom, localExitDirection);
        RefreshRoomDoors(fromRoom);
        ApplyWallSwitcher(fromRoom);

        if (debugLog)
        {
            Debug.Log(
                "部屋生成: "
                + targetGrid
                + " / from="
                + fromRoom.gridPosition
                + " / localExit="
                + localExitDirection
                + " / worldDir="
                + worldDirection
                + " / rotationY="
                + rotation.eulerAngles.y
            );
        }
    }

    void SetupRoom(RoomCell room)
    {
        DecideExits(room);
        DisableSouth(room);

        SpawnDoorsForRoom(room);
        ApplyWallSwitcher(room);
        SpawnPuzzleForRoom(room);
    }

    void DecideExits(RoomCell room)
    {
        if (room == null || room.exitGroups == null)
            return;

        List<RoomExitPatternGroup> candidateGroups =
            new List<RoomExitPatternGroup>();

        foreach (RoomExitPatternGroup group in room.exitGroups)
        {
            if (group == null)
                continue;

            group.exitChance = exitChance;

            //南側は完全に入口専用なので抽選から排除
            if (group.direction == RoomDirection.South)
            {
                group.ForceDisable();
                continue;
            }

            //南以外の方角だけ抽選
            candidateGroups.Add(group);

            bool makeExit = Random.value <= exitChance;
            group.SetExitActive(makeExit);
        }

        if (!guaranteeAtLeastOneExit)
            return;

        bool hasExit = false;

        foreach (RoomExitPatternGroup group in candidateGroups)
        {
            if (group != null && group.enableExit)
            {
                hasExit = true;
                break;
            }
        }

        if (!hasExit && candidateGroups.Count > 0)
        {
            RoomExitPatternGroup forced =
                candidateGroups[Random.Range(0, candidateGroups.Count)];

            forced.SetExitActive(true);

            if (debugLog)
                Debug.Log(room.name + " 最低出口を追加: " + forced.direction);
        }
    }

    void DisableSouth(RoomCell room)
    {
        RoomExitPatternGroup southGroup =
            room.GetExitGroup(RoomDirection.South);

        if (southGroup == null)
            return;

        southGroup.ForceDisable();

        if (debugLog)
            Debug.Log(room.name + " / Southは入口専用なので抽選・扉生成から除外");
    }

    void EnsureExitVisible(RoomCell room, RoomDirection localDirection)
    {
        RoomExitPatternGroup group =
            room.GetExitGroup(localDirection);

        if (group == null)
            return;

        if (!group.enableExit || group.selectedPattern == null)
            group.SetExitActive(true);

        if (group.selectedPattern != null)
            group.selectedPattern.gameObject.SetActive(true);
    }

    public void SpawnDoorsForRoom(RoomCell room)
    {
        if (room == null || room.exitGroups == null)
            return;

        List<DoorController> roomDoors =
            new List<DoorController>();

        foreach (RoomExitPatternGroup group in room.exitGroups)
        {
            if (group == null)
                continue;

            //南側はここでも除外
            if (group.direction == RoomDirection.South)
                continue;

            if (!group.enableExit)
                continue;

            RoomDoorSpawnPoint spawnPoint =
                group.GetSelectedDoorSpawnPoint();

            if (spawnPoint == null)
                continue;

            if (!spawnPoint.CanSpawnDoor())
                continue;

            if (spawnPoint.spawnedDoor != null)
            {
                roomDoors.Add(spawnPoint.spawnedDoor);
                continue;
            }

            GameObject doorPrefab =
                PickRandomDoorPrefab();

            if (doorPrefab == null)
                continue;

            GameObject doorObj =
                Instantiate(
                    doorPrefab,
                    spawnPoint.transform.position,
                    spawnPoint.transform.rotation,
                    room.transform
                );

            DoorController door =
                doorObj.GetComponent<DoorController>();

            spawnPoint.spawnedDoor = door;

            if (door != null)
                roomDoors.Add(door);
        }

        RoomPuzzleState puzzleState =
            room.GetComponent<RoomPuzzleState>();

        if (puzzleState != null)
            puzzleState.roomDoors = roomDoors.ToArray();
    }

    void ApplyWallSwitcher(RoomCell room)
    {
        if (room == null)
            return;

        RoomWallSwitcher switcher =
            room.GetComponent<RoomWallSwitcher>();

        if (switcher != null)
            switcher.Apply(room);
    }

    public void SpawnPuzzleForRoom(RoomCell room)
    {
        if (room == null)
            return;

        RoomPuzzleSpawner spawner =
            room.GetComponent<RoomPuzzleSpawner>();

        if (spawner == null)
            return;

        RoomPuzzleState puzzleState =
            room.GetComponent<RoomPuzzleState>();

        spawner.SpawnPuzzle(puzzlePrefabs, puzzleState);
    }

    public void RefreshRoomDoors(RoomCell room)
    {
        SpawnDoorsForRoom(room);
    }

    void DeleteRoomsFarFrom(Vector2Int centerGrid)
    {
        List<Vector2Int> deleteKeys =
            new List<Vector2Int>();

        foreach (KeyValuePair<Vector2Int, RoomCell> pair in generatedRooms)
        {
            Vector2Int grid = pair.Key;

            if (grid == centerGrid)
                continue;

            int distance =
                Mathf.Abs(grid.x - centerGrid.x)
                + Mathf.Abs(grid.y - centerGrid.y);

            if (distance >= deleteDistance)
                deleteKeys.Add(grid);
        }

        foreach (Vector2Int key in deleteKeys)
        {
            DeleteRoomAt(key);
        }
    }

    void DeleteRoomAt(Vector2Int grid)
    {
        if (!generatedRooms.ContainsKey(grid))
            return;

        RoomCell room = generatedRooms[grid];

        generatedRooms.Remove(grid);

        if (room != null)
        {
            if (debugLog)
                Debug.Log("部屋削除: " + grid + " / " + room.name);

            Destroy(room.gameObject);
        }
    }

    GameObject PickRandomRoomPrefab()
    {
        if (roomPrefabs == null || roomPrefabs.Length <= 0)
        {
            Debug.LogWarning("roomPrefabs が空");
            return null;
        }

        return roomPrefabs[Random.Range(0, roomPrefabs.Length)];
    }

    GameObject PickRandomDoorPrefab()
    {
        if (doorPrefabs == null || doorPrefabs.Length <= 0)
        {
            Debug.LogWarning("doorPrefabs が空");
            return null;
        }

        return doorPrefabs[Random.Range(0, doorPrefabs.Length)];
    }

    RoomDirection LocalDirectionToWorldDirection(Transform roomTransform, RoomDirection localDirection)
    {
        Vector3 localVector =
            DirectionToLocalVector(localDirection);

        Vector3 worldVector =
            roomTransform.TransformDirection(localVector);

        return WorldVectorToDirection(worldVector);
    }

    Vector3 DirectionToLocalVector(RoomDirection direction)
    {
        switch (direction)
        {
            case RoomDirection.North:
                return Vector3.forward;

            case RoomDirection.South:
                return Vector3.back;

            case RoomDirection.East:
                return Vector3.right;

            case RoomDirection.West:
                return Vector3.left;
        }

        return Vector3.zero;
    }

    RoomDirection WorldVectorToDirection(Vector3 worldVector)
    {
        worldVector.y = 0f;
        worldVector.Normalize();

        float dotNorth = Vector3.Dot(worldVector, Vector3.forward);
        float dotSouth = Vector3.Dot(worldVector, Vector3.back);
        float dotEast = Vector3.Dot(worldVector, Vector3.right);
        float dotWest = Vector3.Dot(worldVector, Vector3.left);

        float max = dotNorth;
        RoomDirection result = RoomDirection.North;

        if (dotSouth > max)
        {
            max = dotSouth;
            result = RoomDirection.South;
        }

        if (dotEast > max)
        {
            max = dotEast;
            result = RoomDirection.East;
        }

        if (dotWest > max)
        {
            result = RoomDirection.West;
        }

        return result;
    }

    Quaternion GetRotationForSouthEntrance(RoomDirection worldDirectionFromSource)
    {
        switch (worldDirectionFromSource)
        {
            case RoomDirection.North:
                return Quaternion.Euler(0f, 0f, 0f);

            case RoomDirection.South:
                return Quaternion.Euler(0f, 180f, 0f);

            case RoomDirection.East:
                return Quaternion.Euler(0f, 90f, 0f);

            case RoomDirection.West:
                return Quaternion.Euler(0f, -90f, 0f);
        }

        return Quaternion.identity;
    }

    Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * roomSizeX,
            roomHeight,
            gridPos.y * roomSizeZ
        );
    }

    Vector2Int DirectionToGridOffset(RoomDirection direction)
    {
        switch (direction)
        {
            case RoomDirection.North:
                return northSouthInverted ? Vector2Int.down : Vector2Int.up;

            case RoomDirection.South:
                return northSouthInverted ? Vector2Int.up : Vector2Int.down;

            case RoomDirection.East:
                return eastWestInverted ? Vector2Int.left : Vector2Int.right;

            case RoomDirection.West:
                return eastWestInverted ? Vector2Int.right : Vector2Int.left;
        }

        return Vector2Int.zero;
    }
}
