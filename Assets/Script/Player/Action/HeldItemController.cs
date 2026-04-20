using UnityEngine;

// 手持ちアイテム管理：表示・使用・回転・回転中判定
public class HeldItemController : MonoBehaviour
{
    public Inventory inventory;
    public Transform holdPoint;

    private PickupItem currentHeldItem;

    // ← これが今回のエラー解決用
    public bool IsRotatingItem { get; private set; }

    void Update()
    {
        UpdateHeldItem();
        HandleUse();
        HandleRotation();
    }

    void LateUpdate()
    {
        if (currentHeldItem != null && holdPoint != null)
        {
            currentHeldItem.ForceHoldTransform(holdPoint);
        }
    }

    // =========================
    // アイテム切り替え
    // =========================
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
        }
    }

    // =========================
    // 左クリック使用
    // =========================
    void HandleUse()
    {
        if (currentHeldItem == null)
            return;

        if (Input.GetMouseButtonDown(0) && currentHeldItem.canUseWithLeftClick)
        {
            currentHeldItem.Use();
        }
    }

    // =========================
    // 右クリック回転
    // =========================
    void HandleRotation()
    {
        // 毎フレーム初期化
        IsRotatingItem = false;

        if (currentHeldItem == null)
            return;

        // 右クリック押してる間だけ回転
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

    // =========================
    // 外部アクセス用
    // =========================
    public PickupItem GetCurrentHeldItem()
    {
        return currentHeldItem;
    }

    public void ClearHeldReference()
    {
        currentHeldItem = null;
        IsRotatingItem = false;
    }
}