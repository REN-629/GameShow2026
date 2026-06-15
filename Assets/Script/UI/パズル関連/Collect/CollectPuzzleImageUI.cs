using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectPuzzleImageUI : MonoBehaviour
{
    public static CollectPuzzleImageUI Instance { get; private set; }

    public RectTransform root;
    public Transform iconParent;
    public CollectPuzzleIconUI iconPrefab;

    public Vector2 shownPosition = new Vector2(0f, 120f);
    public Vector2 hiddenPosition = new Vector2(0f, -120f);
    public float moveDuration = 0.25f;
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private CollectPuzzleManager currentPuzzle;
    private readonly List<CollectPuzzleIconUI> icons = new List<CollectPuzzleIconUI>();
    private Coroutine moveRoutine;

    void Awake()
    {
        Instance = this;

        if (root != null)
            root.anchoredPosition = hiddenPosition;
    }

    public void Bind(CollectPuzzleManager puzzle)
    {
        if (puzzle == null || puzzle.IsCompleted())
            return;

        currentPuzzle = puzzle;
        BuildIcons(puzzle.GetTargetCount());
        Refresh(puzzle);
        MoveTo(shownPosition);
    }

    public void Unbind(CollectPuzzleManager puzzle)
    {
        if (currentPuzzle != puzzle)
            return;

        currentPuzzle = null;
        MoveTo(hiddenPosition);
    }

    public void ForceHide(CollectPuzzleManager puzzle)
    {
        if (currentPuzzle != puzzle)
            return;

        currentPuzzle = null;
        MoveTo(hiddenPosition);
    }

    public void Refresh(CollectPuzzleManager puzzle)
    {
        if (currentPuzzle != puzzle)
            return;

        int current = puzzle.GetCurrentCount();
        int target = puzzle.GetTargetCount();

        if (icons.Count != target)
            BuildIcons(target);

        for (int i = 0; i < icons.Count; i++)
        {
            if (icons[i] != null)
                icons[i].SetCollected(i < current);
        }

        if (puzzle.IsCompleted())
            ForceHide(puzzle);
    }

    void BuildIcons(int count)
    {
        ClearIcons();

        if (iconPrefab == null || iconParent == null)
            return;

        for (int i = 0; i < count; i++)
        {
            CollectPuzzleIconUI icon = Instantiate(iconPrefab, iconParent);
            icon.SetCollected(false);
            icons.Add(icon);
        }
    }

    void ClearIcons()
    {
        foreach (CollectPuzzleIconUI icon in icons)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }

        icons.Clear();
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
