using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultWorldBoardBinder : MonoBehaviour
{
    [Header("看板に表示するTMP")]
    public TMP_Text boardTMP;

    [Header("日本語対応TMP Font Asset")]
    public TMP_FontAsset resultFont;

    [Header("Fallback Font Asset")]
    public TMP_FontAsset fallbackFont;

    [Header("予備: Legacy Text")]
    public Text boardText;

    [Header("表示設定")]
    public bool bindOnStart = true;
    public bool applyFontOnBind = true;
    public bool addFallbackToFont = true;

    [Header("クリック対象")]
    public GameObject interactionRoot;
    public bool autoSetupClickableBoard = true;

    [TextArea(4, 12)]
    public string fallbackText =
        "測定結果を取得できませんでした。";

    void Awake()
    {
        if (autoSetupClickableBoard)
            EnsureClickableBoard();
    }

    void Start()
    {
        if (bindOnStart)
            Bind();
    }

    public void Bind()
    {
        string result = fallbackText;

        if (RunResultAnalyzer.Instance != null)
            result = RunResultAnalyzer.Instance.BuildResultText();

        ApplyFont();
        SetText(result);
    }

    public void ApplyFont()
    {
        if (boardTMP == null)
            return;

        if (applyFontOnBind && resultFont != null)
            boardTMP.font = resultFont;

        if (!addFallbackToFont ||
            fallbackFont == null ||
            boardTMP.font == null)
            return;

        if (!boardTMP.font.fallbackFontAssetTable.Contains(fallbackFont))
            boardTMP.font.fallbackFontAssetTable.Add(fallbackFont);
    }

    public void EnsureClickableBoard()
    {
        GameObject target = FindInteractionRoot();

        if (target == null)
            return;

        Collider col = target.GetComponent<Collider>();

        if (col == null)
        {
            BoxCollider box = target.AddComponent<BoxCollider>();
            Renderer renderer = target.GetComponentInChildren<Renderer>();

            if (renderer != null)
            {
                Bounds localBounds =
                    CalculateLocalBounds(target.transform, renderer.bounds);

                box.center = localBounds.center;
                box.size = localBounds.size;
            }
        }

        ResultBoardClickable clickable =
            target.GetComponent<ResultBoardClickable>();

        if (clickable == null)
            clickable = target.AddComponent<ResultBoardClickable>();

        if (clickable.moveTarget == null)
            clickable.moveTarget = target.transform;
    }

    GameObject FindInteractionRoot()
    {
        if (interactionRoot != null)
            return interactionRoot;

        Transform current = transform;

        while (current != null)
        {
            if (current.GetComponent<Collider>() != null)
            {
                interactionRoot = current.gameObject;
                return interactionRoot;
            }

            current = current.parent;
        }

        if (transform.parent != null)
        {
            interactionRoot = transform.parent.gameObject;
            return interactionRoot;
        }

        interactionRoot = gameObject;
        return interactionRoot;
    }

    Bounds CalculateLocalBounds(
        Transform root,
        Bounds worldBounds)
    {
        Vector3 center = root.InverseTransformPoint(worldBounds.center);
        Vector3 size = root.InverseTransformVector(worldBounds.size);

        size.x = Mathf.Abs(size.x);
        size.y = Mathf.Abs(size.y);
        size.z = Mathf.Abs(size.z);

        return new Bounds(center, size);
    }

    void SetText(string value)
    {
        if (boardTMP != null)
            boardTMP.text = value;

        if (boardText != null)
            boardText.text = value;
    }
}
