using UnityEngine;

public class RandomItemSpawner : MonoBehaviour
{
    [Header("スポーンするアイテムPrefab")]
    public GameObject[] itemPrefabs;

    [Header("スポーン数")]
    public int minSpawnCount = 0;
    public int maxSpawnCount = 5;

    [Header("スポーン範囲")]
    public Transform spawnAreaCenter;
    public Vector3 spawnAreaSize = new Vector3(10f, 0f, 10f);

    [Header("高さ補正")]
    public float spawnHeightOffset = 0.5f;

    [Header("地面にRaycastして置く")]
    public bool raycastToGround = true;
    public float raycastHeight = 10f;
    public LayerMask groundMask = ~0;

    [Header("生成先親")]
    public Transform spawnedItemParent;

    [Header("開始時にスポーン")]
    public bool spawnOnStart = true;

    [Header("デバッグ")]
    public bool debugLog = true;

    void Start()
    {
        if (spawnOnStart)
            SpawnRandomItems();
    }

    public void SpawnRandomItems()
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0)
        {
            Debug.LogWarning(name + " itemPrefabs が空です");
            return;
        }

        int min = Mathf.Max(0, minSpawnCount);
        int max = Mathf.Max(min, maxSpawnCount);
        int count = Random.Range(min, max + 1);

        if (debugLog)
            Debug.Log(name + " アイテムスポーン数: " + count);

        for (int i = 0; i < count; i++)
            SpawnOne();
    }

    void SpawnOne()
    {
        GameObject prefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
        if (prefab == null) return;

        Vector3 pos = GetRandomPositionInArea();
        if (raycastToGround)
            pos = AdjustToGround(pos);

        Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        Instantiate(prefab, pos, rot, spawnedItemParent);
    }

    Vector3 GetRandomPositionInArea()
    {
        Vector3 center = spawnAreaCenter != null ? spawnAreaCenter.position : transform.position;
        return center + new Vector3(
            Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f),
            Random.Range(-spawnAreaSize.y * 0.5f, spawnAreaSize.y * 0.5f),
            Random.Range(-spawnAreaSize.z * 0.5f, spawnAreaSize.z * 0.5f)
        );
    }

    Vector3 AdjustToGround(Vector3 pos)
    {
        Vector3 rayStart = pos + Vector3.up * raycastHeight;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundMask))
            return hit.point + Vector3.up * spawnHeightOffset;

        return pos + Vector3.up * spawnHeightOffset;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = spawnAreaCenter != null ? spawnAreaCenter.position : transform.position;
        Gizmos.DrawWireCube(center, spawnAreaSize);
    }
}
