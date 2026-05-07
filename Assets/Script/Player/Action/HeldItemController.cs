// 手持ち管理：選択中アイテムの表示、使用、右クリック回転、HoldPoint選択を管理
using UnityEngine;

public class HeldItemController : MonoBehaviour
{
    public Inventory inventory;
    public HoldPointManager holdPointManager;

    [Header("使用時演出")]
    public HeldItemUseAnimator useAnimator;

    private PickupItem currentHeldItem;

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

        if (useAnimator != null)
        {
            currentHeldItem.ForceHoldTransform(
                holdPoint,
                useAnimator.CurrentPositionOffset,
                useAnimator.CurrentRotationOffset
            );
        }
        else
        {
            currentHeldItem.ForceHoldTransform(holdPoint);
        }
    }

    void UpdateHeldItem()
    {
        if (inventory == null || holdPointManager == null)
            return;

        PickupItem selectedItem = inventory.GetSelectedItem();

        if (currentHeldItem == selectedItem)
            return;

        if (currentHeldItem != null)
        {
            currentHeldItem.SetStoredState();
        }

        if (useAnimator != null)
        {
            useAnimator.ForceStop();
        }

        currentHeldItem = selectedItem;

        if (currentHeldItem != null)
        {
            currentHeldItem.SetHeldState();

            Transform holdPoint = holdPointManager.GetHoldPoint(currentHeldItem.holdType);

            if (holdPoint != null)
            {
                currentHeldItem.ForceHoldTransform(holdPoint);
            }
        }
    }

    void HandleUse()
    {
        if (currentHeldItem == null)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        if (!currentHeldItem.canUseWithLeftClick)
            return;

        // 使用演出中は次の使用を受け付けない
        if (useAnimator != null && useAnimator.IsPlaying)
            return;

        currentHeldItem.Use();

        if (useAnimator != null)
        {
            useAnimator.PlayUseAnimation(currentHeldItem.holdPose);
        }
    }

    void HandleRotation()
    {
        IsRotatingItem = false;

        if (currentHeldItem == null)
            return;

        if (!currentHeldItem.canRotate)
            return;

        // 使用演出中は回転させない
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

    public PickupItem GetCurrentHeldItem()
    {
        return currentHeldItem;
    }

    public void ClearHeldReference()
    {
        currentHeldItem = null;
        IsRotatingItem = false;

        if (useAnimator != null)
        {
            useAnimator.ForceStop();
        }
    }
}
