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

    [Header("出口設定")]
    [Range(0f, 1f)]
    public float exitChance = 0.5f;

    public bool guaranteeAtLeastOneExit = true;

    [Header("接続ドア設定")]
    public bool preventDoubleDoors = true;
    public bool blockDoorOnConnectedSide = true;
    public bool hideConnectedSideExitPattern = true;

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

            SetupRoom(startRoom, null);
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

        TryGenerateDirection(centerRoom, RoomDirection.North);
        TryGenerateDirection(centerRoom, RoomDirection.South);
        TryGenerateDirection(centerRoom, RoomDirection.East);
        TryGenerateDirection(centerRoom, RoomDirection.West);

        RefreshRoomDoors(centerRoom);
        ApplyWallSwitcher(centerRoom);
    }

    void TryGenerateDirection(RoomCell fromRoom, RoomDirection direction)
    {
        RoomExitPatternGroup fromGroup = fromRoom.GetExitGroup(direction);

        if (fromGroup == null || !fromGroup.enableExit)
            return;

        Vector2Int targetGrid =
            fromRoom.gridPosition + DirectionToGridOffset(direction);

        if (generatedRooms.ContainsKey(targetGrid))
        {
            RoomCell existingRoom = generatedRooms[targetGrid];

            ConnectRooms(fromRoom, existingRoom, direction);

            RefreshRoomDoors(fromRoom);
            RefreshRoomDoors(existingRoom);

            ApplyWallSwitcher(fromRoom);
            ApplyWallSwitcher(existingRoom);

            return;
        }

        GameObject prefab = PickRandomRoomPrefab();

        if (prefab == null)
            return;

        GameObject obj =
            Instantiate(prefab, GridToWorld(targetGrid), Quaternion.identity);

        RoomCell newRoom = obj.GetComponent<RoomCell>();

        if (newRoom == null)
        {
            Debug.LogError(prefab.name + " に RoomCell がありません");
            return;
        }

        newRoom.gridPosition = targetGrid;
        newRoom.generator = this;

        if (newRoom.exitGroups == null || newRoom.exitGroups.Length == 0)
            newRoom.exitGroups = newRoom.GetComponentsInChildren<RoomExitPatternGroup>(true);

        RegisterRoom(newRoom);

        RoomDirection newRoomConnectedSide = GetOpposite(direction);

        SetupRoom(newRoom, newRoomConnectedSide);

        EnsureExitVisible(fromRoom, direction);

        RefreshRoomDoors(fromRoom);
        ApplyWallSwitcher(fromRoom);

        if (debugLog)
        {
            Debug.Log(
                "部屋生成: "
                + targetGrid
                + " / from="
                + fromRoom.gridPosition
                + " / dir="
                + direction
                + " / newRoomConnectedSide="
                + newRoomConnectedSide
            );
        }
    }

    void SetupRoom(RoomCell room, RoomDirection? connectedSide)
    {
        DecideExits(room, connectedSide);

        if (connectedSide.HasValue)
            BlockConnectionSide(room, connectedSide.Value);

        SpawnDoorsForRoom(room);
        ApplyWallSwitcher(room);
        SpawnPuzzleForRoom(room);
    }

    void DecideExits(RoomCell room, RoomDirection? connectedSide)
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

            if (connectedSide.HasValue && group.direction == connectedSide.Value)
            {
                group.SetExitActive(true);
                continue;
            }

            candidateGroups.Add(group);

            bool makeExit = Random.value <= exitChance;
            group.SetExitActive(makeExit);
        }

        if (!guaranteeAtLeastOneExit)
            return;

        bool hasOtherExit = false;

        foreach (RoomExitPatternGroup group in candidateGroups)
        {
            if (group != null && group.enableExit)
            {
                hasOtherExit = true;
                break;
            }
        }

        if (!hasOtherExit && candidateGroups.Count > 0)
        {
            RoomExitPatternGroup forced =
                candidateGroups[Random.Range(0, candidateGroups.Count)];

            forced.SetExitActive(true);

            if (debugLog)
            {
                Debug.Log(
                    room.name
                    + " は出口が無かったので最低1つ出口を追加: "
                    + forced.direction
                );
            }
        }
    }

    void ConnectRooms(RoomCell fromRoom, RoomCell targetRoom, RoomDirection directionFromRoom)
    {
        if (fromRoom == null || targetRoom == null)
            return;

        RoomDirection targetConnectedSide = GetOpposite(directionFromRoom);

        EnsureExitVisible(fromRoom, directionFromRoom);
        EnsureExitVisible(targetRoom, targetConnectedSide);

        BlockConnectionSide(targetRoom, targetConnectedSide);

        if (debugLog)
        {
            Debug.Log(
                "既存部屋接続: "
                + fromRoom.gridPosition
                + " "
                + directionFromRoom
                + " -> "
                + targetRoom.gridPosition
                + " / targetSide="
                + targetConnectedSide
            );
        }
    }

    void EnsureExitVisible(RoomCell room, RoomDirection direction)
    {
        RoomExitPatternGroup group = room.GetExitGroup(direction);

        if (group == null)
            return;

        if (!group.enableExit || group.selectedPattern == null)
            group.SetExitActive(true);

        if (group.selectedPattern != null)
            group.selectedPattern.gameObject.SetActive(true);
    }

    void BlockConnectionSide(RoomCell room, RoomDirection direction)
    {
        if (!preventDoubleDoors)
            return;

        RoomExitPatternGroup group = room.GetExitGroup(direction);

        if (group == null)
            return;

        if (!group.enableExit || group.selectedPattern == null)
            group.SetExitActive(true);

        RoomDoorSpawnPoint spawnPoint =
            group.GetSelectedDoorSpawnPoint();

        if (spawnPoint != null && blockDoorOnConnectedSide)
            spawnPoint.BlockDoorByConnection();

        if (hideConnectedSideExitPattern && group.selectedPattern != null)
            group.selectedPattern.gameObject.SetActive(false);

        if (debugLog)
            Debug.Log(room.name + " / " + direction + " は接続済み側として処理");
    }

    public void SpawnDoorsForRoom(RoomCell room)
    {
        if (room == null || room.exitGroups == null)
            return;

        List<DoorController> roomDoors =
            new List<DoorController>();

        foreach (RoomExitPatternGroup group in room.exitGroups)
        {
            if (group == null || !group.enableExit)
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

            GameObject doorPrefab = PickRandomDoorPrefab();

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

    RoomDirection GetOpposite(RoomDirection direction)
    {
        switch (direction)
        {
            case RoomDirection.North:
                return RoomDirection.South;
            case RoomDirection.South:
                return RoomDirection.North;
            case RoomDirection.East:
                return RoomDirection.West;
            case RoomDirection.West:
                return RoomDirection.East;
        }

        return RoomDirection.North;
    }
}
