using UnityEngine;

public class PasswordTerminalInteractor : MonoBehaviour
{
    [Header("対象端末")]
    public PasswordTerminal terminal;

    [Header("操作UI")]
    public GameObject terminalUI;

    [Header("プレイヤー")]
    public string playerTag = "Player";

    [Header("操作設定")]
    public KeyCode interactKey = KeyCode.E;
    public KeyCode closeKey = KeyCode.Escape;

    [Header("操作中に止めるBehaviour")]
    public Behaviour[] disableWhileInteracting;

    [Header("カーソル")]
    public bool showCursor = true;
    public CursorLockMode cursorLockModeOnOpen = CursorLockMode.None;
    public CursorLockMode cursorLockModeOnClose = CursorLockMode.Locked;

    [Header("状態")]
    public bool playerInRange = false;
    public bool isInteracting = false;

    [Header("デバッグ")]
    public bool debugLog = true;

    void Start()
    {
        SetUI(false);
    }

    void Update()
    {
        if (!isInteracting)
        {
            if (playerInRange && Input.GetKeyDown(interactKey))
                OpenTerminal();

            return;
        }

        if (Input.GetKeyDown(closeKey))
            CloseTerminal();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = true;

        if (debugLog)
            Debug.Log("端末操作可能");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = false;

        if (isInteracting)
            CloseTerminal();
    }

    public void OpenTerminal()
    {
        if (isInteracting)
            return;

        isInteracting = true;

        SetUI(true);
        SetPlayerControl(false);

        if (showCursor)
        {
            Cursor.visible = true;
            Cursor.lockState = cursorLockModeOnOpen;
        }

        if (debugLog)
            Debug.Log("端末操作開始");
    }

    public void CloseTerminal()
    {
        if (!isInteracting)
            return;

        isInteracting = false;

        SetUI(false);
        SetPlayerControl(true);

        if (showCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = cursorLockModeOnClose;
        }

        if (debugLog)
            Debug.Log("端末操作終了");
    }

    void SetUI(bool active)
    {
        if (terminalUI != null)
            terminalUI.SetActive(active);
    }

    void SetPlayerControl(bool enabled)
    {
        if (disableWhileInteracting == null)
            return;

        foreach (Behaviour behaviour in disableWhileInteracting)
        {
            if (behaviour != null)
                behaviour.enabled = enabled;
        }
    }
}
