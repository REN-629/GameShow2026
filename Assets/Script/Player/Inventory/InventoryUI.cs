// インベントリUI：スロット表示とマウスホイール選択を管理
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Inventory inventory;
    public List<InventorySlotUI> slots;

    void Update()
    {
        HandleScroll();
        Refresh();
    }

    void HandleScroll()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0) inventory.SelectPrevious();
        if (scroll < 0) inventory.SelectNext();
    }

    void Refresh()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < inventory.items.Count)
            {
                slots[i].SetItem(inventory.items[i], i == inventory.selectedIndex);
            }
            else
            {
                slots[i].SetEmpty();
            }
        }
    }
}
