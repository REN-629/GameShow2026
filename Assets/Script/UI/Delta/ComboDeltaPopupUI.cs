using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComboDeltaPopupUI : MonoBehaviour
{
    public GameObject root;
    public TextMeshProUGUI deltaTMP;
    public Text deltaText;
    public Image comboFillImage;

    public float showDuration = 5f;
    public float blinkStartTime = 1f;
    public float blinkInterval = 0.12f;

    public string plusPrefix = "+";
    public string minusPrefix = "";
    public string suffix = "";

    private int comboValue = 0;
    private float timer = 0f;
    private bool visible = false;

    void Start()
    {
        Hide();
    }

    void Update()
    {
        if (!visible)
            return;

        timer -= Time.deltaTime;

        if (comboFillImage != null)
            comboFillImage.fillAmount = Mathf.Clamp01(timer / showDuration);

        if (timer <= blinkStartTime)
        {
            float blink = Mathf.PingPong(Time.time / blinkInterval, 1f);
            SetAlpha(blink > 0.5f ? 1f : 0.25f);
        }
        else
        {
            SetAlpha(1f);
        }

        if (timer <= 0f)
            Hide();
    }

    public void AddDelta(int amount)
    {
        if (!visible)
        {
            comboValue = 0;
            Show();
        }

        comboValue += amount;
        timer = showDuration;
        UpdateText();
    }

    public void SetReasonDelta(string reason, float amount)
    {
        Show();
        timer = showDuration;

        string sign = amount >= 0f ? plusPrefix : minusPrefix;
        string value = reason + ":" + sign + Mathf.Abs(amount).ToString("0") + suffix;

        SetText(value);
    }

    void Show()
    {
        visible = true;

        if (root != null)
            root.SetActive(true);

        SetAlpha(1f);
    }

    void Hide()
    {
        visible = false;
        comboValue = 0;
        timer = 0f;

        if (root != null)
            root.SetActive(false);

        SetAlpha(1f);
    }

    void UpdateText()
    {
        string sign = comboValue >= 0 ? plusPrefix : minusPrefix;
        SetText(sign + Mathf.Abs(comboValue).ToString() + suffix);
    }

    void SetText(string value)
    {
        if (deltaTMP != null)
            deltaTMP.text = value;

        if (deltaText != null)
            deltaText.text = value;
    }

    void SetAlpha(float alpha)
    {
        if (deltaTMP != null)
        {
            Color c = deltaTMP.color;
            c.a = alpha;
            deltaTMP.color = c;
        }

        if (deltaText != null)
        {
            Color c = deltaText.color;
            c.a = alpha;
            deltaText.color = c;
        }

        if (comboFillImage != null)
        {
            Color c = comboFillImage.color;
            c.a = alpha;
            comboFillImage.color = c;
        }
    }
}
