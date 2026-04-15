//インベントリ本体：アイテム管理と選択状態を保持
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<ItemData> items = new List<ItemData>();
    public int maxSlots = 4;
    public int selectedIndex = 0;

    public bool AddItem(ItemData item)
    {
        if (items.Count >= maxSlots) return false;
        items.Add(item);
        if (items.Count == 1) selectedIndex = 0;
        return true;
    }

    public ItemData GetSelectedItem()
    {
        if (items.Count == 0) return null;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, items.Count - 1);
        return items[selectedIndex];
    }

    public void SelectNext()
    {
        if (items.Count == 0) return;
        selectedIndex = (selectedIndex + 1) % items.Count;
    }

    public void SelectPrevious()
    {
        if (items.Count == 0) return;
        selectedIndex--;
        if (selectedIndex < 0) selectedIndex = items.Count - 1;
    }
}
