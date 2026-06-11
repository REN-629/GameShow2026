using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PasswordTerminalRayInteractor : MonoBehaviour
{
    [Header("カメラ")]
    public Camera playerCamera;

    [Header("距離")]
    public float interactDistance = 3f;

    [Header("入力")]
    public KeyCode interactKey = KeyCode.E;

    [Header("対象Layer")]
    public LayerMask terminalMask = ~0;

    [Header("案内UI / TMP")]
    public TextMeshProUGUI promptTMP;

    [Header("案内UI / Legacy Text")]
    public Text promptText;

    [Header("表示文")]
    public string promptMessage = "[E] 端末を操作";

    private PasswordTerminalInteractor currentTarget;

    void Update()
    {
        FindTarget();
        UpdatePrompt();

        if (currentTarget != null && Input.GetKeyDown(interactKey))
        {
            currentTarget.OpenTerminal();
        }
    }

    void FindTarget()
    {
        currentTarget = null;

        Camera cam = playerCamera != null ? playerCamera : Camera.main;

        if (cam == null)
            return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, terminalMask, QueryTriggerInteraction.Collide))
        {
            currentTarget = hit.collider.GetComponentInParent<PasswordTerminalInteractor>();
        }
    }

    void UpdatePrompt()
    {
        bool show = currentTarget != null && !currentTarget.isInteracting;

        if (promptTMP != null)
        {
            promptTMP.gameObject.SetActive(show);
            promptTMP.text = promptMessage;
        }

        if (promptText != null)
        {
            promptText.gameObject.SetActive(show);
            promptText.text = promptMessage;
        }
    }
}
