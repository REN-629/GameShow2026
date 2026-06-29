using UnityEngine;

public class CarryObjectSpawnPoint : MonoBehaviour
{
    public bool canSpawn = true;
    public GameObject spawnedObject;

    public bool CanSpawn()
    {
        return canSpawn && spawnedObject == null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }
}
