//インベントリと重量で変動する移動速度

//PickupItem.itemWeight を全部足して合計重量を出す
//最大重量100なら合計100で移動倍率0
//InventoryWeightGaugeUI はこの重量を見てゲージを動かす

using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("スロット")]
    public int maxSlots = 4;
    public PickupItem[] slots;
    public int selectedIndex = 0;

    [Header("重量システム")]
    public float maxWeight = 100f;

    [Tooltip("最大重量以上なら移動不可にする")]
    public bool stopMovementAtMaxWeight = true;

    [Tooltip("重量0の時の速度倍率")]
    public float lightMoveMultiplier = 1f;

    [Tooltip("重量100の時の速度倍率。0なら完全停止")]
    public float heavyMoveMultiplier = 0f;

    [Header("あとから拡張用：重量カーブ")]
    public AnimationCurve weightMoveCurve;

    void Awake()
    {
        slots = new PickupItem[maxSlots];
    }

    public bool AddItem(PickupItem item)
    {
        if (item == null)
            return false;

        if (!CanAddItemByWeight(item))
        {
            Debug.Log("重すぎて持てない: " + GetItemName(item));
            return false;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = item;

                //item.SetStoredState();　下の使うのでいらない
                bool willBeSelectedSlot = i == selectedIndex;
                item.SetStoredState(!willBeSelectedSlot);


                Debug.Log("取得: " + GetItemName(item) + " / slot=" + i);
                Debug.Log("重量: " + GetCurrentWeight() + " / " + maxWeight);

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

        Debug.Log("重量: " + GetCurrentWeight() + " / " + maxWeight);

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

    public float GetCurrentWeight()
    {
        float total = 0f;

        foreach (PickupItem item in slots)
        {
            if (item != null)
                total += Mathf.Max(0f, item.itemWeight);
        }

        return total;
    }

    public float GetWeightRate()
    {
        if (maxWeight <= 0f)
            return 0f;

        return Mathf.Clamp01(GetCurrentWeight() / maxWeight);
    }

    public bool IsOverWeight()
    {
        return GetCurrentWeight() >= maxWeight;
    }

    public bool CanAddItemByWeight(PickupItem item)
    {
        if (item == null)
            return false;

        float nextWeight = GetCurrentWeight() + Mathf.Max(0f, item.itemWeight);

        return nextWeight <= maxWeight;
    }

    public float GetMoveSpeedMultiplier()
    {
        if (stopMovementAtMaxWeight && IsOverWeight())
            return 0f;

        float rate = GetWeightRate();

        if (weightMoveCurve != null && weightMoveCurve.length > 0)
            return Mathf.Clamp01(weightMoveCurve.Evaluate(rate));

        return Mathf.Lerp(lightMoveMultiplier, heavyMoveMultiplier, rate);
    }

    string GetItemName(PickupItem item)
    {
        if (item == null)
            return "null";

        if (item.itemData != null)
            return item.itemData.itemName;

        return item.name;
    }
}
