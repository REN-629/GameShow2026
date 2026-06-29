using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialChecklistUI : MonoBehaviour
{
    public static TutorialChecklistUI Instance { get; private set; }

    public RectTransform root;
    public Text checklistText;

    public Vector2 shownPosition = new Vector2(0f, 120f);
    public Vector2 hiddenPosition = new Vector2(0f, -120f);
    public float moveDuration = 0.25f;
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private TutorialChecklistClearCondition currentChecklist;
    private Coroutine moveRoutine;

    void Awake()
    {
        Instance = this;

        if (root != null)
            root.anchoredPosition = hiddenPosition;

        if (checklistText != null)
            checklistText.text = "";
    }

    public void Bind(TutorialChecklistClearCondition checklist)
    {
        if (checklist == null)
            return;

        if (checklist.IsCleared())
            return;

        currentChecklist = checklist;
        SetText(checklist.BuildText());
        MoveTo(shownPosition);
    }

    public void Refresh(TutorialChecklistClearCondition checklist)
    {
        if (currentChecklist != checklist)
            return;

        if (checklist == null)
            return;

        SetText(checklist.BuildText());
    }

    public void Unbind(TutorialChecklistClearCondition checklist)
    {
        if (currentChecklist != checklist)
            return;

        currentChecklist = null;
        MoveTo(hiddenPosition);
    }

    public void ForceHide(TutorialChecklistClearCondition checklist)
    {
        if (currentChecklist != checklist)
            return;

        currentChecklist = null;
        MoveTo(hiddenPosition);
    }

    void SetText(string text)
    {
        if (checklistText != null)
            checklistText.text = text;
    }

    void MoveTo(Vector2 target)
    {
        if (root == null)
            return;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveRoutine(target));
    }

    IEnumerator MoveRoutine(Vector2 target)
    {
        Vector2 start = root.anchoredPosition;
        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / moveDuration);
            float e = moveCurve != null ? moveCurve.Evaluate(t) : t;
            root.anchoredPosition = Vector2.LerpUnclamped(start, target, e);
            yield return null;
        }

        root.anchoredPosition = target;
        moveRoutine = null;
    }
}
