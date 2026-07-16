//InventoryWeightGaugeUI：重量ゲージを画面に表示する
//Inventory.GetWeightRate()を見てfillAmountを変える

using UnityEngine;
using UnityEngine.UI;

public class InventoryWeightGaugeUI : MonoBehaviour
{
    [Header("参照")]
    public Inventory inventory;

    [Header("ゲージ画像")]
    public Image fillImage;

    [Header("テキスト")]
    public Text weightText;

    [Header("表示設定")]
    public bool showMoveSpeedPercent = true;

    void Update()
    {
        if (inventory == null)
            return;

        float currentWeight = inventory.GetCurrentWeight();
        float maxWeight = inventory.maxWeight;
        float rate = inventory.GetWeightRate();

        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Lerp(
            fillImage.fillAmount,
            rate,
            Time.deltaTime * 8f //ゲージ増減時の速度補完(ヌルっと動かす速度)
            );
        }

        if (weightText != null)
        {
            if (showMoveSpeedPercent)
            {
                int speedPercent =
                    Mathf.RoundToInt(inventory.GetMoveSpeedMultiplier() * 100f);

                weightText.text =
                    
                    + Mathf.RoundToInt(currentWeight)
                    + " / "
                    + Mathf.RoundToInt(maxWeight);

                    //+ "  速度 "     速度表示はいらないかもなので消す
                    //+ speedPercent
                    //+ "%";
            }
            else
            {
                weightText.text =
                    
                    + Mathf.RoundToInt(currentWeight)
                    + " / "
                    + Mathf.RoundToInt(maxWeight);
            }
        }
    }
}
