// ItemCollector：カメラ前方のアイテムを拾う
//
// 役割：
// ・SphereCastで少し太いレイを飛ばす
// ・PickupAreaやModelに当たっても、親のPickupItemを取得する
// ・EキーでInventoryに入れる

using UnityEngine;
using UnityEngine.UI;

public class ItemCollector : MonoBehaviour
{
    [Header("参照")]
    public Camera playerCamera;
    public Inventory inventory;

    [Header("拾う設定")]
    public KeyCode pickupKey = KeyCode.E;
    public float pickupDistance = 3f;
    public float pickupRadius = 0.25f;
    public LayerMask pickupLayerMask = ~0;

    [Header("UI")]
    public GameObject pickupGuideRoot;
    public Text pickupGuideText;

    private PickupItem currentTarget;

    void Update()
    {
        DetectPickupItem();
        UpdatePickupUI();
        HandlePickupInput();
    }

    void DetectPickupItem()
    {
        currentTarget = null;

        if (playerCamera == null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.SphereCast(
            ray,
            pickupRadius,
            out RaycastHit hit,
            pickupDistance,
            pickupLayerMask,
            QueryTriggerInteraction.Collide))
        {
            PickupItem item = hit.collider.GetComponentInParent<PickupItem>();

            if (item != null && item.CurrentState == PickupItemState.World)
            {
                currentTarget = item;
            }
        }
    }

    void HandlePickupInput()
    {
        if (currentTarget == null || inventory == null)
            return;

        if (Input.GetKeyDown(pickupKey))
        {
            bool success = inventory.AddItem(currentTarget);

            if (success)
                currentTarget = null;
        }
    }

    void UpdatePickupUI()
    {
        if (pickupGuideRoot == null)
            return;

        bool hasTarget = currentTarget != null;

        pickupGuideRoot.SetActive(hasTarget);

        if (hasTarget && pickupGuideText != null)
        {
            string itemName =
                currentTarget.itemData != null
                ? currentTarget.itemData.itemName
                : currentTarget.name;

            pickupGuideText.text = "E： " + itemName + " を拾う";
        }
    }

    public PickupItem GetCurrentTarget()
    {
        return currentTarget;
    }
}
