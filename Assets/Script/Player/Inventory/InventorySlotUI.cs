// スロットUI：1枠分の表示と選択状態の見た目管理
using UnityEngine;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    public TextMeshProUGUI label;
    public GameObject selectedFrame;

    public void SetItem(ItemData item, bool selected)
    {
        label.text = item.itemName;
        selectedFrame.SetActive(selected);
    }

    public void SetEmpty()
    {
        label.text = "";
        selectedFrame.SetActive(false);
    }
}
