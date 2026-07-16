//エネルギー系のアイテム持ってるときのUI(残りエネルギーとか)
using UnityEngine;
using UnityEngine.UI;

public class HeldItemEnergyUI : MonoBehaviour
{
    public HeldItemController heldItemController;
    public GameObject root;
    public Image fillImage;
    public Text energyText;

    public float fillSmoothSpeed = 8f;

    public string label = "";
    public bool showOnOffState = true;

    void Update()
    {
        if (heldItemController == null)
        {
            SetVisible(false);
            return;
        }

        PickupItem item = heldItemController.GetCurrentHeldItem();

        if (item == null)
        {
            SetVisible(false);
            return;
        }

        //手に持ってる時だけ表示
        if (item.CurrentState != PickupItemState.Held)
        {
            SetVisible(false);
            return;
        }

        ToggleUseAction toggle =
            item.GetComponentInChildren<ToggleUseAction>(true);

        if (toggle == null)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);

        float rate = toggle.GetEnergyRate();

        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Lerp(
                fillImage.fillAmount,
                rate,
                Time.deltaTime * fillSmoothSpeed
            );
        }

        if (energyText != null)
        {
            string stateText = "";

            if (showOnOffState)
            {
                stateText = toggle.IsOn ? " " : " ";
            }

            if (toggle.IsEnergyEmpty())
            {
                energyText.text =
                    label + " 0" + stateText + " ";
            }
            else
            {
                energyText.text =
                    label + " "
                    + toggle.GetRemainingSecondsRounded()
                    + ""
                    + stateText;
            }
        }
    }

    void SetVisible(bool visible)
    {
        if (root != null)
        {
            root.SetActive(visible);
        }
    }
}
