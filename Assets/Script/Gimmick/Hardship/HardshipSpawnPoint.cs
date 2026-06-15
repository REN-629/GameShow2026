using UnityEngine;

public class HardshipSpawnPoint : MonoBehaviour
{
    public bool canSpawn = true;

    [Range(0f, 1f)]
    public float spawnChance = 0.5f;

    public GameObject spawnedObject;

    public bool ShouldSpawn()
    {
        if (!canSpawn)
            return false;

        if (spawnedObject != null)
            return false;

        return Random.value <= spawnChance;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}
