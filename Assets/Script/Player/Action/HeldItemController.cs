// 手持ち管理：選択中アイテムの表示、使用、右クリック回転を管理
using UnityEngine;

public class HeldItemController : MonoBehaviour
{
    public Inventory inventory;
    public Transform holdPoint;

    private PickupItem currentHeldItem;

    void Update()
    {
        UpdateHeldItem();
        HandleUse();
        HandleRotation();
    }

    void UpdateHeldItem()
    {
        if (inventory == null || holdPoint == null)
            return;

        PickupItem selectedItem = inventory.GetSelectedItem();

        if (currentHeldItem == selectedItem)
            return;

        if (currentHeldItem != null)
        {
            currentHeldItem.SetStoredState();
        }

        currentHeldItem = selectedItem;

        if (currentHeldItem != null)
        {
            currentHeldItem.SetHeldState(holdPoint);
            Debug.Log("手持ち表示: " + currentHeldItem.itemData.itemName);
        }
    }

    void HandleUse()
    {
        if (currentHeldItem == null)
            return;

        if (Input.GetMouseButtonDown(0) && currentHeldItem.canUseWithLeftClick)
        {
            currentHeldItem.Use();
        }
    }

    void HandleRotation()
    {
        if (currentHeldItem == null)
            return;

        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            Transform itemTransform = currentHeldItem.transform;

            itemTransform.localRotation *= Quaternion.Euler(
                itemTransform.InverseTransformDirection(transform.up) * mouseX * -currentHeldItem.rotateSpeed
            );

            itemTransform.localRotation *= Quaternion.Euler(
                itemTransform.InverseTransformDirection(transform.right) * mouseY * currentHeldItem.rotateSpeed
            );
        }
    }

    public PickupItem GetCurrentHeldItem()
    {
        return currentHeldItem;
    }

    public void ClearHeldReference()
    {
        currentHeldItem = null;
    }
}
