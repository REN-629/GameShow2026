// HeldItemController：選択中アイテムを手に表示する
//
// 役割：
// ・Inventoryの選択中スロットを見る
// ・選択中アイテムをHeld状態にする
// ・HoldPointに追従させる
// ・左クリック使用
// ・右クリック回転
//
// 重要：
// 投げる時は ReleaseHeldItemForThrow() を使う。
// 投げる時に SetStoredState() を呼ばないようにするため。

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

        Vector3 usePos = Vector3.zero;
        Vector3 useRot = Vector3.zero;

        if (useAnimator != null)
        {
            usePos = useAnimator.CurrentPositionOffset;
            useRot = useAnimator.CurrentRotationOffset;
        }

        currentHeldItem.ForceHoldTransform(holdPoint, usePos, useRot);
    }

    void UpdateHeldItem()
    {
        if (inventory == null || holdPointManager == null)
            return;

        PickupItem selectedItem = inventory.GetSelectedItem();

        if (currentHeldItem == selectedItem)
            return;

        // 前に持っていたアイテムは収納状態へ
        // ただし投げる時は ReleaseHeldItemForThrow() で先に null にするため、ここは走らない。
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

    public void ReleaseHeldItemForThrow(PickupItem item)
    {
        if (currentHeldItem == item)
        {
            currentHeldItem = null;
        }

        IsRotatingItem = false;

        if (useAnimator != null)
        {
            useAnimator.ForceStop();
        }
    }
}
