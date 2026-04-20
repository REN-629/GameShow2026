using UnityEngine;
// ワールドアイテム：実体管理、手持ち表示、回転情報を管理
public class PickupItem : MonoBehaviour
{
    public ItemData itemData;
    [Header("使用設定")]
    public bool canUseWithLeftClick = false;
    [Header("回転設定")]
    public float rotateSpeed = 3f;
    public Transform rotationPivot;
    [Header("手持ち基準")]
    public Vector3 holdLocalPositionOffset = Vector3.zero;
    public Vector3 holdLocalRotationOffset = Vector3.zero;
    private Vector3 currentHoldRotationOffset;
    private Rigidbody rb;
    private Collider[] colliders;
    private Renderer[] renderers;
    void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>(true);
        colliders = GetComponentsInChildren<Collider>(true);
        renderers = GetComponentsInChildren<Renderer>(true);
        ResetHoldRotation();
    }
    public void ResetHoldRotation()
    {
        currentHoldRotationOffset = holdLocalRotationOffset;
    }
    public Vector3 GetCurrentHoldRotationOffset()
    {
        return currentHoldRotationOffset;
    }
    public void AddHoldRotation(Vector3 delta)
    {
        currentHoldRotationOffset += delta;
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
        ResetHoldRotation();
    }
    public void SetHeldState(Transform holdPoint)
    {
        transform.SetParent(holdPoint, false);
        transform.localPosition = holdLocalPositionOffset;
        transform.localRotation = Quaternion.Euler(currentHoldRotationOffset);
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
    public void ForceHoldTransform(Transform holdPoint)
    {
        transform.SetParent(holdPoint, false);
        transform.localPosition = holdLocalPositionOffset;
        transform.localRotation = Quaternion.Euler(currentHoldRotationOffset);
        if (rb != null)
        {
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
        ResetHoldRotation();
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
    public Transform GetRotationTarget()
    {
        return rotationPivot != null ? rotationPivot : transform;
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