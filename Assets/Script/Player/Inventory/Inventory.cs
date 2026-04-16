// インベントリ本体：実体アイテムを固定スロットで保持し選択状態を管理
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int maxSlots = 4;
    public PickupItem[] slots;
    public int selectedIndex = 0;

    void Awake()
    {
        slots = new PickupItem[maxSlots];
    }

    public bool AddItem(PickupItem item)
    {
        if (item == null)
            return false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = item;
                item.SetStoredState();
                Debug.Log("取得: " + item.itemData.itemName + " / slot=" + i);
                return true;
            }
        }

        Debug.Log("持てない");
        return false;
    }

    public PickupItem GetItemAt(int index)
    {
        if (index < 0 || index >= slots.Length)
            return null;

        return slots[index];
    }

    public PickupItem GetSelectedItem()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Length)
            return null;

        return slots[selectedIndex];
    }

    public PickupItem RemoveSelectedItem()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Length)
            return null;

        PickupItem removed = slots[selectedIndex];
        slots[selectedIndex] = null;
        return removed;
    }

    public void SelectNext()
    {
        if (slots.Length == 0)
            return;

        selectedIndex++;
        if (selectedIndex >= slots.Length)
            selectedIndex = 0;
    }

    public void SelectPrevious()
    {
        if (slots.Length == 0)
            return;

        selectedIndex--;
        if (selectedIndex < 0)
            selectedIndex = slots.Length - 1;
    }
}
