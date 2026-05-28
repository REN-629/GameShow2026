using System.Collections.Generic;
using UnityEngine;

// InfiniteRoomGenerator
//
// 超単純化版:
// ・Southは入口専用として完全除外
// ・出口抽選はNorth/East/Westだけ
// ・最低1つ出口をNorth/East/Westから作る
// ・部屋生成もNorth/East/Westの出口がある方向だけ
// ・新部屋はSouthが接続元を向くように回転生成
// ・Southにはドアを生成しない
//
// 注意:
// 部屋Prefab側ではSouthに入口穴/入口通路を固定で作る。
// South側をランダム壁やランダム扉として扱わない。
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

    [Header("デバッグ")]
    public bool debugLog = true;

    private Dictionary<Vector2Int, RoomCell> generatedRooms =
        new Dictionary<Vector2Int, RoomCell>();

    void Start()
    {
        if (startRoom != null)
        {
            startRoom.gridPosition = Vector2Int.zero;
            startRoom.generator = this;

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

        // Southは入口専用なので生成方向から除外
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

        if (generatedRooms.ContainsKey(targetGrid))
        {
            EnsureExitVisible(fromRoom, localExitDirection);
            RefreshRoomDoors(fromRoom);
            ApplyWallSwitcher(fromRoom);

            if (debugLog)
            {
                Debug.Log(
                    "既存部屋あり: "
                    + targetGrid
                    + " / localExit="
                    + localExitDirection
                    + " / worldDir="
                    + worldDirection
                );
            }

            return;
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

        if (newRoom.exitGroups == null || newRoom.exitGroups.Length == 0)
            newRoom.exitGroups =
                newRoom.GetComponentsInChildren<RoomExitPatternGroup>(true);

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

            // Southは完全に入口専用なので抽選から排除
            if (group.direction == RoomDirection.South)
            {
                group.ForceDisable();
                continue;
            }

            // North/East/Westだけ抽選
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
                return Quaternion.Euler(0f, -90f, 0f);

            case RoomDirection.West:
                return Quaternion.Euler(0f, 90f, 0f);
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
                return Vector2Int.up;

            case RoomDirection.South:
                return Vector2Int.down;

            case RoomDirection.East:
                return Vector2Int.right;

            case RoomDirection.West:
                return Vector2Int.left;
        }

        return Vector2Int.zero;
    }
}
