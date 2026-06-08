//選択中アイテムを手に表示する
//重要
//投げる時は ReleaseHeldItemForThrow() を使う
//投げる時に SetStoredState() を呼ばないようにするため
//
// 追加:
// ・壁押し戻し方式は削除
// ・持っている間だけアイテムを HeldItem Layer に変更
// ・収納/投げる/参照クリア時に元のLayerへ戻す

using UnityEngine;

public class HeldItemController : MonoBehaviour
{
    public Inventory inventory;
    public HoldPointManager holdPointManager;

    [Header("手持ちアイテム専用Layer")]
    public bool useHeldItemLayer = true;

    [Tooltip("手持ち中に切り替えるLayer名")]
    public string heldItemLayerName = "HeldItem";

    [Tooltip("手放した時に戻すLayer名")]
    public string defaultLayerName = "Default";

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

        currentHeldItem.ForceHoldTransform(
            holdPoint,
            usePos,
            useRot
        );
    }

    void UpdateHeldItem()
    {
        if (inventory == null || holdPointManager == null)
            return;

        PickupItem selectedItem = inventory.GetSelectedItem();

        if (currentHeldItem == selectedItem)
            return;

        // 前に持っていたアイテムは収納状態へ
        // ただし投げる時は ReleaseHeldItemForThrow() で先に null にするため、ここは走らない
        if (currentHeldItem != null)
        {
            RestoreItemLayer(currentHeldItem);
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
            ApplyHeldItemLayer(currentHeldItem);

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

    void ApplyHeldItemLayer(PickupItem item)
    {
        if (!useHeldItemLayer)
            return;

        if (item == null)
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
        {
            RestoreItemLayer(currentHeldItem);
        }

        currentHeldItem = null;
        IsRotatingItem = false;

        if (useAnimator != null)
        {
            useAnimator.ForceStop();
        }
    }

    public void ReleaseHeldItemForThrow(PickupItem item)
    {
        if (item != null)
        {
            RestoreItemLayer(item);
        }

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
