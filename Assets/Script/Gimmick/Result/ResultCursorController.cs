using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResultCursorController : MonoBehaviour
{
    public static ResultCursorController Instance { get; private set; }

    [Header("UI")]
    public RectTransform cursorRoot;
    public Image cursorImage;

    [Header("画像")]
    public Sprite normalCursor;
    public Sprite inspectCursor;

    [Header("表示サイズ")]
    public Vector2 cursorSize = new Vector2(42f, 42f);

    [Header("マウス位置補正")]
    public Vector2 cursorOffset = new Vector2(4f, -4f);

    [Header("3Dクリック判定")]
    public Camera rayCamera;
    public LayerMask inspectableMask = ~0;
    public float rayDistance = 100f;

    [Header("入力")]
    public KeyCode cancelKey = KeyCode.Escape;

    [Header("設定")]
    public bool hideSystemCursor = true;
    public bool unlockCursor = true;

    private ResultBoardClickable focusedBoard;
    private ResultBoardClickable hoveredBoard;

    public bool HasFocusedObject => focusedBoard != null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (rayCamera == null)
            rayCamera = Camera.main;

        LoadDefaultSprites();

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

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        Cursor.visible = true;
    }

    void Update()
    {
        UpdateCursorPosition();
        UpdateHover();
        HandleClick();

        if (Input.GetKeyDown(cancelKey) && focusedBoard != null)
            focusedBoard.ReturnToOriginal();
    }

    void LoadDefaultSprites()
    {
        if (normalCursor == null)
            normalCursor = Resources.Load<Sprite>("ResultCursor/ResultCursor_Arrow");

        if (inspectCursor == null)
            inspectCursor = Resources.Load<Sprite>("ResultCursor/ResultCursor_Magnifier");
    }

    void UpdateCursorPosition()
    {
        if (cursorRoot == null)
            return;

        cursorRoot.position = (Vector2)Input.mousePosition + cursorOffset;
    }

    void UpdateHover()
    {
        hoveredBoard = null;

        Camera cam = rayCamera != null ? rayCamera : Camera.main;

        if (cam != null)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                rayDistance,
                inspectableMask,
                QueryTriggerInteraction.Collide))
            {
                hoveredBoard =
                    hit.collider.GetComponentInParent<ResultBoardClickable>();
            }
        }

        if (hoveredBoard != null &&
            (focusedBoard == null || focusedBoard == hoveredBoard))
        {
            SetInspectCursor();
        }
        else
        {
            SetNormalCursor();
        }
    }

    void HandleClick()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        if (hoveredBoard == null)
            return;

        if (!CanFocus(hoveredBoard))
            return;

        hoveredBoard.ToggleFocus();
    }

    public bool CanFocus(ResultBoardClickable board)
    {
        return focusedBoard == null || focusedBoard == board;
    }

    public void RegisterFocusedBoard(ResultBoardClickable board)
    {
        focusedBoard = board;
    }

    public void UnregisterFocusedBoard(ResultBoardClickable board)
    {
        if (focusedBoard == board)
            focusedBoard = null;
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
