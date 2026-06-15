using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectPuzzleImageUI : MonoBehaviour
{
    public static CollectPuzzleImageUI Instance { get; private set; }

    [Header("Root")]
    public GameObject root;

    [Header("アイコン配置親")]
    public Transform iconParent;

    [Header("アイコンPrefab")]
    public CollectPuzzleIconUI iconPrefab;

    [Header("表示設定")]
    public bool hideWhenUnbound = true;

    private CollectPuzzleManager currentPuzzle;
    private readonly List<CollectPuzzleIconUI> icons =
        new List<CollectPuzzleIconUI>();

    void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Bind(CollectPuzzleManager puzzle)
    {
        if (puzzle == null)
            return;

        currentPuzzle = puzzle;

        BuildIcons(puzzle.GetTargetCount());
        Refresh(puzzle);
        Show();
    }

    public void Unbind(CollectPuzzleManager puzzle)
    {
        if (currentPuzzle != puzzle)
            return;

        currentPuzzle = null;

        if (hideWhenUnbound)
            Hide();
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
            bool collected = i < current;

            if (icons[i] != null)
                icons[i].SetCollected(collected);
        }
    }

    void BuildIcons(int count)
    {
        ClearIcons();

        if (iconPrefab == null || iconParent == null)
            return;

        for (int i = 0; i < count; i++)
        {
            CollectPuzzleIconUI icon =
                Instantiate(iconPrefab, iconParent);

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

    public void Show()
    {
        if (root != null)
            root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }
}
