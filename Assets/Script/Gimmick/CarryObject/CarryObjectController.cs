using UnityEngine;

public class CarryObjectController : MonoBehaviour
{
    [Header("参照")]
    public Camera playerCamera;
    public CarryObjectHoldPointSet holdPointSet;

    [Header("通常アイテム操作を止める")]
    public HeldItemController heldItemController;
    public ItemThrower itemThrower;

    [Header("入力")]
    public KeyCode interactKey = KeyCode.E;
    public KeyCode releaseKey = KeyCode.G;
    public int rotateMouseButton = 1;

    [Header("拾う")]
    public float pickupDistance = 3f;
    public LayerMask pickupMask = ~0;

    [Header("投げチャージ")]
    public bool useThrowCharge = true;
    public float maxChargeTime = 1.2f;
    public ThrowChargeGaugeUI chargeGauge;

    [Header("操作")]
    public bool releaseWithInteractKey = true;

    [Header("デバッグ")]
    public bool debugLog = true;

    private CarryObject currentObject;
    private bool charging;
    private float chargeTimer;

    public bool IsCarrying => currentObject != null;

    void Update()
    {
        if (currentObject == null)
        {
            if (Input.GetKeyDown(interactKey))
                TryPickup();

            return;
        }

        HandleRotation();
        HandleReleaseAndThrow();
    }

    void LateUpdate()
    {
        UpdateHeldObject();
    }

    void TryPickup()
    {
        Camera cam = playerCamera != null ? playerCamera : Camera.main;

        if (cam == null)
            return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, pickupDistance, pickupMask, QueryTriggerInteraction.Collide))
            return;

        CarryObject carryObject = hit.collider.GetComponentInParent<CarryObject>();

        if (carryObject == null || carryObject.isHeld)
            return;

        Pickup(carryObject);
    }

    public void Pickup(CarryObject carryObject)
    {
        if (carryObject == null || currentObject != null)
            return;

        if (carryObject.canStoreInInventory)
        {
            Debug.LogWarning(carryObject.name + " はインベントリ格納対象です。PickupItem側で扱ってください。");
            return;
        }

        Transform holdPoint = GetHoldPoint(carryObject);

        if (holdPoint == null)
        {
            Debug.LogWarning("CarryObjectController: HoldPointがありません");
            return;
        }

        StoreHeldItemBeforeCarry();

        currentObject = carryObject;
        currentObject.Hold(holdPoint);

        if (RunActionLogger.Instance != null)
            RunActionLogger.Instance.LogCarryObjectUse();

        charging = false;
        chargeTimer = 0f;
        HideGauge();

        SetNormalItemControl(false);

        if (debugLog)
            Debug.Log("CarryObjectを持った: " + carryObject.name);
    }

    void UpdateHeldObject()
    {
        if (currentObject == null)
            return;

        Transform holdPoint = GetHoldPoint(currentObject);

        if (holdPoint != null)
            currentObject.ForceHoldTransform(holdPoint);
    }

    void HandleRotation()
    {
        if (currentObject == null || !currentObject.canRotateWhileHeld)
            return;

        if (!Input.GetMouseButton(rotateMouseButton))
            return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        currentObject.AddRotation(new Vector3(
            mouseY * currentObject.rotateSpeed,
            -mouseX * currentObject.rotateSpeed,
            0f
        ));
    }

    void HandleReleaseAndThrow()
    {
        if (currentObject == null)
            return;

        if (releaseWithInteractKey && Input.GetKeyDown(interactKey))
        {
            DropObject();
            return;
        }

        if (!currentObject.canThrow || !useThrowCharge)
        {
            if (Input.GetKeyDown(releaseKey))
                DropObject();

            return;
        }

        if (Input.GetKeyDown(releaseKey))
            StartCharge();

        if (charging && Input.GetKey(releaseKey))
        {
            chargeTimer += Time.deltaTime;

            if (chargeTimer > maxChargeTime)
                chargeTimer = maxChargeTime;

            UpdateGauge();
        }

        if (charging && Input.GetKeyUp(releaseKey))
            ThrowObject();
    }

    void StartCharge()
    {
        charging = true;
        chargeTimer = 0f;
        UpdateGauge();
    }

    float GetChargeRate()
    {
        if (maxChargeTime <= 0f)
            return 1f;

        return Mathf.Clamp01(chargeTimer / maxChargeTime);
    }

    void UpdateGauge()
    {
        if (chargeGauge != null)
            chargeGauge.SetRate(GetChargeRate());
    }

    void HideGauge()
    {
        if (chargeGauge != null)
            chargeGauge.Hide();
    }

    public void DropObject()
    {
        if (currentObject == null)
            return;

        CarryObject obj = currentObject;
        currentObject = null;

        charging = false;
        chargeTimer = 0f;
        HideGauge();

        Transform origin = GetReleaseOrigin();
        obj.DropFrom(origin);

        SetNormalItemControl(true);

        if (debugLog)
            Debug.Log("CarryObjectを手放した: " + obj.name);
    }

    public void ThrowObject()
    {
        if (currentObject == null)
            return;

        CarryObject obj = currentObject;
        currentObject = null;

        float chargeRate = GetChargeRate();

        charging = false;
        chargeTimer = 0f;
        HideGauge();

        Transform origin = GetReleaseOrigin();
        obj.ThrowFrom(origin, chargeRate);

        if (RunActionLogger.Instance != null)
            RunActionLogger.Instance.LogCarryObjectThrow();

        SetNormalItemControl(true);

        if (debugLog)
            Debug.Log("CarryObjectを投げた: " + obj.name + " / charge=" + chargeRate);
    }

    Transform GetReleaseOrigin()
    {
        Camera cam = playerCamera != null ? playerCamera : Camera.main;

        if (cam != null)
            return cam.transform;

        return transform;
    }

    Transform GetHoldPoint(CarryObject carryObject)
    {
        if (carryObject == null)
            return null;

        if (carryObject.useCustomHoldPoint && carryObject.customHoldPoint != null)
            return carryObject.customHoldPoint;

        if (holdPointSet == null)
            return null;

        return holdPointSet.GetHoldPoint(carryObject.holdType);
    }


    void StoreHeldItemBeforeCarry()
    {
        if (heldItemController != null)
            heldItemController.StoreCurrentHeldItemForExternalHold();
    }

    void SetNormalItemControl(bool enabled)
    {
        if (heldItemController != null)
            heldItemController.enabled = enabled;

        if (itemThrower != null)
            itemThrower.enabled = enabled;
    }
}
