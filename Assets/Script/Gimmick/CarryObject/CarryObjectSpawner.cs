using UnityEngine;

public class CarryObjectSpawner : MonoBehaviour
{
    public GameObject[] carryObjectPrefabs;
    public CarryObjectSpawnPoint[] spawnPoints;

    public int minSpawnCount = 1;
    public int maxSpawnCount = 1;

    public bool spawnOnStart = true;
    public bool debugLog = true;

    void Reset()
    {
        spawnPoints = GetComponentsInChildren<CarryObjectSpawnPoint>(true);
    }

    void Start()
    {
        if (spawnOnStart)
            SpawnObjects();
    }

    public void SpawnObjects()
    {
        if (carryObjectPrefabs == null || carryObjectPrefabs.Length == 0)
            return;

        if (spawnPoints == null || spawnPoints.Length == 0)
            spawnPoints = GetComponentsInChildren<CarryObjectSpawnPoint>(true);

        int targetCount = Random.Range(minSpawnCount, maxSpawnCount + 1);
        int spawned = 0;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawned >= targetCount)
                break;

            CarryObjectSpawnPoint point = spawnPoints[i];

            if (point == null || !point.CanSpawn())
                continue;

            GameObject prefab = carryObjectPrefabs[Random.Range(0, carryObjectPrefabs.Length)];

            if (prefab == null)
                continue;

            GameObject obj = Instantiate(prefab, point.transform.position, point.transform.rotation, transform);
            point.spawnedObject = obj;
            spawned++;

            if (debugLog)
                Debug.Log("CarryObject生成: " + prefab.name);
        }
    }
}
