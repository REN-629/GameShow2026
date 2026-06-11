using UnityEngine;

public class PasswordTerminalWorldButton : MonoBehaviour
{
    [Header("端末ボタン")]
    public PasswordTerminalButton button;

    [Header("押す演出")]
    public Transform buttonModel;
    public Vector3 pressedOffset = new Vector3(0f, -0.02f, 0f);
    public float returnSpeed = 10f;

    private Vector3 defaultLocalPosition;
    private bool pressed;

    void Start()
    {
        if (buttonModel == null)
            buttonModel = transform;

        defaultLocalPosition = buttonModel.localPosition;
    }

    void Update()
    {
        if (buttonModel == null)
            return;

        Vector3 target =
            pressed
            ? defaultLocalPosition + pressedOffset
            : defaultLocalPosition;

        buttonModel.localPosition =
            Vector3.Lerp(buttonModel.localPosition, target, Time.deltaTime * returnSpeed);

        pressed = false;
    }

    public void Press()
    {
        pressed = true;

        if (button != null)
            button.Press();
    }
}
