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
    private Material[][] originalMaterials;
    private bool transparencyApplied;

    void Awake()
    {
        CacheComponents();
        CacheOriginalMaterials();
    }

    void CacheComponents()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (colliders == null || colliders.Length == 0)
            colliders = GetComponentsInChildren<Collider>(true);

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(true);
    }

    void CacheOriginalMaterials()
    {
        CacheComponents();

        if (renderers == null)
            return;

        originalMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                originalMaterials[i] = renderers[i].materials;
        }
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

            Material[] mats = renderer.materials;

            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null)
                    continue;

                SetMaterialTransparent(mats[i], heldAlpha);
            }

            renderer.materials = mats;
        }
    }

    void RestoreTransparency()
    {
        if (!transparencyApplied)
            return;

        CacheComponents();

        if (renderers != null && originalMaterials != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null &&
                    i < originalMaterials.Length &&
                    originalMaterials[i] != null)
                {
                    renderers[i].materials = originalMaterials[i];
                }
            }
        }

        transparencyApplied = false;
    }

    void SetMaterialTransparent(Material mat, float alpha)
    {
        if (!mat.HasProperty("_Color"))
            return;

        Color color = mat.color;
        color.a = alpha;
        mat.color = color;

        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);

        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    }
}
