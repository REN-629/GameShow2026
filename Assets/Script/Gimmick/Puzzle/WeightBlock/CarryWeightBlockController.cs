using UnityEngine;

public class CarryWeightBlockController : MonoBehaviour
{
    [Header("参照")]
    public Camera playerCamera;
    public Transform holdPoint;

    [Header("持っている間止める通常アイテム操作")]
    public HeldItemController heldItemController;
    public ItemThrower itemThrower;

    [Header("入力")]
    public KeyCode interactKey = KeyCode.E;
    public KeyCode releaseKey = KeyCode.G;
    public int rotateMouseButton = 1;

    [Header("拾う")]
    public float pickupDistance = 3f;
    public LayerMask pickupMask = ~0;

    [Header("手放す")]
    public float dropForwardOffset = 1f;
    public float dropUpOffset = 0.2f;
    public float releaseForce = 2f;

    [Header("デバッグ")]
    public bool debugLog = true;

    private CarryWeightBlock currentBlock;

    public bool IsCarrying => currentBlock != null;

    void Update()
    {
        if (currentBlock == null)
        {
            if (Input.GetKeyDown(interactKey))
                TryPickup();

            return;
        }

        UpdateHeldBlock();
        HandleRotation();

        if (Input.GetKeyDown(releaseKey) || Input.GetKeyDown(interactKey))
            ReleaseBlock();
    }

    void TryPickup()
    {
        Camera cam = playerCamera != null ? playerCamera : Camera.main;

        if (cam == null)
            return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, pickupDistance, pickupMask, QueryTriggerInteraction.Collide))
            return;

        CarryWeightBlock block = hit.collider.GetComponentInParent<CarryWeightBlock>();

        if (block == null || block.isHeld)
            return;

        Pickup(block);
    }

    public void Pickup(CarryWeightBlock block)
    {
        if (block == null || currentBlock != null)
            return;

        currentBlock = block;
        currentBlock.Hold(holdPoint);

        SetNormalItemControl(false);

        if (debugLog)
            Debug.Log("重りブロックを持った: " + block.name);
    }

    void UpdateHeldBlock()
    {
        if (currentBlock == null || holdPoint == null)
            return;

        currentBlock.ForceHoldTransform(holdPoint);
    }

    void HandleRotation()
    {
        if (currentBlock == null || !currentBlock.canRotateWhileHeld)
            return;

        if (!Input.GetMouseButton(rotateMouseButton))
            return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        currentBlock.AddRotation(new Vector3(
            mouseY * currentBlock.rotateSpeed,
            -mouseX * currentBlock.rotateSpeed,
            0f
        ));
    }

    public void ReleaseBlock()
    {
        if (currentBlock == null)
            return;

        Camera cam = playerCamera != null ? playerCamera : Camera.main;

        Vector3 forward = transform.forward;
        Quaternion rotation = transform.rotation;

        if (cam != null)
        {
            forward = cam.transform.forward;
            rotation = cam.transform.rotation;
        }

        Vector3 position =
            transform.position
            + forward.normalized * dropForwardOffset
            + Vector3.up * dropUpOffset;

        Vector3 force = forward.normalized * releaseForce;

        CarryWeightBlock block = currentBlock;
        currentBlock = null;

        block.Release(position, rotation, force);

        SetNormalItemControl(true);

        if (debugLog)
            Debug.Log("重りブロックを手放した: " + block.name);
    }

    void SetNormalItemControl(bool enabled)
    {
        if (heldItemController != null)
            heldItemController.enabled = enabled;

        if (itemThrower != null)
            itemThrower.enabled = enabled;
    }
}
