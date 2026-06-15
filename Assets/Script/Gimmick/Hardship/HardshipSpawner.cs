using UnityEngine;

public class HardshipSpawner : MonoBehaviour
{
    public GameObject[] hardshipPrefabs;
    public HardshipSpawnPoint[] spawnPoints;
    public int maxSpawnCount = 2;
    public bool spawnOnStart = false;
    public bool debugLog = true;

    void Reset()
    {
        spawnPoints = GetComponentsInChildren<HardshipSpawnPoint>(true);
    }

    void Start()
    {
        if (spawnOnStart)
            SpawnHardships();
    }

    public void SpawnHardships()
    {
        if (hardshipPrefabs == null || hardshipPrefabs.Length == 0)
            return;

        if (spawnPoints == null || spawnPoints.Length == 0)
            spawnPoints = GetComponentsInChildren<HardshipSpawnPoint>(true);

        int count = 0;

        foreach (HardshipSpawnPoint point in spawnPoints)
        {
            if (point == null)
                continue;

            if (count >= maxSpawnCount)
                break;

            if (!point.ShouldSpawn())
                continue;

            GameObject prefab = hardshipPrefabs[Random.Range(0, hardshipPrefabs.Length)];

            if (prefab == null)
                continue;

            GameObject obj = Instantiate(prefab, point.transform.position, point.transform.rotation, transform);
            point.spawnedObject = obj;
            count++;

            if (debugLog)
                Debug.Log("苦難生成: " + prefab.name);
        }
    }
}
