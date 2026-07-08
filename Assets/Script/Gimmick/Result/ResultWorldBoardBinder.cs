using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultWorldBoardBinder : MonoBehaviour
{
    [Header("看板に表示するTextMeshPro")]
    public TextMeshPro boardTMP;

    [Header("予備: UI TextMeshPro")]
    public TextMeshProUGUI boardTMPUI;

    [Header("予備: Legacy Text")]
    public Text boardText;

    [Header("表示設定")]
    public bool bindOnStart = true;

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
            boardTMP.text = value;

        if (boardTMPUI != null)
            boardTMPUI.text = value;

        if (boardText != null)
            boardText.text = value;
    }
}
