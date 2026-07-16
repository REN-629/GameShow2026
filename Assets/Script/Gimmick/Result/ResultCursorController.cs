using UnityEngine;
using UnityEngine.UI;

public class ResultCursorController : MonoBehaviour
{
    public static ResultCursorController Instance { get; private set; }

    public RectTransform cursorRoot;
    public Image cursorImage;
    public Sprite normalCursor;
    public Sprite inspectCursor;
    public Vector2 cursorSize = new Vector2(42f, 42f);
    public Vector2 cursorOffset = new Vector2(4f, -4f);
    public Camera rayCamera;
    public LayerMask inspectableMask = ~0;
    public float rayDistance = 100f;
    public KeyCode cancelKey = KeyCode.Escape;
    public bool hideSystemCursor = true;
    public bool unlockCursor = true;

    private ResultBoardClickable focusedBoard;
    private ResultExitDoorClickable activeDoor;

    public bool IsInteractionLocked => focusedBoard != null || activeDoor != null;

    void Awake()
    {
        Instance = this;

        if (rayCamera == null)
            rayCamera = Camera.main;

        if (cursorRoot != null)
            cursorRoot.sizeDelta = cursorSize;
    }

    void OnEnable()
    {
        if (unlockCursor)
            Cursor.lockState = CursorLockMode.None;

        Cursor.visible = !hideSystemCursor;
        SetNormalCursor();
    }

    void OnDisable()
    {
        Cursor.visible = true;
    }

    void Update()
    {
        if (cursorRoot != null)
            cursorRoot.position = (Vector2)Input.mousePosition + cursorOffset;

        MonoBehaviour hovered = RaycastInteractable();

        bool canInspect =
            hovered is ResultBoardClickable board && CanFocus(board) ||
            hovered is ResultExitDoorClickable door && CanUseDoor(door);

        if (canInspect)
            SetInspectCursor();
        else
            SetNormalCursor();

        if (Input.GetMouseButtonDown(0))
            HandleClick();

        if (Input.GetKeyDown(cancelKey) &&
            focusedBoard != null &&
            activeDoor == null)
        {
            focusedBoard.ReturnToOriginal();
        }
    }

    void HandleClick()
    {
        MonoBehaviour clicked = RaycastInteractable();

        if (clicked is ResultBoardClickable board)
        {
            if (CanFocus(board))
                board.ToggleFocus();

            return;
        }

        if (clicked is ResultExitDoorClickable door &&
            CanUseDoor(door))
        {
            door.Activate();
        }
    }

    MonoBehaviour RaycastInteractable()
    {
        Camera cam = rayCamera != null ? rayCamera : Camera.main;

        if (cam == null)
            return null;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            rayDistance,
            inspectableMask,
            QueryTriggerInteraction.Collide))
        {
            return null;
        }

        ResultBoardClickable board =
            hit.collider.GetComponentInParent<ResultBoardClickable>();

        if (board != null)
            return board;

        return hit.collider.GetComponentInParent<ResultExitDoorClickable>();
    }

    public bool CanFocus(ResultBoardClickable board)
    {
        return activeDoor == null &&
               (focusedBoard == null || focusedBoard == board);
    }

    public bool CanUseDoor(ResultExitDoorClickable door)
    {
        return focusedBoard == null &&
               (activeDoor == null || activeDoor == door);
    }

    public void RegisterFocusedBoard(ResultBoardClickable board)
    {
        focusedBoard = board;
        ResultInteractHint.RefreshAll();
    }

    public void UnregisterFocusedBoard(ResultBoardClickable board)
    {
        if (focusedBoard == board)
            focusedBoard = null;

        ResultInteractHint.RefreshAll();
    }

    public void RegisterActiveDoor(ResultExitDoorClickable door)
    {
        activeDoor = door;
        ResultInteractHint.RefreshAll();
    }

    void SetNormalCursor()
    {
        if (cursorImage != null && normalCursor != null)
            cursorImage.sprite = normalCursor;
    }

    void SetInspectCursor()
    {
        if (cursorImage != null && inspectCursor != null)
            cursorImage.sprite = inspectCursor;
    }
}
