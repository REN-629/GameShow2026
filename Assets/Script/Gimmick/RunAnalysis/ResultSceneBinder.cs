//リザルトテキスト用
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultSceneBinder : MonoBehaviour
{
    [Header("リザルト全文 / TMP")]
    public TextMeshProUGUI resultTMP;

    [Header("個別表示 / TMP")]
    public TextMeshProUGUI clearRoomTMP;
    public TextMeshProUGUI bestRoomTMP;

    [Header("リザルト全文 / Legacy Text")]
    public Text resultText;

    [Header("個別表示 / Legacy Text")]
    public Text clearRoomText;
    public Text bestRoomText;

    void Start()
    {
        Bind();
    }

    public void Bind()
    {
        if (RunScoreManager.Instance != null)
        {
            RunScoreManager.Instance.BindTMP(clearRoomTMP, bestRoomTMP);
            RunScoreManager.Instance.BindLegacyText(clearRoomText, bestRoomText);
        }

        string result = "何らかの不具合で測定不能";

        if (RunResultAnalyzer.Instance != null)
            result = RunResultAnalyzer.Instance.BuildResultText();

        if (resultTMP != null)
            resultTMP.text = result;

        if (resultText != null)
            resultText.text = result;
    }
}
