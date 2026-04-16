// ワールドアイテム：拾われる実体としてデータと表示状態と使用設定を管理
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public ItemData itemData;

    [Header("使用設定")]
    public bool canUseWithLeftClick = false;

    [Header("回転設定")]
    public float rotateSpeed = 3f;

    [Header("手持ち位置補正")]
    public Vector3 holdPositionOffset = Vector3.zero;
    public Vector3 holdRotationOffset = Vector3.zero;

    private Rigidbody rb;
    private Collider[] colliders;
    private Renderer[] renderers;

    void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>(true);
        colliders = GetComponentsInChildren<Collider>(true);
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void SetStoredState()
    {
        transform.SetParent(null);

        SetVisible(false);
        SetColliders(false);

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.detectCollisions = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void SetHeldState()
    {
        transform.SetParent(null);

        SetVisible(true);
        SetColliders(false);

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.detectCollisions = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void DropToWorld(Vector3 position, Quaternion rotation)
    {
        transform.SetParent(null);
        transform.position = position;
        transform.rotation = rotation;

        SetVisible(true);
        SetColliders(true);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.detectCollisions = true;
        }
    }

    public void ThrowToWorld(Vector3 position, Quaternion rotation, Vector3 force)
    {
        DropToWorld(position, rotation);

        if (rb != null)
        {
            rb.AddForce(force, ForceMode.Impulse);
        }
    }

    public void Use()
    {
        if (!canUseWithLeftClick)
            return;

        Debug.Log(itemData.itemName + " を使用した");
    }

    void SetVisible(bool visible)
    {
        foreach (Renderer r in renderers)
        {
            if (r != null)
                r.enabled = visible;
        }
    }

    void SetColliders(bool enabled)
    {
        foreach (Collider c in colliders)
        {
            if (c != null)
                c.enabled = enabled;
        }
    }
}