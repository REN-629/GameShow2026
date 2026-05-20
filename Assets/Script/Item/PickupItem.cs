//PickupItem：アイテム本体の一番重要なスクリプト
//V2:重さの参照
//InventoryがこのitemWeightを合計してプレイヤーの移動速度や重量ゲージに使う

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PickupItem : MonoBehaviour
{
    [Header("アイテムデータ")]
    public ItemData itemData;

    [Header("重量")]
    [Tooltip("このアイテムの重さ。最大重量100を基準にする。例：ライター2、バール12、箱30")]
    public float itemWeight = 1f;

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
        ApplyWorldState();
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

    public void SetStoredState(bool forceTurnOffToggle = true)
    {
        ToggleUseAction toggle = GetComponent<ToggleUseAction>();

        if (forceTurnOffToggle && toggle != null)
        {
            toggle.ForceTurnOff();
        }

        currentState = PickupItemState.Stored;

        ApplyStoredState();


        //エネルギー系アイテムを持ってない時にオフにする
        //ToggleUseAction toggle = GetComponent<ToggleUseAction>();

        // if (toggle != null)
        //{
        //   toggle.ForceTurnOff();
        //}

    }

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

    void StopRigidbodyForCarry()
    {
        if (rb == null)
            return;

        if (rb.isKinematic)
            rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.useGravity = false;
        rb.detectCollisions = false;
        rb.isKinematic = true;
    }

    void SetVisible(bool visible)
    {
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

        if (success && durability != null)
        {
            durability.ApplyDamage(1);
        }
    }
}
