// アイテム取得：カメラ前方のPickupItemを拾ってインベントリに追加
using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    public Camera cam;
    public Inventory inventory;
    public float distance = 3f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, distance))
            {
                PickupItem item = hit.collider.GetComponentInParent<PickupItem>();
                if (item != null && inventory.AddItem(item.itemData))
                {
                    Destroy(item.gameObject);
                }
            }
        }
    }
}
