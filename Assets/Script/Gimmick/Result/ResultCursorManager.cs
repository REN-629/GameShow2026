using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResultCursorManager : MonoBehaviour
{
    public static ResultCursorManager Instance { get; private set; }

    [Header("Cursor UI")]
    public RectTransform cursorRoot;
    public Image cursorImage;

    [Header("見た目")]
    public Sprite cursorSprite;
    public Vector2 cursorOffset = Vector2.zero;

    [Header("入力")]
    public Camera rayCamera;
    public LayerMask clickableMask = ~0;
    public float rayDistance = 100f;
    public KeyCode cancelKey = KeyCode.Escape;

    [Header("通常カーソル")]
    public bool hideSystemCursor = true;
    public bool unlockSystemCursor = true;

    [Header("状態")]
    public ResultBoardClickable activeBoard;

    [Header("デバッグ")]
    public bool debugLog = false;

    void Awake()
    {
        Instance = this;

        if (rayCamera == null)
            rayCamera = Camera.main;

        if (cursorImage != null && cursorSprite != null)
            cursorImage.sprite = cursorSprite;
    }

    void OnEnable()
    {
        if (unlockSystemCursor)
            Cursor.lockState = CursorLockMode.None;

        if (hideSystemCursor)
            Cursor.visible = false;
    }

    void OnDisable()
    {
        Cursor.visible = true;
    }

    void Update()
    {
        UpdateCursorPosition();
        HandleCancel();
        HandleClick();
    }

    void UpdateCursorPosition()
    {
        if (cursorRoot == null)
            return;

        cursorRoot.position = (Vector2)Input.mousePosition + cursorOffset;
    }

    void HandleCancel()
    {
        if (!Input.GetKeyDown(cancelKey))
            return;

        if (activeBoard != null)
            activeBoard.ReturnToOriginal();
    }

    void HandleClick()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        Camera cam = rayCamera != null ? rayCamera : Camera.main;

        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, clickableMask, QueryTriggerInteraction.Collide))
            return;

        ResultBoardClickable board =
            hit.collider.GetComponentInParent<ResultBoardClickable>();

        if (board == null)
            return;

        if (activeBoard != null && activeBoard != board)
        {
            if (debugLog)
                Debug.Log("他のボードを持っているためクリック無効");

            return;
        }

        board.ToggleFocus();
    }

    public bool CanInteract(ResultBoardClickable board)
    {
        return activeBoard == null || activeBoard == board;
    }

    public void SetActiveBoard(ResultBoardClickable board)
    {
        activeBoard = board;
    }

    public void ClearActiveBoard(ResultBoardClickable board)
    {
        if (activeBoard == board)
            activeBoard = null;
    }
}
