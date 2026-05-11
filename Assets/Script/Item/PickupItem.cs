// PickupItem：アイテム本体の一番重要なスクリプト
//
// このスクリプトの役割：
// ・アイテムを「World / Held / Stored」の3状態で管理する
// ・見た目ON/OFF、Collider ON/OFF、Rigidbody ON/OFFをまとめて切り替える
// ・投げる時に確実にWorld状態へ戻す
// ・効果属性、材質属性、使用処理を持つ
//
// 付ける場所：
// アイテムRootに付ける。
// 例：
// CrowbarRoot ← PickupItem / Rigidbody / AttributeDurability / BluntUseAction
// ├ PhysicsCollider
// ├ PickupArea
// └ Model
//
// 絶対ルール：
// ・RigidbodyはPickupItemと同じRootに付ける
// ・ModelにはPickupItemを付けない
// ・PickupAreaは拾う専用Triggerにする

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PickupItem : MonoBehaviour
{
    [Header("アイテムデータ")]
    public ItemData itemData;

    [Header("現在の状態")]
    [SerializeField] private PickupItemState currentState = PickupItemState.World;

    [Header("使用設定")]
    public bool canUseWithLeftClick = false;

    [Header("投擲設定")]
    public bool canThrow = true;

    [Header("回転設定")]
    public bool canRotate = true;
    public float rotateSpeed = 3f;

    [Header("持ち方")]
    public HoldType holdType = HoldType.OneHand;

    [Header("持ち方設定")]
    public HoldPoseData holdPose = new HoldPoseData();

    [Header("属性：何ができるか")]
    public EffectAttributeType[] effectAttributes;

    [Header("材質：何でできているか")]
    public MaterialAttributeType[] materialAttributes;

    [Header("拾う専用Collider")]
    [Tooltip("子オブジェクト PickupArea の SphereCollider を入れる。Is Trigger ON 推奨。")]
    public Collider pickupArea;

    [Header("デバッグ")]
    public bool debugStateLog = false;

    public bool IsHeld => currentState == PickupItemState.Held;
    public PickupItemState CurrentState => currentState;

    private Rigidbody rb;
    private Vector3 currentHoldRotationOffset;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError(name + " に Rigidbody がありません。PickupItem と同じRootに付けてください。");
        }
    }

    void Start()
    {
        // シーンに最初から置いてあるアイテムはWorld状態にする
        ApplyWorldState();
    }

    // --------------------------------------------------
    // 属性チェック
    // --------------------------------------------------

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

    // --------------------------------------------------
    // 状態変更
    // --------------------------------------------------

    public void SetWorldState()
    {
        currentState = PickupItemState.World;
        ApplyWorldState();
    }

    public void SetHeldState()
    {
        currentState = PickupItemState.Held;
        ApplyHeldState();
    }

    public void SetStoredState()
    {
        currentState = PickupItemState.Stored;
        ApplyStoredState();
    }

    // World状態：地面や空中にある状態
    // 見える、物理ON、拾う判定ON
    void ApplyWorldState()
    {
        transform.SetParent(null);

        SetVisible(true);
        SetPhysicsColliders(true);
        SetPickupArea(true);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.detectCollisions = true;
        }

        if (debugStateLog)
            Debug.Log(name + " → World状態");
    }

    // Held状態：手に持っている状態
    // 見える、物理OFF、拾う判定OFF
    void ApplyHeldState()
    {
        transform.SetParent(null);

        SetVisible(true);
        SetPhysicsColliders(false);
        SetPickupArea(false);

        StopRigidbodyForCarry();

        ResetHoldRotation();

        if (debugStateLog)
            Debug.Log(name + " → Held状態");
    }

    // Stored状態：インベントリにしまわれている状態
    // 見えない、物理OFF、拾う判定OFF
    void ApplyStoredState()
    {
        transform.SetParent(null);

        SetVisible(false);
        SetPhysicsColliders(false);
        SetPickupArea(false);

        StopRigidbodyForCarry();

        ResetHoldRotation();

        if (debugStateLog)
            Debug.Log(name + " → Stored状態");
    }

    // 手持ちや収納中は物理を止める
    void StopRigidbodyForCarry()
    {
        if (rb == null)
            return;

        // Unity 6ではKinematic中にlinearVelocityを触ると警告が出る。
        // なので一度isKinematicをfalseにしてから速度を0にする。
        if (rb.isKinematic)
            rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.useGravity = false;
        rb.detectCollisions = false;
        rb.isKinematic = true;
    }

    // --------------------------------------------------
    // 表示とCollider制御
    // --------------------------------------------------

    void SetVisible(bool visible)
    {
        // 毎回取り直す。これで「投げたのにRendererがOFFのまま」を防ぐ。
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer r in renderers)
        {
            if (r != null)
                r.enabled = visible;
        }
    }

    void SetPhysicsColliders(bool enabled)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);

        foreach (Collider c in colliders)
        {
            if (c == null)
                continue;

            // PickupAreaは拾う専用なので物理Colliderとは別扱い
            if (pickupArea != null && c == pickupArea)
                continue;

            c.enabled = enabled;
        }
    }

    void SetPickupArea(bool enabled)
    {
        if (pickupArea != null)
            pickupArea.enabled = enabled;
    }

    // --------------------------------------------------
    // HoldPoint追従
    // --------------------------------------------------

    public void AddHoldRotation(Vector3 delta)
    {
        currentHoldRotationOffset += delta;
    }

    public void ResetHoldRotation()
    {
        currentHoldRotationOffset = Vector3.zero;
    }

    public void ForceHoldTransform(Transform holdPoint)
    {
        ForceHoldTransform(holdPoint, Vector3.zero, Vector3.zero);
    }

    public void ForceHoldTransform(Transform holdPoint, Vector3 extraPositionOffset, Vector3 extraRotationOffset)
    {
        if (holdPoint == null)
            return;

        Vector3 basePos = holdPose != null ? holdPose.positionOffset : Vector3.zero;
        Vector3 baseRot = holdPose != null ? holdPose.rotationOffset : Vector3.zero;
        Vector3 baseScale = holdPose != null ? holdPose.scale : Vector3.one;

        Vector3 finalPos = basePos + extraPositionOffset;
        Vector3 finalRot = baseRot + currentHoldRotationOffset + extraRotationOffset;

        transform.position =
            holdPoint.position
            + holdPoint.right * finalPos.x
            + holdPoint.up * finalPos.y
            + holdPoint.forward * finalPos.z;

        transform.rotation =
            holdPoint.rotation * Quaternion.Euler(finalRot);

        transform.localScale = baseScale;
    }

    // --------------------------------------------------
    // ワールド配置・投擲
    // --------------------------------------------------

    public void DropToWorld(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;

        SetWorldState();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void ThrowToWorld(Vector3 position, Quaternion rotation, Vector3 force)
    {
        DropToWorld(position, rotation);

        if (rb == null)
        {
            Debug.LogError(name + " はRigidbodyがないので投げられません");
            return;
        }

        rb.WakeUp();
        rb.AddForce(force, ForceMode.Impulse);

        Debug.Log(name + " を投げた / pos=" + transform.position + " / force=" + force + " / state=" + currentState);
    }

    // --------------------------------------------------
    // 使用
    // --------------------------------------------------

    public void Use()
    {
        if (!canUseWithLeftClick)
            return;

        AttributeDurability durability = GetComponent<AttributeDurability>();

        if (durability != null && durability.IsBroken())
        {
            Debug.Log(name + " は壊れていて使えない");
            return;
        }

        ItemUseAction action = GetComponent<ItemUseAction>();

        if (action == null)
            return;

        bool success = action.Use(this);

        // 空振りなら耐久を減らさない
        if (success && durability != null)
        {
            durability.ApplyDamage(1);
        }
    }
}
