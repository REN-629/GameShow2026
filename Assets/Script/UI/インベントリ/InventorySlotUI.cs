// スロットUI：1枠分の表示と選択状態の見た目管理
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Text label;
    public GameObject selectedFrame;

    public void SetItem(ItemData item, bool selected)
    {
        if (label != null)
        {
            label.text = item == null ? "" : item.itemName;
        }

        if (selectedFrame != null)
        {
            selectedFrame.SetActive(selected);
        }
    }
}
