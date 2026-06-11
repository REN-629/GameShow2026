using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PasswordHintDisplay : MonoBehaviour
{
    [Header("対象パズル")]
    public PasswordPuzzle passwordPuzzle;

    [Header("UI / TMP")]
    public TextMeshProUGUI hintTMP;

    [Header("UI / Legacy Text")]
    public Text hintText;

    [Header("ヒント形式")]
    public HintType hintType = HintType.ShowDirectly;

    public enum HintType
    {
        ShowDirectly,
        SplitDigits,
        Reversed,
        SumOnly
    }

    void Start()
    {
        UpdateHint();
    }

    public void UpdateHint()
    {
        if (passwordPuzzle == null)
            return;

        string hint = BuildHint(passwordPuzzle.GetPassword());

        if (hintTMP != null)
            hintTMP.text = hint;

        if (hintText != null)
            hintText.text = hint;
    }

    string BuildHint(string pass)
    {
        if (string.IsNullOrEmpty(pass))
            return "";

        switch (hintType)
        {
            case HintType.ShowDirectly:
                return pass;

            case HintType.SplitDigits:
                return string.Join(" - ", pass.ToCharArray());

            case HintType.Reversed:
                char[] chars = pass.ToCharArray();
                System.Array.Reverse(chars);
                return new string(chars);

            case HintType.SumOnly:
                int sum = 0;

                foreach (char c in pass)
                {
                    if (char.IsDigit(c))
                        sum += c - '0';
                }

                return "SUM = " + sum;
        }

        return pass;
    }
}
