using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeltaPopupUI : MonoBehaviour
{
    [Header("UI / TMP")]
    public TextMeshProUGUI deltaTMP;

    [Header("UI / Legacy Text")]
    public Text deltaText;

    [Header("表示時間")]
    public float showDuration = 5f;

    [Header("点滅")]
    public float blinkDuration = 1f;
    public float blinkInterval = 0.12f;

    [Header("整数表示")]
    public bool roundToInt = true;

    [Header("プラス記号")]
    public bool showPlusSign = true;

    private float currentDelta = 0f;
    private float timer = 0f;
    private Coroutine hideRoutine;

    void Start()
    {
        SetVisible(false);
    }

    public void AddDelta(float amount)
    {
        currentDelta += amount;
        timer = showDuration;

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        UpdateText();
        SetVisible(true);

        hideRoutine = StartCoroutine(HideRoutine());
    }

    public void Clear()
    {
        currentDelta = 0f;

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        SetVisible(false);
    }

    System.Collections.IEnumerator HideRoutine()
    {
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        float blinkTimer = 0f;
        bool visible = true;

        while (blinkTimer < blinkDuration)
        {
            visible = !visible;
            SetVisible(visible);

            yield return new WaitForSeconds(blinkInterval);
            blinkTimer += blinkInterval;
        }

        currentDelta = 0f;
        SetVisible(false);
    }

    void UpdateText()
    {
        string sign = "";

        if (currentDelta > 0f && showPlusSign)
            sign = "+";

        string value =
            roundToInt
            ? Mathf.RoundToInt(currentDelta).ToString()
            : currentDelta.ToString("F1");

        string text = sign + value;

        if (deltaTMP != null)
            deltaTMP.text = text;

        if (deltaText != null)
            deltaText.text = text;
    }

    void SetVisible(bool visible)
    {
        if (deltaTMP != null)
            deltaTMP.gameObject.SetActive(visible);

        if (deltaText != null)
            deltaText.gameObject.SetActive(visible);
    }
}
