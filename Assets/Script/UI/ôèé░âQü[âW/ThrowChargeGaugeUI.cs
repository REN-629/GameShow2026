using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThrowChargeGaugeUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject root;
    public Image fillImage;
    public Slider slider;

    [Header("Text")]
    public TextMeshProUGUI percentTMP;
    public Text percentText;

    [Header("表示")]
    public bool hideWhenZero = true;
    public string prefix = "";
    public string suffix = "%";

    void Start()
    {
        SetRate(0f);
    }

    public void SetRate(float rate)
    {
        rate = Mathf.Clamp01(rate);

        if (root != null && hideWhenZero)
            root.SetActive(rate > 0f);

        if (fillImage != null)
            fillImage.fillAmount = rate;

        if (slider != null)
            slider.value = rate;

        string value = prefix + Mathf.RoundToInt(rate * 100f) + suffix;

        if (percentTMP != null)
            percentTMP.text = value;

        if (percentText != null)
            percentText.text = value;
    }

    public void Hide()
    {
        SetRate(0f);

        if (root != null)
            root.SetActive(false);
    }
}
