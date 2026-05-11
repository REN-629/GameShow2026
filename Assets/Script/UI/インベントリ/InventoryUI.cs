// インベントリUI：固定スロット表示とマウスホイール選択を管理
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Inventory inventory;
    public List<InventorySlotUI> slots = new List<InventorySlotUI>();

    void Update()
    {
        if (inventory == null)
            return;

        HandleScroll();
        Refresh();
    }

    void HandleScroll()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (scroll > 0f)
            inventory.SelectPrevious();

        if (scroll < 0f)
            inventory.SelectNext();
    }

    void Refresh()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            PickupItem item = inventory.GetItemAt(i);
            bool selected = (i == inventory.selectedIndex);

            if (item == null)
            {
                slots[i].SetItem(null, selected);
            }
            else
            {
                slots[i].SetItem(item.itemData, selected);
            }
        }
    }
}
