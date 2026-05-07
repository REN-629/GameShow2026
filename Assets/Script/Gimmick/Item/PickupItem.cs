// PickupItem：統合版（属性・材質・耐久・保持状態・使用処理対応）
// 修正版：Kinematic Rigidbody に velocity を設定しないように順番を修正
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
        // Rigidbody は PickupItem と同じ Root に付けるのが前提
        rb = GetComponent<Rigidbody>();

        colliders = GetComponentsInChildren<Collider>(true);
        renderers = GetComponentsInChildren<Renderer>(true);

        if (rb == null)
        {
            Debug.LogError(name + " に Rigidbody がありません。PickupItem と同じRootに Rigidbody を付けてください。");
        }
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

        StopPhysicsAndMakeKinematic();

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

        StopPhysicsAndMakeKinematic();

        ResetHoldState();
    }

    void StopPhysicsAndMakeKinematic()
    {
        if (rb == null)
            return;

        // 重要：
        // Kinematic化する前に速度を止める。
        // Kinematic後に velocity を触るとUnityが警告を出す。
        if (rb.isKinematic)
        {
            rb.isKinematic = false;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.useGravity = false;
        rb.detectCollisions = false;
        rb.isKinematic = true;
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

        transform.SetParent(null);

        transform.position =
            holdPoint.position
            + holdPoint.right * finalPositionOffset.x
            + holdPoint.up * finalPositionOffset.y
            + holdPoint.forward * finalPositionOffset.z;

        transform.rotation =
            holdPoint.rotation
            * Quaternion.Euler(finalRotationOffset);

        transform.localScale = baseScale;
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
            // ワールドに戻す時は物理を復帰
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.detectCollisions = true;

            // 念のため投げる直前の速度をクリア
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
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
            Debug.Log(name + " を投げた / force=" + force + " / isKinematic=" + rb.isKinematic);
        }
        else
        {
            Debug.LogError(name + " は Rigidbody が無いため投げられません");
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
