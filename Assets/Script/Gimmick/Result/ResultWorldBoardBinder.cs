using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultWorldBoardBinder : MonoBehaviour
{
    [Header("看板に表示するTMP")]
    public TMP_Text boardTMP;

    [Header("予備: Legacy Text")]
    public Text boardText;

    [Header("日本語対応TMP Font Asset")]
    public TMP_FontAsset resultFont;

    [Header("表示設定")]
    public bool bindOnStart = true;
    public bool applyFontOnBind = true;

    [TextArea(4, 12)]
    public string fallbackText = "測定結果を取得できませんでした。";

    void Start()
    {
        if (bindOnStart)
            Bind();
    }

    public void Bind()
    {
        string result = fallbackText;

        if (RunResultAnalyzer.Instance != null)
            result = RunResultAnalyzer.Instance.BuildResultText();

        SetText(result);
    }

    void SetText(string value)
    {
        if (boardTMP != null)
        {
            if (applyFontOnBind && resultFont != null)
                boardTMP.font = resultFont;

            boardTMP.text = value;
        }

        if (boardText != null)
            boardText.text = value;
    }
}
