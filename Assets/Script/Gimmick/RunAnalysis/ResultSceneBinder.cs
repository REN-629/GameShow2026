//リザルトシーン用
//シーンを跨いで残っているRunScoreManager と RunResultAnalyzerからUIに結果を渡す
using UnityEngine;
using UnityEngine.UI;
//using TMPro;


public class ResultSceneBinder : MonoBehaviour
{
    [Header("リザルト全文")]
    public Text resultText;

    [Header("個別表示")]
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
            RunScoreManager.Instance.BindUI(clearRoomText, bestRoomText);
        }

        if (resultText != null)
        {
            if (RunResultAnalyzer.Instance != null)
            {
                resultText.text = RunResultAnalyzer.Instance.BuildResultText();
            }
            else
            {
                resultText.text = "RunResultAnalyzer がありません";
            }
        }
    }
}
