using UnityEngine;
using TMPro;

[System.Obsolete("ResultWorldBoardBinderを推奨。古いUI表示互換用です。")]\npublic class RunResultUI : MonoBehaviour
{
    [Header("リザルト表示Text")]
    public TextMeshProUGUI resultText;

    [Header("開始時に非表示")]
    public bool hideOnStart = true;

    void Start()
    {
        if (hideOnStart)
            gameObject.SetActive(false);
    }

    public void ShowResult()
    {
        gameObject.SetActive(true);

        if (resultText == null)
            return;

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
