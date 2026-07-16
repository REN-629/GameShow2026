using TMPro;
using UnityEngine;

public class ResultTMPFontSetter : MonoBehaviour
{
    [Header("日本語対応TMP Font Asset")]
    public TMP_FontAsset resultFont;

    [Header("Fallback Font Asset")]
    public TMP_FontAsset fallbackFont;

    [Header("対象Text")]
    public TMP_Text[] targetTexts;

    [Header("子から自動取得")]
    public bool autoCollectChildren = true;

    [Header("開始時に適用")]
    public bool applyOnStart = true;

    void Start()
    {
        if (applyOnStart)
            Apply();
    }

    public void Apply()
    {
        if (autoCollectChildren)
            targetTexts = GetComponentsInChildren<TMP_Text>(true);

        if (targetTexts == null)
            return;

        foreach (TMP_Text text in targetTexts)
        {
            if (text == null)
                continue;

            if (resultFont != null)
                text.font = resultFont;

            if (fallbackFont != null &&
                text.font != null &&
                !text.font.fallbackFontAssetTable.Contains(fallbackFont))
            {
                text.font.fallbackFontAssetTable.Add(fallbackFont);
            }

            text.SetAllDirty();
        }
    }
}
