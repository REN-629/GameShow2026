// スロットUI：1枠分の表示と選択状態の見た目管理
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [Header("名前表示")]
    public Text label;

    [Header("アイコン表示")]
    public Image iconImage;

    [Header("空スロット時にアイコンを非表示")]
    public bool hideIconWhenEmpty = true;

    [Header("選択枠")]
    public GameObject selectedFrame;

    public void SetItem(PickupItem item, bool selected)
    {
        ItemData itemData = item != null ? item.itemData : null;

        if (label != null)
        {
            label.text = itemData == null ? "" : itemData.itemName;
        }

        if (iconImage != null)
        {
            Sprite icon = item != null ? item.itemIcon : null;

            iconImage.sprite = icon;

            if (hideIconWhenEmpty)
                iconImage.enabled = icon != null;
            else
                iconImage.enabled = true;
        }

        if (selectedFrame != null)
        {
            selectedFrame.SetActive(selected);
        }
    }

    // 古いInventoryUIから呼ばれても壊れない互換用
    public void SetItem(ItemData itemData, bool selected)
    {
        if (label != null)
        {
            label.text = itemData == null ? "" : itemData.itemName;
        }

        if (iconImage != null)
        {
            iconImage.sprite = null;

            if (hideIconWhenEmpty)
                iconImage.enabled = false;
        }

        if (selectedFrame != null)
        {
            selectedFrame.SetActive(selected);
        }
    }
}
