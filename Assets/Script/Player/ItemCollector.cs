// アイテム取得：カメラ前方のPickupItemを拾ってインベントリに収納
using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    public Camera cam;
    public Inventory inventory;
    public float distance = 3f;
    public KeyCode pickupKey = KeyCode.E;

    void Update()
    {
        if (Input.GetKeyDown(pickupKey))
        {
            TryPickup();
        }
    }

    void TryPickup()
    {
        if (cam == null || inventory == null)
        {
            Debug.LogWarning("ItemCollector の参照が未設定");
            return;
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, distance))
        {
            PickupItem item = hit.collider.GetComponentInParent<PickupItem>();

            if (item != null)
            {
                inventory.AddItem(item);
            }
        }
    }
}
