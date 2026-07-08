using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultSceneBinder : MonoBehaviour
{
    [Header("推奨: ワールド看板Binder")]
    public ResultWorldBoardBinder worldBoardBinder;

    [Header("リザルト全文 / TMP UI 互換用")]
    public TextMeshProUGUI resultTMP;

    [Header("個別表示 / TMP")]
    public TextMeshProUGUI clearRoomTMP;
    public TextMeshProUGUI bestRoomTMP;

    [Header("リザルト全文 / Legacy Text 互換用")]
    public Text resultText;

    [Header("個別表示 / Legacy Text")]
    public Text clearRoomText;
    public Text bestRoomText;

    [Header("開始時にUI全文を非表示")]
    public bool hideLegacyResultUIOnStart = true;

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

        if (worldBoardBinder != null)
            worldBoardBinder.Bind();

        string result = "何らかの不具合で測定不能";

        if (RunResultAnalyzer.Instance != null)
            result = RunResultAnalyzer.Instance.BuildResultText();

        if (resultTMP != null)
        {
            resultTMP.text = result;

            if (hideLegacyResultUIOnStart)
                resultTMP.gameObject.SetActive(false);
        }

        if (resultText != null)
        {
            resultText.text = result;

            if (hideLegacyResultUIOnStart)
                resultText.gameObject.SetActive(false);
        }
    }
}
