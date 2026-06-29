using UnityEngine;

public class TutorialChecklistClearCondition : MonoBehaviour
{
    public TutorialPuzzleDoorOpener doorOpener;

    public bool requireMove;
    public bool requireJump;
    public bool requireItemPickup;
    public bool requireThrow;
    public bool requireUse;

    public string moveLabel = "移動する";
    public string jumpLabel = "ジャンプする";
    public string pickupLabel = "アイテムを拾う";
    public string throwLabel = "アイテムを投げる";
    public string useLabel = "アイテムを使う";

    public bool detectInput = true;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode pickupKey = KeyCode.E;
    public KeyCode throwKey = KeyCode.G;
    public int useMouseButton = 0;

    public bool hidePermanentlyAfterClear = true;

    private bool moved;
    private bool jumped;
    private bool pickedUp;
    private bool thrown;
    private bool used;
    private bool cleared;
    private bool activeInRoom;

    void Awake()
    {
        if (doorOpener == null)
            doorOpener = GetComponent<TutorialPuzzleDoorOpener>();

        if (doorOpener == null)
            doorOpener = GetComponentInParent<TutorialPuzzleDoorOpener>();
    }

    void Update()
    {
        if (cleared)
            return;

        if (!activeInRoom)
            return;

        if (detectInput)
            DetectInput();

        CheckClear();
    }

    public void ActivateChecklist()
    {
        if (hidePermanentlyAfterClear && cleared)
            return;

        activeInRoom = true;

        if (TutorialChecklistUI.Instance != null)
            TutorialChecklistUI.Instance.Bind(this);
    }

    public void DeactivateChecklist()
    {
        activeInRoom = false;

        if (TutorialChecklistUI.Instance != null)
            TutorialChecklistUI.Instance.Unbind(this);
    }

    void DetectInput()
    {
        if (requireMove && !moved)
        {
            float x = Mathf.Abs(Input.GetAxisRaw("Horizontal"));
            float z = Mathf.Abs(Input.GetAxisRaw("Vertical"));

            if (x > 0.01f || z > 0.01f)
                MarkMove();
        }

        if (requireJump && !jumped && Input.GetKeyDown(jumpKey))
            MarkJump();

        if (requireItemPickup && !pickedUp && Input.GetKeyDown(pickupKey))
            MarkItemPickup();

        if (requireThrow && !thrown && Input.GetKeyDown(throwKey))
            MarkThrow();

        if (requireUse && !used && Input.GetMouseButtonDown(useMouseButton))
            MarkUse();
    }

    public void MarkMove()
    {
        moved = true;
        RefreshUI();
    }

    public void MarkJump()
    {
        jumped = true;
        RefreshUI();
    }

    public void MarkItemPickup()
    {
        pickedUp = true;
        RefreshUI();
    }

    public void MarkThrow()
    {
        thrown = true;
        RefreshUI();
    }

    public void MarkUse()
    {
        used = true;
        RefreshUI();
    }

    void CheckClear()
    {
        if (requireMove && !moved)
            return;

        if (requireJump && !jumped)
            return;

        if (requireItemPickup && !pickedUp)
            return;

        if (requireThrow && !thrown)
            return;

        if (requireUse && !used)
            return;

        Clear();
    }

    public void Clear()
    {
        if (cleared)
            return;

        cleared = true;
        activeInRoom = false;

        if (TutorialChecklistUI.Instance != null)
            TutorialChecklistUI.Instance.ForceHide(this);

        if (doorOpener != null)
            doorOpener.Clear();
    }

    public bool IsCleared()
    {
        return cleared;
    }

    void RefreshUI()
    {
        if (!activeInRoom)
            return;

        if (TutorialChecklistUI.Instance != null)
            TutorialChecklistUI.Instance.Refresh(this);
    }

    public string BuildText()
    {
        string text = "";

        if (requireMove)
            text += Line(moved, moveLabel);

        if (requireJump)
            text += Line(jumped, jumpLabel);

        if (requireItemPickup)
            text += Line(pickedUp, pickupLabel);

        if (requireThrow)
            text += Line(thrown, throwLabel);

        if (requireUse)
            text += Line(used, useLabel);

        return text;
    }

    string Line(bool done, string label)
    {
        return (done ? "✓ " : "□ ") + label + "\n";
    }
}
