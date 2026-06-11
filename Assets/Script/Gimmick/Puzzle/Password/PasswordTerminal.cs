using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PasswordTerminal : MonoBehaviour
{
    [Header("対象パズル")]
    public PasswordPuzzle passwordPuzzle;

    [Header("入力")]
    public string currentInput = "";

    [Header("UI / TMP")]
    public TextMeshProUGUI displayTMP;

    [Header("UI / Legacy Text")]
    public Text displayText;

    [Header("表示")]
    public string emptyDisplay = "----";
    public bool maskInput = false;
    public char maskChar = '*';

    [Header("入力制限")]
    public bool ignoreInputAfterSolved = true;

    [Header("デバッグ")]
    public bool debugLog = true;

    void Start()
    {
        UpdateDisplay();
    }

    public void InputDigit(int digit)
    {
        if (passwordPuzzle == null)
            return;

        if (ignoreInputAfterSolved && passwordPuzzle.solved)
            return;

        digit = Mathf.Clamp(digit, 0, 9);

        if (currentInput.Length >= passwordPuzzle.digitCount)
            return;

        currentInput += digit.ToString();
        UpdateDisplay();
    }

    public void Submit()
    {
        if (passwordPuzzle == null)
            return;

        bool success = passwordPuzzle.TrySubmit(currentInput);

        if (debugLog)
            Debug.Log("Password Submit: " + currentInput + " / success=" + success);

        if (!success)
            ClearInput();
    }

    public void ClearInput()
    {
        currentInput = "";
        UpdateDisplay();
    }

    public void Backspace()
    {
        if (string.IsNullOrEmpty(currentInput))
            return;

        currentInput = currentInput.Substring(0, currentInput.Length - 1);
        UpdateDisplay();
    }

    public void DamageTerminal()
    {
        if (passwordPuzzle != null)
            passwordPuzzle.ApplyDamage(1);
    }

    void UpdateDisplay()
    {
        string value = currentInput;

        if (string.IsNullOrEmpty(value))
            value = emptyDisplay;
        else if (maskInput)
            value = new string(maskChar, currentInput.Length);

        if (displayTMP != null)
            displayTMP.text = value;

        if (displayText != null)
            displayText.text = value;
    }
}
