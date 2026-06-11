using UnityEngine;

public class PasswordTerminalButton : MonoBehaviour
{
    public PasswordTerminal terminal;

    [Header("ボタン種別")]
    public ButtonType buttonType = ButtonType.Digit;

    [Header("数字ボタンの場合")]
    public int digit = 0;

    public enum ButtonType
    {
        Digit,
        Submit,
        Clear,
        Backspace
    }

    public void Press()
    {
        if (terminal == null)
            return;

        switch (buttonType)
        {
            case ButtonType.Digit:
                terminal.InputDigit(digit);
                break;

            case ButtonType.Submit:
                terminal.Submit();
                break;

            case ButtonType.Clear:
                terminal.ClearInput();
                break;

            case ButtonType.Backspace:
                terminal.Backspace();
                break;
        }
    }
}
