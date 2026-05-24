// InfiniteRoomGenerator：無限部屋生成の本体
//
// 仕様:
// ・現在部屋の東西南北に、未生成なら部屋を1つずつ生成
// ・部屋Prefabはランダム
// ・出口パターンもランダム
// ・DoorSpawnPointに扉Prefabをランダム生成
// ・PuzzleSpawnPointにパズルPrefabをランダム生成
// ・生成された扉はRoomPuzzleState.roomDoorsへ自動登録

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

    [Header("生成設定")]
    public bool forceGenerateAllDirections = true;

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
    }

    void TryGenerateDirection(RoomCell fromRoom, RoomDirection direction)
    {
        Vector2Int targetGrid =
            fromRoom.gridPosition + DirectionToGridOffset(direction);

        if (generatedRooms.ContainsKey(targetGrid))
            return;

        if (!forceGenerateAllDirections)
        {
            RoomExitPatternGroup fromGroup = fromRoom.GetExitGroup(direction);

            if (fromGroup == null || !fromGroup.enableExit)
                return;
        }

        GameObject prefab = PickRandomRoomPrefab();

        if (prefab == null)
            return;

        GameObject obj = Instantiate(prefab, GridToWorld(targetGrid), Quaternion.identity);

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

        RoomDirection cameFrom = GetOpposite(direction);

        SetupRoom(newRoom, cameFrom);

        if (debugLog)
        {
            Debug.Log(
                "部屋生成: " + targetGrid
                + " / from=" + fromRoom.gridPosition
                + " / dir=" + direction
                + " / prefab=" + prefab.name
            );
        }
    }

    void SetupRoom(RoomCell room, RoomDirection? cameFromDirection)
    {
        SetupRoomExitPatterns(room, cameFromDirection);
        SpawnDoorsForRoom(room);
        SpawnPuzzleForRoom(room);
    }

    void SetupRoomExitPatterns(RoomCell room, RoomDirection? cameFromDirection)
    {
        if (room == null || room.exitGroups == null)
            return;

        foreach (RoomExitPatternGroup group in room.exitGroups)
        {
            if (group == null)
                continue;

            if (cameFromDirection.HasValue && group.direction == cameFromDirection.Value)
            {
                group.enableExit = true;
                group.SelectRandomPattern();
                continue;
            }

            group.SelectRandomPattern();
        }
    }

    public void SpawnDoorsForRoom(RoomCell room)
    {
        if (room == null || room.exitGroups == null)
            return;

        List<DoorController> roomDoors = new List<DoorController>();

        foreach (RoomExitPatternGroup group in room.exitGroups)
        {
            if (group == null)
                continue;

            RoomDoorSpawnPoint spawnPoint = group.GetSelectedDoorSpawnPoint();

            if (spawnPoint == null)
                continue;

            if (!spawnPoint.spawnDoor)
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

            DoorController door = doorObj.GetComponent<DoorController>();

            spawnPoint.spawnedDoor = door;

            if (door != null)
                roomDoors.Add(door);
        }

        RoomPuzzleState puzzleState = room.GetComponent<RoomPuzzleState>();

        if (puzzleState != null)
            puzzleState.roomDoors = roomDoors.ToArray();
    }

    public void SpawnPuzzleForRoom(RoomCell room)
    {
        if (room == null)
            return;

        RoomPuzzleSpawner spawner = room.GetComponent<RoomPuzzleSpawner>();

        if (spawner == null)
            return;

        RoomPuzzleState puzzleState = room.GetComponent<RoomPuzzleState>();

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
