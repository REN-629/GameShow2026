// 部屋が無限に続くように見せる
// South入口専用 + 部屋レベル管理 + Trigger到達スコア対応版

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
    public bool overwriteExistingRoomOnGenerate = false;

    [Header("Level親管理")]
    [Tooltip("ONならLevel_2などの空オブジェクトの子に部屋をまとめる")]
    public bool groupRoomsByLevel = true;

    [Tooltip("生成部屋をまとめる親")]
    public Transform generatedRoomsRoot;

    [Header("Level補正")]
    [Tooltip("ONなら、生成元RoomIdentityが壊れていても親Levelから補正する")]
    public bool fixRoomLevelBeforeGenerate = true;

    [Header("未来衝突チェック")]
    [Tooltip("ONなら、この出口の先に部屋を作った場合に、さらにその部屋の出口候補が既存部屋へ衝突しそうな方向を抽選から外す")]
    public bool preventFutureRoomOverlap = true;

    [Header("デバッグ")]
    public bool debugLog = true;

    private Dictionary<Vector2Int, RoomCell> generatedRooms =
        new Dictionary<Vector2Int, RoomCell>();

    private Dictionary<int, Transform> levelParents =
        new Dictionary<int, Transform>();

    void Start()
    {
        if (generatedRoomsRoot == null)
        {
            GameObject root = new GameObject("GeneratedRooms");
            generatedRoomsRoot = root.transform;
        }

        if (startRoom != null)
        {
            startRoom.gridPosition = Vector2Int.zero;
            startRoom.generator = this;

            RoomIdentity identity = startRoom.GetComponent<RoomIdentity>();

            if (identity != null)
            {
                identity.SetupId(startRoom.gridPosition, 1, 0);
            }

            if (groupRoomsByLevel)
            {
                Transform levelParent = GetOrCreateLevelParent(1);
                startRoom.transform.SetParent(levelParent, true);
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

        FixRoomIdentityIfNeeded(room);

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

        if (deleteFarRooms)
            DeleteRoomsFarFrom(centerRoom.gridPosition);

        // Southは入口専用なので生成方向から除外
        TryGenerateFromLocalExit(centerRoom, RoomDirection.North);
        TryGenerateFromLocalExit(centerRoom, RoomDirection.East);
        TryGenerateFromLocalExit(centerRoom, RoomDirection.West);

        RefreshRoomDoors(centerRoom);
        ApplyWallSwitcher(centerRoom);
    }

    int GetRoomLevelSafe(RoomCell room)
    {
        if (room == null)
            return 1;

        RoomIdentity identity = room.GetComponent<RoomIdentity>();

        if (identity != null && identity.roomLevel > 0)
            return identity.roomLevel;

        // 親がLevel_数字ならそこから補正
        Transform parent = room.transform.parent;

        if (parent != null)
        {
            RoomLevelGroup group = parent.GetComponent<RoomLevelGroup>();

            if (group != null && group.roomLevel > 0)
                return group.roomLevel;

            if (parent.name.StartsWith("Level_"))
            {
                string number = parent.name.Replace("Level_", "");

                if (int.TryParse(number, out int parsedLevel))
                    return parsedLevel;
            }
        }

        return 1;
    }

    void FixRoomIdentityIfNeeded(RoomCell room)
    {
        if (!fixRoomLevelBeforeGenerate)
            return;

        if (room == null)
            return;

        RoomIdentity identity = room.GetComponent<RoomIdentity>();

        if (identity == null)
            return;

        int safeLevel = GetRoomLevelSafe(room);

        if (identity.roomLevel != safeLevel || string.IsNullOrEmpty(identity.roomId))
        {
            int parentLevel = Mathf.Max(0, safeLevel - 1);
            identity.SetupId(room.gridPosition, safeLevel, parentLevel);

            if (debugLog)
            {
                Debug.Log(
                    room.name
                    + " のRoomIdentityを補正: level="
                    + safeLevel
                    + " / grid="
                    + room.gridPosition
                );
            }
        }
    }

    void TryGenerateFromLocalExit(RoomCell fromRoom, RoomDirection localExitDirection)
    {
        if (fromRoom == null)
            return;

        FixRoomIdentityIfNeeded(fromRoom);

        RoomExitPatternGroup fromGroup =
            fromRoom.GetExitGroup(localExitDirection);

        if (fromGroup == null || !fromGroup.enableExit)
            return;

        RoomDirection worldDirection =
            LocalDirectionToWorldDirection(fromRoom.transform, localExitDirection);

        Vector2Int targetGrid =
            fromRoom.gridPosition + DirectionToGridOffset(worldDirection);

        // Level親ズレ対策:
        // fromRoomのRoomIdentityが未設定/古い場合でも、親Levelから安全に補正して使う
        int parentLevel =
            GetRoomLevelSafe(fromRoom);

        int nextLevel =
            parentLevel + 1;

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

        GameObject prefab =
            PickRandomRoomPrefab();

        if (prefab == null)
            return;

        Quaternion rotation =
            GetRotationForSouthEntrance(worldDirection);

        Transform parent =
            groupRoomsByLevel ? GetOrCreateLevelParent(nextLevel) : generatedRoomsRoot;

        GameObject obj =
            Instantiate(prefab, GridToWorld(targetGrid), rotation, parent);

        RoomCell newRoom =
            obj.GetComponent<RoomCell>();

        if (newRoom == null)
        {
            Debug.LogError(prefab.name + " に RoomCell がありません");
            return;
        }

        newRoom.gridPosition = targetGrid;
        newRoom.generator = this;

        RoomIdentity newIdentity =
            newRoom.GetComponent<RoomIdentity>();

        if (newIdentity != null)
        {
            newIdentity.SetupId(targetGrid, nextLevel, parentLevel);
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
                + " / level="
                + nextLevel
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

            // Southは完全に入口専用なので抽選から排除
            if (group.direction == RoomDirection.South)
            {
                group.ForceDisable();
                continue;
            }

            if (!CanUseExitCandidate(room, group.direction))
            {
                group.ForceDisable();
                continue;
            }

            // North/East/Westだけ抽選
            candidateGroups.Add(group);

            bool makeExit =
                Random.value <= exitChance;

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

            // Southはここでも除外
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

        CleanupEmptyLevelParents();
    }

    void DeleteRoomAt(Vector2Int grid)
    {
        if (!generatedRooms.ContainsKey(grid))
            return;

        RoomCell room =
            generatedRooms[grid];

        generatedRooms.Remove(grid);

        if (room != null)
        {
            if (debugLog)
                Debug.Log("部屋削除: " + grid + " / " + room.name);

            Destroy(room.gameObject);
        }
    }

    void CleanupEmptyLevelParents()
    {
        List<int> removeKeys =
            new List<int>();

        foreach (KeyValuePair<int, Transform> pair in levelParents)
        {
            Transform parent = pair.Value;

            if (parent == null)
            {
                removeKeys.Add(pair.Key);
                continue;
            }

            if (parent.childCount <= 0)
            {
                removeKeys.Add(pair.Key);
                Destroy(parent.gameObject);
            }
        }

        foreach (int key in removeKeys)
        {
            levelParents.Remove(key);
        }
    }

    Transform GetOrCreateLevelParent(int level)
    {
        if (levelParents.ContainsKey(level) && levelParents[level] != null)
            return levelParents[level];

        if (generatedRoomsRoot == null)
        {
            GameObject root = new GameObject("GeneratedRooms");
            generatedRoomsRoot = root.transform;
        }

        GameObject obj = new GameObject("Level_" + level);
        obj.transform.SetParent(generatedRoomsRoot, false);

        RoomLevelGroup group =
            obj.AddComponent<RoomLevelGroup>();

        group.Setup(level);

        levelParents.Add(level, obj.transform);

        return obj.transform;
    }

    bool CanUseExitCandidate(RoomCell room, RoomDirection localExitDirection)
    {
        if (!preventFutureRoomOverlap)
            return true;

        if (room == null)
            return false;

        if (localExitDirection == RoomDirection.South)
            return false;

        if (WouldFutureBranchOverlap(room, localExitDirection))
        {
            if (debugLog)
            {
                RoomDirection worldDirection =
                    LocalDirectionToWorldDirection(room.transform, localExitDirection);

                Vector2Int futureGrid =
                    room.gridPosition + DirectionToGridOffset(worldDirection);

                Debug.Log(
                    room.name
                    + " / "
                    + localExitDirection
                    + " は未来衝突の可能性があるため出口候補から除外 / futureGrid="
                    + futureGrid
                );
            }

            return false;
        }

        return true;
    }

    bool WouldFutureBranchOverlap(RoomCell room, RoomDirection localExitDirection)
    {
        if (room == null)
            return false;

        RoomDirection worldDirection =
            LocalDirectionToWorldDirection(room.transform, localExitDirection);

        Vector2Int futureGrid =
            room.gridPosition + DirectionToGridOffset(worldDirection);

        if (generatedRooms.ContainsKey(futureGrid))
            return true;

        Quaternion futureRotation =
            GetRotationForSouthEntrance(worldDirection);

        RoomDirection[] futureLocalExits =
        {
            RoomDirection.North,
            RoomDirection.East,
            RoomDirection.West
        };

        foreach (RoomDirection futureLocalExit in futureLocalExits)
        {
            RoomDirection futureWorldDirection =
                LocalDirectionToWorldDirection(futureRotation, futureLocalExit);

            Vector2Int checkGrid =
                futureGrid + DirectionToGridOffset(futureWorldDirection);

            if (checkGrid == room.gridPosition)
                continue;

            if (generatedRooms.ContainsKey(checkGrid))
                return true;
        }

        return false;
    }

    RoomDirection LocalDirectionToWorldDirection(Quaternion rotation, RoomDirection localDirection)
    {
        Vector3 localVector =
            DirectionToLocalVector(localDirection);

        Vector3 worldVector =
            rotation * localVector;

        return WorldVectorToDirection(worldVector);
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

        float dotNorth =
            Vector3.Dot(worldVector, Vector3.forward);

        float dotSouth =
            Vector3.Dot(worldVector, Vector3.back);

        float dotEast =
            Vector3.Dot(worldVector, Vector3.right);

        float dotWest =
            Vector3.Dot(worldVector, Vector3.left);

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
