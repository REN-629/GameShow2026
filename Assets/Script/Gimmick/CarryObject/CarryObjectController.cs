using UnityEngine;

public class CarryObjectController : MonoBehaviour
{
    public Camera playerCamera;
    public CarryObjectHoldPointSet holdPointSet;

    public HeldItemController heldItemController;
    public ItemThrower itemThrower;

    public KeyCode interactKey = KeyCode.E;
    public KeyCode releaseKey = KeyCode.G;
    public int rotateMouseButton = 1;

    public float pickupDistance = 3f;
    public LayerMask pickupMask = ~0;

    public bool releaseWithInteractKey = true;
    public bool debugLog = true;

    CarryObject currentObject;

    public bool IsCarrying => currentObject != null;

    void Update()
    {
        if (currentObject == null)
        {
            if (Input.GetKeyDown(interactKey))
                TryPickup();

            return;
        }

        UpdateHeldObject();
        HandleRotation();

        if (Input.GetKeyDown(releaseKey) || (releaseWithInteractKey && Input.GetKeyDown(interactKey)))
            ReleaseObject();
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

        Transform holdPoint = GetHoldPoint(carryObject);

        if (holdPoint == null)
        {
            Debug.LogWarning("CarryObjectController: HoldPointがありません");
            return;
        }

        currentObject = carryObject;
        currentObject.Hold(holdPoint);

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

    public void ReleaseObject()
    {
        if (currentObject == null)
            return;

        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        Transform origin = cam != null ? cam.transform : transform;

        CarryObject obj = currentObject;
        currentObject = null;

        obj.ReleaseFrom(origin);
        SetNormalItemControl(true);

        if (debugLog)
            Debug.Log("CarryObjectを手放した: " + obj.name);
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

    void SetNormalItemControl(bool enabled)
    {
        if (heldItemController != null)
            heldItemController.enabled = enabled;

        if (itemThrower != null)
            itemThrower.enabled = enabled;
    }
}
