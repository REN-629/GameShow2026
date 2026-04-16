// 投擲ゲージUI：G長押し中のチャージ量をImageのfillAmountに反映する
using UnityEngine;
using UnityEngine.UI;

public class ThrowChargeUI : MonoBehaviour
{
    public ItemThrower itemThrower;
    public Image fillImage;
    public GameObject rootObject;

    void Update()
    {
        if (itemThrower == null || fillImage == null)
            return;

        bool charging = itemThrower.IsCharging();
        float rate = itemThrower.GetChargeRate();

        fillImage.fillAmount = rate;

        if (rootObject != null)
        {
            rootObject.SetActive(charging);
        }
    }
}
