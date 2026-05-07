// PickupItem：統合版（属性・材質・耐久・保持状態・使用処理対応）
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    // --------------------------------------------------
    // 基本データ
    // --------------------------------------------------

    [Header("アイテムデータ")]
    public ItemData itemData;

    // --------------------------------------------------
    // 使用設定
    // --------------------------------------------------

    [Header("使用設定")]
    public bool canUseWithLeftClick = false;

    // --------------------------------------------------
    // 投擲設定
    // --------------------------------------------------

    [Header("投擲設定")]
    public bool canThrow = true;

    // --------------------------------------------------
    // 回転設定
    // --------------------------------------------------

    [Header("回転設定")]
    public bool canRotate = true;
    public float rotateSpeed = 3f;
    public bool resetRotationOnStore = true;

    // --------------------------------------------------
    // 持ち方
    // --------------------------------------------------

    [Header("持ち方")]
    public HoldType holdType = HoldType.OneHand;

    [Header("持ち方設定")]
    public HoldPoseData holdPose = new HoldPoseData();

    // --------------------------------------------------
    // 効果属性
    // 「何ができるか」
    // 例：
    // Blunt = 殴れる
    // Fire = 火を付けられる
    // --------------------------------------------------

    [Header("効果属性")]
    public EffectAttributeType[] effectAttributes;

    // --------------------------------------------------
    // 材質属性
    // 「何でできているか」
    // 例：
    // Metal = 金属
    // Wood = 木
    // --------------------------------------------------

    [Header("材質属性")]
    public MaterialAttributeType[] materialAttributes;

    // --------------------------------------------------
    // 現在手に持たれているか
    // --------------------------------------------------

    public bool IsHeld { get; private set; }

    // --------------------------------------------------
    // 内部用
    // --------------------------------------------------

    private Vector3 currentExtraRotation;

    private Rigidbody rb;
    private Collider[] colliders;
    private Renderer[] renderers;

    // --------------------------------------------------
    // 初期化
    // --------------------------------------------------

    void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>(true);

        colliders =
            GetComponentsInChildren<Collider>(true);

        renderers =
            GetComponentsInChildren<Renderer>(true);
    }

    // --------------------------------------------------
    // 効果属性を持っているか
    // --------------------------------------------------

    public bool HasEffectAttribute(
        EffectAttributeType attribute)
    {
        if (effectAttributes == null)
            return false;

        foreach (EffectAttributeType a in effectAttributes)
        {
            if (a == attribute)
            {
                return true;
            }
        }

        return false;
    }

    // --------------------------------------------------
    // 材質属性を持っているか
    // --------------------------------------------------

    public bool HasMaterialAttribute(
        MaterialAttributeType attribute)
    {
        if (materialAttributes == null)
            return false;

        foreach (MaterialAttributeType a in materialAttributes)
        {
            if (a == attribute)
            {
                return true;
            }
        }

        return false;
    }

    // --------------------------------------------------
    // Hold回転追加
    // --------------------------------------------------

    public void AddHoldRotation(Vector3 delta)
    {
        currentExtraRotation += delta;
    }

    // --------------------------------------------------
    // Hold状態リセット
    // --------------------------------------------------

    public void ResetHoldState()
    {
        currentExtraRotation = Vector3.zero;
    }

    // --------------------------------------------------
    // インベントリ収納状態
    // --------------------------------------------------

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

    // --------------------------------------------------
    // 手持ち状態
    // --------------------------------------------------

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

    // --------------------------------------------------
    // HoldPoint追従
    // --------------------------------------------------

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

    // --------------------------------------------------
    // ワールドへ落とす
    // --------------------------------------------------

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

    // --------------------------------------------------
    // 投げる
    // --------------------------------------------------

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

    // --------------------------------------------------
    // 使用
    // --------------------------------------------------

    public void Use()
    {
        if (!canUseWithLeftClick)
            return;

        AttributeDurability durability =
            GetComponent<AttributeDurability>();

        // 壊れている場合は使用不可
        if (durability != null && durability.IsBroken())
        {
            Debug.Log(name + " は壊れていて使えない");
            return;
        }

        ItemUseAction action =
            GetComponent<ItemUseAction>();

        if (action != null)
        {
            action.Use(this);

            // 使用時に耐久を1減らす
            if (durability != null)
            {
                durability.ApplyDamage(1);
            }
        }
    }

    // --------------------------------------------------
    // Renderer ON/OFF
    // --------------------------------------------------

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

    // --------------------------------------------------
    // Collider ON/OFF
    // --------------------------------------------------

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
