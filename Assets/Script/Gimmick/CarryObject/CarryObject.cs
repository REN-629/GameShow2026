using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class CarryObject : MonoBehaviour
{
    [Header("分類")]
    public CarryObjectType objectType = CarryObjectType.Generic;

    [Header("重量")]
    public float weight = 50f;

    [Header("インベントリ")]
    public bool canStoreInInventory = false;

    [Header("持てる")]
    public bool canCarry = true;
    public CarryObjectHoldType holdType = CarryObjectHoldType.Default;

    [Header("持ち位置補正")]
    public Vector3 heldPositionOffset;
    public Vector3 heldRotationOffset;

    [Header("専用HoldPoint")]
    public bool useCustomHoldPoint = false;
    public Transform customHoldPoint;

    [Header("回転")]
    public bool canRotateWhileHeld = true;
    public float rotateSpeed = 4f;

    [Header("手放し")]
    public bool canRelease = true;
    public float releaseForwardOffset = 1f;
    public float releaseUpOffset = 0.2f;
    public float dropForce = 1f;

    [Header("投げ")]
    public bool canThrow = true;
    public float minThrowForce = 3f;
    public float maxThrowForce = 15f;
    public float throwForceMultiplier = 1f;

    [Header("投擲回転")]
    public bool addSpinOnThrow = true;
    public Vector3 maxAngularVelocity = new Vector3(8f, 14f, 10f);
    public bool randomizeSpinDirection = true;
    public float spinRandomMultiplier = 1f;

    [Header("物理")]
    public bool disableCollidersWhileHeld = true;
    public bool forceContinuousCollisionOnRelease = true;
    public bool useInterpolationOnRelease = true;

    [Header("手持ち中の半透明")]
    public bool makeTransparentWhileHeld = false;
    [Range(0.05f, 1f)]
    public float heldAlpha = 0.35f;

    [Header("状態")]
    public bool isHeld = false;

    private Rigidbody rb;
    private Collider[] colliders;
    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;
    private bool transparencyApplied;

    void Awake()
    {
        CacheComponents();
    }

    void CacheComponents()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (colliders == null || colliders.Length == 0)
            colliders = GetComponentsInChildren<Collider>(true);

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(true);

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();
    }

    public float GetWeight()
    {
        return weight;
    }

    public Rigidbody GetRigidbody()
    {
        CacheComponents();
        return rb;
    }

    public void Hold(Transform holdPoint)
    {
        if (!canCarry)
            return;

        Transform targetHoldPoint = GetActualHoldPoint(holdPoint);

        if (targetHoldPoint == null)
            return;

        CacheComponents();

        isHeld = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.detectCollisions = false;
            rb.isKinematic = true;
        }

        if (disableCollidersWhileHeld)
            SetCollidersEnabled(false);

        if (makeTransparentWhileHeld)
            ApplyHeldTransparency();

        ForceHoldTransform(targetHoldPoint);
    }

    public void ForceHoldTransform(Transform holdPoint)
    {
        if (!isHeld)
            return;

        Transform targetHoldPoint = GetActualHoldPoint(holdPoint);

        if (targetHoldPoint == null)
            return;

        transform.position =
            targetHoldPoint.position +
            targetHoldPoint.TransformDirection(heldPositionOffset);

        transform.rotation =
            targetHoldPoint.rotation *
            Quaternion.Euler(heldRotationOffset);
    }

    Transform GetActualHoldPoint(Transform fallback)
    {
        if (useCustomHoldPoint && customHoldPoint != null)
            return customHoldPoint;

        return fallback;
    }

    public void AddRotation(Vector3 rotation)
    {
        if (!isHeld || !canRotateWhileHeld)
            return;

        transform.Rotate(rotation, Space.Self);
    }

    public void DropFrom(Transform origin)
    {
        if (!canRelease)
            return;

        Vector3 forward = transform.forward;
        Quaternion rotation = transform.rotation;
        Vector3 position = transform.position;

        if (origin != null)
        {
            forward = origin.forward;
            rotation = origin.rotation;
            position =
                origin.position +
                forward.normalized * releaseForwardOffset +
                Vector3.up * releaseUpOffset;
        }

        ReleaseToWorld(position, rotation, forward.normalized * dropForce, false, 0f);
    }

    public void ThrowFrom(Transform origin, float chargeRate)
    {
        if (!canRelease || !canThrow)
            return;

        chargeRate = Mathf.Clamp01(chargeRate);

        Vector3 forward = transform.forward;
        Quaternion rotation = transform.rotation;
        Vector3 position = transform.position;

        if (origin != null)
        {
            forward = origin.forward;
            rotation = origin.rotation;
            position =
                origin.position +
                forward.normalized * releaseForwardOffset +
                Vector3.up * releaseUpOffset;
        }

        float force =
            Mathf.Lerp(minThrowForce, maxThrowForce, chargeRate) *
            Mathf.Max(0f, throwForceMultiplier);

        ReleaseToWorld(position, rotation, forward.normalized * force, true, chargeRate);
    }

    void ReleaseToWorld(Vector3 position, Quaternion rotation, Vector3 force, bool isThrow, float chargeRate)
    {
        CacheComponents();

        isHeld = false;

        transform.position = position;
        transform.rotation = rotation;

        RestoreTransparency();

        if (disableCollidersWhileHeld)
            SetCollidersEnabled(true);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.detectCollisions = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (forceContinuousCollisionOnRelease)
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            if (useInterpolationOnRelease)
                rb.interpolation = RigidbodyInterpolation.Interpolate;

            rb.WakeUp();
            rb.AddForce(force, ForceMode.Impulse);

            if (isThrow && addSpinOnThrow)
                rb.angularVelocity = GetAngularVelocity(chargeRate);
        }
    }

    Vector3 GetAngularVelocity(float chargeRate)
    {
        Vector3 baseSpin = maxAngularVelocity * chargeRate;

        if (!randomizeSpinDirection)
            return baseSpin;

        Vector3 random = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;

        Vector3 randomized = Vector3.Scale(
            random,
            new Vector3(
                Mathf.Abs(maxAngularVelocity.x),
                Mathf.Abs(maxAngularVelocity.y),
                Mathf.Abs(maxAngularVelocity.z)
            )
        );

        return Vector3.Lerp(baseSpin, randomized * chargeRate, spinRandomMultiplier);
    }

    void SetCollidersEnabled(bool enabled)
    {
        CacheComponents();

        if (colliders == null)
            return;

        foreach (Collider col in colliders)
        {
            if (col != null)
                col.enabled = enabled;
        }
    }

    void ApplyHeldTransparency()
    {
        CacheComponents();

        if (renderers == null)
            return;

        transparencyApplied = true;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            renderer.GetPropertyBlock(propertyBlock);

            Material mat = renderer.sharedMaterial;

            Color color = Color.white;

            if (mat != null && mat.HasProperty("_Color"))
                color = mat.color;

            color.a = heldAlpha;

            propertyBlock.SetColor("_Color", color);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }

    void RestoreTransparency()
    {
        if (!transparencyApplied)
            return;

        CacheComponents();

        if (renderers == null)
            return;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            renderer.SetPropertyBlock(null);
        }

        transparencyApplied = false;
    }
}
