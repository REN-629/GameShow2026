using UnityEngine;

public class RoomPuzzleSpawner : MonoBehaviour
{
    [Header("パズル生成位置")]
    public RoomPuzzleSpawnPoint[] spawnPoints;

    [Header("1部屋に生成するパズル数")]
    public int puzzleCount = 1;

    [Header("デバッグ")]
    public bool debugLog = true;

    void Reset()
    {
        spawnPoints = GetComponentsInChildren<RoomPuzzleSpawnPoint>(true);
    }

    public void SpawnPuzzle(GameObject[] globalPuzzlePrefabs, RoomPuzzleState targetRoom)
    {
        if (!GameModeManager.UsesRandomPuzzles)
            return;

        GameObject[] roomPuzzlePrefabs = GetRoomPuzzlePrefabs(globalPuzzlePrefabs);

        if (roomPuzzlePrefabs == null || roomPuzzlePrefabs.Length == 0)
        {
            if (debugLog)
                Debug.LogWarning(name + " 生成可能なPuzzleがありません");

            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
            spawnPoints = GetComponentsInChildren<RoomPuzzleSpawnPoint>(true);

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning(name + " PuzzleSpawnPointがありません");
            return;
        }

        int spawnedCount = 0;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnedCount >= puzzleCount)
                break;

            RoomPuzzleSpawnPoint point = spawnPoints[i];

            if (point == null)
                continue;

            if (!point.spawnPuzzle)
                continue;

            if (point.spawnedPuzzle != null)
                continue;

            GameObject[] usablePrefabs = GetSpawnPointPuzzlePrefabs(point, roomPuzzlePrefabs);

            if (usablePrefabs == null || usablePrefabs.Length == 0)
                continue;

            GameObject prefab = PickRandomPuzzlePrefab(usablePrefabs);

            if (prefab == null)
                continue;

            GameObject obj = Instantiate(
                prefab,
                point.transform.position,
                point.transform.rotation,
                transform
            );

            point.spawnedPuzzle = obj;

            RoomPuzzleTarget[] targets = obj.GetComponentsInChildren<RoomPuzzleTarget>(true);

            foreach (RoomPuzzleTarget target in targets)
            {
                if (target != null)
                    target.targetRoom = targetRoom;
            }

            spawnedCount++;

            if (debugLog)
                Debug.Log("パズル生成: " + prefab.name);
        }
    }

    GameObject[] GetRoomPuzzlePrefabs(GameObject[] globalPuzzlePrefabs)
    {
        RoomPuzzleSet puzzleSet = GetComponent<RoomPuzzleSet>();

        if (puzzleSet == null)
            puzzleSet = GetComponentInParent<RoomPuzzleSet>();

        if (puzzleSet == null)
            return globalPuzzlePrefabs;

        return puzzleSet.GetPuzzlePrefabs(globalPuzzlePrefabs);
    }

    GameObject[] GetSpawnPointPuzzlePrefabs(RoomPuzzleSpawnPoint point, GameObject[] roomPuzzlePrefabs)
    {
        RoomPuzzleSpawnPointFilter filter =
            point.GetComponent<RoomPuzzleSpawnPointFilter>();

        if (filter == null)
            return roomPuzzlePrefabs;

        return filter.Filter(roomPuzzlePrefabs);
    }

    GameObject PickRandomPuzzlePrefab(GameObject[] puzzlePrefabs)
    {
        if (puzzlePrefabs == null || puzzlePrefabs.Length == 0)
            return null;

        return puzzlePrefabs[Random.Range(0, puzzlePrefabs.Length)];
    }
}
