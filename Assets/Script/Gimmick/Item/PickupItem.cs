// PickupItem：統合版（属性・材質・耐久・保持状態・使用処理対応）
// UseActionが true を返した時だけ、使用耐久が減る。
// 空振りや無効ヒットでは耐久が減らない。
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("アイテムデータ")]
    public ItemData itemData;

    [Header("使用設定")]
    public bool canUseWithLeftClick = false;

    [Header("投擲設定")]
    public bool canThrow = true;

    [Header("回転設定")]
    public bool canRotate = true;
    public float rotateSpeed = 3f;
    public bool resetRotationOnStore = true;

    [Header("持ち方")]
    public HoldType holdType = HoldType.OneHand;

    [Header("持ち方設定")]
    public HoldPoseData holdPose = new HoldPoseData();

    [Header("効果属性")]
    public EffectAttributeType[] effectAttributes;

    [Header("材質属性")]
    public MaterialAttributeType[] materialAttributes;

    public bool IsHeld { get; private set; }

    private Vector3 currentExtraRotation;

    private Rigidbody rb;
    private Collider[] colliders;
    private Renderer[] renderers;

    void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>(true);
        colliders = GetComponentsInChildren<Collider>(true);
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    public bool HasEffectAttribute(EffectAttributeType attribute)
    {
        if (effectAttributes == null)
            return false;

        foreach (EffectAttributeType a in effectAttributes)
        {
            if (a == attribute)
                return true;
        }

        return false;
    }

    public bool HasMaterialAttribute(MaterialAttributeType attribute)
    {
        if (materialAttributes == null)
            return false;

        foreach (MaterialAttributeType a in materialAttributes)
        {
            if (a == attribute)
                return true;
        }

        return false;
    }

    public void AddHoldRotation(Vector3 delta)
    {
        currentExtraRotation += delta;
    }

    public void ResetHoldState()
    {
        currentExtraRotation = Vector3.zero;
    }

    public void SetStoredState()
    {
        transform.SetParent(null);

        IsHeld = false;

        SetVisible(false);
        SetColliders(false);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.useGravity = false;
            rb.detectCollisions = false;
            rb.isKinematic = true;
        }

        if (resetRotationOnStore)
        {
            ResetHoldState();
        }
    }

    public void SetHeldState()
    {
        transform.SetParent(null);

        IsHeld = true;

        SetVisible(true);
        SetColliders(false);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.useGravity = false;
            rb.detectCollisions = false;
            rb.isKinematic = true;
        }

        ResetHoldState();
    }

    public void ForceHoldTransform(Transform holdPoint)
    {
        ForceHoldTransform(
            holdPoint,
            Vector3.zero,
            Vector3.zero
        );
    }

    public void ForceHoldTransform(
        Transform holdPoint,
        Vector3 extraPositionOffset,
        Vector3 extraRotationOffset
    )
    {
        if (holdPoint == null)
            return;

        Vector3 basePositionOffset =
            holdPose != null
            ? holdPose.positionOffset
            : Vector3.zero;

        Vector3 baseRotationOffset =
            holdPose != null
            ? holdPose.rotationOffset
            : Vector3.zero;

        Vector3 baseScale =
            holdPose != null
            ? holdPose.scale
            : Vector3.one;

        Vector3 finalPositionOffset =
            basePositionOffset + extraPositionOffset;

        Vector3 finalRotationOffset =
            baseRotationOffset
            + currentExtraRotation
            + extraRotationOffset;

        Transform itemRoot = transform;

        itemRoot.SetParent(null);

        itemRoot.position =
            holdPoint.position
            + holdPoint.right * finalPositionOffset.x
            + holdPoint.up * finalPositionOffset.y
            + holdPoint.forward * finalPositionOffset.z;

        itemRoot.rotation =
            holdPoint.rotation
            * Quaternion.Euler(finalRotationOffset);

        itemRoot.localScale = baseScale;
    }

    public void DropToWorld(
        Vector3 position,
        Quaternion rotation)
    {
        transform.SetParent(null);

        IsHeld = false;

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

        ResetHoldState();
    }

    public void ThrowToWorld(
        Vector3 position,
        Quaternion rotation,
        Vector3 force)
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

        AttributeDurability durability =
            GetComponent<AttributeDurability>();

        if (durability != null && durability.IsBroken())
        {
            Debug.Log(name + " は壊れていて使えない");
            return;
        }

        ItemUseAction action =
            GetComponent<ItemUseAction>();

        if (action != null)
        {
            bool success = action.Use(this);

            // 使用が有効だった時だけ耐久を減らす。
            // 鈍器なら「何かに有効ヒットした時だけ」減る。
            if (success && durability != null)
            {
                durability.ApplyDamage(1);
            }
        }
    }

    void SetVisible(bool visible)
    {
        foreach (Renderer r in renderers)
        {
            if (r != null)
            {
                r.enabled = visible;
            }
        }
    }

    void SetColliders(bool enabled)
    {
        foreach (Collider c in colliders)
        {
            if (c != null)
            {
                c.enabled = enabled;
            }
        }
    }
}
