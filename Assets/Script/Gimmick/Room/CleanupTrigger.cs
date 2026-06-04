using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CleanupTrigger : MonoBehaviour
{
    public bool useTagFilter = true;
    public string[] allowedTags = { "Item", "Debris", "Fragment" };

    public bool ignorePlayer = true;
    public string playerTag = "Player";

    public bool destroyRootObject = true;
    public bool debugLog = false;

    void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (ignorePlayer && other.CompareTag(playerTag))
            return;

        GameObject target = GetDestroyTarget(other);

        if (target == null)
            return;

        if (!CanDestroy(target, other))
            return;

        if (debugLog)
            Debug.Log("Cleanup削除: " + target.name);

        Destroy(target);
    }

    GameObject GetDestroyTarget(Collider other)
    {
        if (!destroyRootObject)
            return other.gameObject;

        PickupItem pickup = other.GetComponentInParent<PickupItem>();

        if (pickup != null)
            return pickup.gameObject;

        Rigidbody rb = other.attachedRigidbody;

        if (rb != null)
            return rb.gameObject;

        return other.gameObject;
    }

    bool CanDestroy(GameObject target, Collider originalCollider)
    {
        if (!useTagFilter)
            return true;

        foreach (string tag in allowedTags)
        {
            if (string.IsNullOrEmpty(tag))
                continue;

            if (target.CompareTag(tag) || originalCollider.CompareTag(tag))
                return true;
        }

        return false;
    }
}
