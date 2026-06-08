//選択中のアイテムを手に表示する
using UnityEngine;

public class HeldItemController : MonoBehaviour
{
    public Inventory inventory;
    public HoldPointManager holdPointManager;

    [Header("使用時演出")]
    public HeldItemUseAnimator useAnimator;

    [Header("持ち替え演出")]
    public HeldItemSwitchAnimator switchAnimator;

    [Header("手持ちアイテム専用Layer")]
    public bool useHeldItemLayer = true;
    public string heldItemLayerName = "HeldItem";
    public string defaultLayerName = "Default";

    private PickupItem currentHeldItem;
    private PickupItem pendingItem;

    public bool IsRotatingItem { get; private set; }

    void Update()
    {
        UpdateHeldItem();
        HandleUse();
        HandleRotation();
    }

    void LateUpdate()
    {
        if (currentHeldItem == null || holdPointManager == null)
            return;

        Transform holdPoint = holdPointManager.GetHoldPoint(currentHeldItem.holdType);

        if (holdPoint == null)
            return;

        Vector3 usePos = Vector3.zero;
        Vector3 useRot = Vector3.zero;

        if (useAnimator != null)
        {
            usePos = useAnimator.CurrentPositionOffset;
            useRot = useAnimator.CurrentRotationOffset;
        }

        Vector3 switchPos = Vector3.zero;
        Vector3 switchRot = Vector3.zero;

        if (switchAnimator != null)
        {
            switchPos = switchAnimator.CurrentPositionOffset;
            switchRot = switchAnimator.CurrentRotationOffset;
        }

        currentHeldItem.ForceHoldTransform(
            holdPoint,
            usePos + switchPos,
            useRot + switchRot
        );
    }

    void UpdateHeldItem()
    {
        if (inventory == null || holdPointManager == null)
            return;

        PickupItem selectedItem = inventory.GetSelectedItem();

        if (currentHeldItem == selectedItem)
            return;

        if (pendingItem == selectedItem && switchAnimator != null && switchAnimator.IsSwitching)
            return;

        pendingItem = selectedItem;

        if (switchAnimator != null)
        {
            switchAnimator.StartSwitch(SwitchToPendingItem);
        }
        else
        {
            SwitchToPendingItem();
        }
    }

    void SwitchToPendingItem()
    {
        if (currentHeldItem != null)
        {
            RestoreItemLayer(currentHeldItem);
            currentHeldItem.SetStoredState();
        }

        if (useAnimator != null)
            useAnimator.ForceStop();

        currentHeldItem = pendingItem;
        pendingItem = null;

        if (currentHeldItem != null)
        {
            currentHeldItem.SetHeldState();
            ApplyHeldItemLayer(currentHeldItem);

            Transform holdPoint = holdPointManager.GetHoldPoint(currentHeldItem.holdType);

            if (holdPoint != null)
                currentHeldItem.ForceHoldTransform(holdPoint);
        }
    }

    void HandleUse()
    {
        if (currentHeldItem == null)
            return;

        if (switchAnimator != null && switchAnimator.IsSwitching)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        if (!currentHeldItem.canUseWithLeftClick)
            return;

        if (useAnimator != null && useAnimator.IsPlaying)
            return;

        currentHeldItem.Use();

        if (useAnimator != null)
            useAnimator.PlayUseAnimation(currentHeldItem.holdPose);
    }

    void HandleRotation()
    {
        IsRotatingItem = false;

        if (currentHeldItem == null)
            return;

        if (switchAnimator != null && switchAnimator.IsSwitching)
            return;

        if (!currentHeldItem.canRotate)
            return;

        if (useAnimator != null && useAnimator.IsPlaying)
            return;

        if (Input.GetMouseButton(1))
        {
            IsRotatingItem = true;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            currentHeldItem.AddHoldRotation(new Vector3(
                mouseY * currentHeldItem.rotateSpeed,
                -mouseX * currentHeldItem.rotateSpeed,
                0f
            ));
        }
    }

    void ApplyHeldItemLayer(PickupItem item)
    {
        if (!useHeldItemLayer || item == null)
            return;

        int layer = LayerMask.NameToLayer(heldItemLayerName);

        if (layer < 0)
        {
            Debug.LogWarning("Layerが見つかりません: " + heldItemLayerName);
            return;
        }

        SetLayerRecursively(item.gameObject, layer);
    }

    void RestoreItemLayer(PickupItem item)
    {
        if (item == null)
            return;

        int layer = LayerMask.NameToLayer(defaultLayerName);

        if (layer < 0)
        {
            Debug.LogWarning("Layerが見つかりません: " + defaultLayerName);
            return;
        }

        SetLayerRecursively(item.gameObject, layer);
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null)
            return;

        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    public PickupItem GetCurrentHeldItem()
    {
        return currentHeldItem;
    }

    public void ClearHeldReference()
    {
        if (currentHeldItem != null)
            RestoreItemLayer(currentHeldItem);

        currentHeldItem = null;
        pendingItem = null;
        IsRotatingItem = false;

        if (useAnimator != null)
            useAnimator.ForceStop();

        if (switchAnimator != null)
            switchAnimator.ForceStop();
    }

    public void ReleaseHeldItemForThrow(PickupItem item)
    {
        if (item != null)
            RestoreItemLayer(item);

        if (currentHeldItem == item)
            currentHeldItem = null;

        if (pendingItem == item)
            pendingItem = null;

        IsRotatingItem = false;

        if (useAnimator != null)
            useAnimator.ForceStop();

        if (switchAnimator != null)
            switchAnimator.ForceStop();
    }
}
