using UnityEngine;

public class ItemThrower : MonoBehaviour
{
    [Header("参照")]
    public Inventory inventory;
    public HeldItemController heldItemController;
    public Camera playerCamera;

    [Header("投げる位置")]
    public Transform throwPoint;

    [Header("入力")]
    public KeyCode throwKey = KeyCode.G;

    [Header("投擲力")]
    public float minThrowForce = 3f;
    public float maxThrowForce = 15f;
    public float maxChargeTime = 1.2f;

    [Header("投擲回転")]
    public bool addSpinOnThrow = true;
    public Vector3 defaultMaxAngularVelocity = new Vector3(8f, 14f, 10f);
    public bool randomizeSpinDirection = true;
    public float spinRandomMultiplier = 1f;

    [Header("チャージUI")]
    public ThrowChargeGaugeUI chargeGauge;

    [Header("デバッグ")]
    public bool debugLog = true;

    private float chargeTimer = 0f;
    private bool charging = false;

    void Update()
    {
        HandleThrowCharge();
    }

    void HandleThrowCharge()
    {
        if (Input.GetKeyDown(throwKey))
        {
            StartCharge();
        }

        if (charging && Input.GetKey(throwKey))
        {
            chargeTimer += Time.deltaTime;

            if (chargeTimer > maxChargeTime)
                chargeTimer = maxChargeTime;

            UpdateGauge();
        }

        if (charging && Input.GetKeyUp(throwKey))
        {
            ThrowSelectedItem();
        }
    }

    void StartCharge()
    {
        if (inventory == null)
            return;

        if (inventory.GetSelectedItem() == null)
            return;

        charging = true;
        chargeTimer = 0f;
        UpdateGauge();
    }

    void UpdateGauge()
    {
        if (chargeGauge != null)
            chargeGauge.SetRate(GetChargeRate());
    }

    float GetChargeRate()
    {
        if (maxChargeTime <= 0f)
            return 1f;

        return Mathf.Clamp01(chargeTimer / maxChargeTime);
    }

    void ThrowSelectedItem()
    {
        if (!charging)
            return;

        charging = false;

        if (chargeGauge != null)
            chargeGauge.Hide();

        if (inventory == null)
            return;

        PickupItem item = inventory.GetSelectedItem();

        if (item == null)
            return;

        if (!item.canThrow)
            return;

        float chargeRate = GetChargeRate();
        float forceValue = Mathf.Lerp(minThrowForce, maxThrowForce, chargeRate);

        Transform origin = throwPoint;

        Vector3 position;
        Quaternion rotation;
        Vector3 direction;

        if (origin != null)
        {
            position = origin.position;
            rotation = origin.rotation;
            direction = origin.forward;
        }
        else
        {
            Camera cam = playerCamera != null ? playerCamera : Camera.main;

            if (cam != null)
            {
                position = cam.transform.position + cam.transform.forward * 0.8f;
                rotation = cam.transform.rotation;
                direction = cam.transform.forward;
            }
            else
            {
                position = transform.position + transform.forward;
                rotation = transform.rotation;
                direction = transform.forward;
            }
        }

        if (heldItemController != null)
            heldItemController.ReleaseHeldItemForThrow(item);

        inventory.RemoveSelectedItem();

        Vector3 force = direction.normalized * forceValue;

        item.ThrowToWorld(position, rotation, force);

        ApplySpin(item, chargeRate);

        if (debugLog)
            Debug.Log(item.name + " 投擲 charge=" + chargeRate + " force=" + forceValue);
    }

    void ApplySpin(PickupItem item, float chargeRate)
    {
        if (!addSpinOnThrow)
            return;

        if (item == null)
            return;

        Rigidbody rb = item.GetComponent<Rigidbody>();

        if (rb == null)
            return;

        Vector3 angularVelocity = GetAngularVelocity(item, chargeRate);
        rb.angularVelocity = angularVelocity;
    }

    Vector3 GetAngularVelocity(PickupItem item, float chargeRate)
    {
        ThrowableSpinSettings spinSettings =
            item.GetComponent<ThrowableSpinSettings>();

        if (spinSettings != null && spinSettings.useCustomSpin)
            return spinSettings.GetAngularVelocity(chargeRate);

        Vector3 baseSpin = defaultMaxAngularVelocity * chargeRate;

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
                Mathf.Abs(defaultMaxAngularVelocity.x),
                Mathf.Abs(defaultMaxAngularVelocity.y),
                Mathf.Abs(defaultMaxAngularVelocity.z)
            )
        );

        return Vector3.Lerp(baseSpin, randomized * chargeRate, spinRandomMultiplier);
    }
}
