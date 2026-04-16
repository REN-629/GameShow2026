// 手持ち管理：選択中アイテムの表示、使用、右クリック回転、指定距離追従を管理
using UnityEngine;

public class HeldItemController : MonoBehaviour
{
    public Inventory inventory;
    public Transform handPoint;

    [Header("手持ち追従距離")]
    public float forwardDistance = 0.5f;
    public float sideOffset = 0.15f;
    public float heightOffset = -0.1f;

    private PickupItem currentHeldItem;

    void Update()
    {
        UpdateHeldItem();
        HandleUse();
        HandleRotation();
    }

    void LateUpdate()
    {
        FollowHandPoint();
    }

    void UpdateHeldItem()
    {
        if (inventory == null || handPoint == null)
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
            currentHeldItem.SetHeldState();
            Debug.Log("手持ち表示: " + currentHeldItem.itemData.itemName);
        }
    }

    void FollowHandPoint()
    {
        if (currentHeldItem == null || handPoint == null)
            return;

        Vector3 targetPosition =
          handPoint.position
          + handPoint.forward * forwardDistance
          + handPoint.right * sideOffset
          + handPoint.up * heightOffset;

        currentHeldItem.transform.position = targetPosition;

        Quaternion targetRotation =
          handPoint.rotation * Quaternion.Euler(currentHeldItem.holdRotationOffset);

        currentHeldItem.transform.rotation = targetRotation;
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

            currentHeldItem.holdRotationOffset += new Vector3(
              mouseY * currentHeldItem.rotateSpeed,
              -mouseX * currentHeldItem.rotateSpeed,
              0f
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