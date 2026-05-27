//部屋内にランダムにパズルを生成する


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

    public void SpawnPuzzle(GameObject[] puzzlePrefabs, RoomPuzzleState targetRoom)
    {
        if (puzzlePrefabs == null || puzzlePrefabs.Length == 0)
        {
            Debug.LogWarning(name + " puzzlePrefabsが空です");
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

            GameObject prefab = puzzlePrefabs[Random.Range(0, puzzlePrefabs.Length)];

            GameObject obj =
                Instantiate(
                    prefab,
                    point.transform.position,
                    point.transform.rotation,
                    transform
                );

            point.spawnedPuzzle = obj;

            // パズル側が特定の部屋を直接参照したい場合用
            RoomPuzzleTarget target = obj.GetComponent<RoomPuzzleTarget>();

            if (target != null)
                target.targetRoom = targetRoom;

            spawnedCount++;

            if (debugLog)
                Debug.Log("パズル生成: " + prefab.name);
        }
    }
}
