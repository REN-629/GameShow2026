using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollectFragment : MonoBehaviour
{
    public CollectPuzzleManager manager;
    public string playerTag = "Player";

    [Header("取得後")]
    public bool destroyOnCollect = true;

    private bool collected;

    void Awake()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
            col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        if (!other.CompareTag(playerTag))
            return;

        collected = true;

        if (manager != null)
            manager.CollectFragment(this);

        if (destroyOnCollect)
            Destroy(gameObject);
    }
}
