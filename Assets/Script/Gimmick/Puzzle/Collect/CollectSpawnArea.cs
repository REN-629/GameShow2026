using UnityEngine;

public class CollectSpawnArea : MonoBehaviour
{
    public Vector3 GetRandomPosition()
    {
        Vector3 size = transform.lossyScale;

        return transform.position + new Vector3(
            Random.Range(-size.x * 0.5f, size.x * 0.5f),
            Random.Range(-size.y * 0.5f, size.y * 0.5f),
            Random.Range(-size.z * 0.5f, size.z * 0.5f)
        );
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = old;
    }
}
