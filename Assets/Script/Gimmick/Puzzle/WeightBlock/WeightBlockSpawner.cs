using UnityEngine;

public class WeightBlockSpawner : MonoBehaviour
{
    [Header("生成する重りPrefab")]
    public GameObject[] blockPrefabs;

    [Header("生成ポイント")]
    public WeightBlockSpawnPoint[] spawnPoints;

    [Header("生成数")]
    public int minSpawnCount = 1;
    public int maxSpawnCount = 1;

    [Header("開始時に生成")]
    public bool spawnOnStart = true;

    [Header("デバッグ")]
    public bool debugLog = true;

    void Reset()
    {
        spawnPoints = GetComponentsInChildren<WeightBlockSpawnPoint>(true);
    }

    void Start()
    {
        if (spawnOnStart)
            SpawnBlocks();
    }

    public void SpawnBlocks()
    {
        if (blockPrefabs == null || blockPrefabs.Length == 0)
            return;

        if (spawnPoints == null || spawnPoints.Length == 0)
            spawnPoints = GetComponentsInChildren<WeightBlockSpawnPoint>(true);

        if (spawnPoints == null || spawnPoints.Length == 0)
            return;

        int count = Random.Range(minSpawnCount, maxSpawnCount + 1);
        int spawned = 0;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawned >= count)
                break;

            WeightBlockSpawnPoint point = spawnPoints[i];

            if (point == null || !point.CanSpawn())
                continue;

            GameObject prefab = blockPrefabs[Random.Range(0, blockPrefabs.Length)];

            if (prefab == null)
                continue;

            GameObject obj = Instantiate(
                prefab,
                point.transform.position,
                point.transform.rotation,
                transform
            );

            point.spawnedObject = obj;
            spawned++;

            if (debugLog)
                Debug.Log("重りブロック生成: " + prefab.name);
        }
    }
}
