using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ResultSceneBootstrap
{
    private const string ResultSceneName = "Result";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Setup()
    {
        Scene scene = SceneManager.GetActiveScene();

        if (scene.name != ResultSceneName)
            return;

        SetupCursor();
        SetupResultBoards();
    }

    static void SetupCursor()
    {
        if (Object.FindFirstObjectByType<ResultCursorController>() != null)
            return;

        GameObject canvasObject = new GameObject(
            "ResultCursorCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GraphicRaycaster raycaster =
            canvasObject.GetComponent<GraphicRaycaster>();
        raycaster.enabled = false;

        GameObject cursorObject = new GameObject(
            "ResultCursor",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        cursorObject.transform.SetParent(canvasObject.transform, false);

        RectTransform cursorRect =
            cursorObject.GetComponent<RectTransform>();

        cursorRect.anchorMin = Vector2.zero;
        cursorRect.anchorMax = Vector2.zero;
        cursorRect.pivot = new Vector2(0f, 1f);
        cursorRect.sizeDelta = new Vector2(42f, 42f);

        Image image = cursorObject.GetComponent<Image>();
        image.raycastTarget = false;
        image.preserveAspect = true;

        ResultCursorController controller =
            canvasObject.AddComponent<ResultCursorController>();

        controller.cursorRoot = cursorRect;
        controller.cursorImage = image;
        controller.rayCamera = Camera.main;
    }

    static void SetupResultBoards()
    {
        ResultWorldBoardBinder[] binders =
            Object.FindObjectsByType<ResultWorldBoardBinder>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (ResultWorldBoardBinder binder in binders)
        {
            if (binder != null)
                binder.EnsureClickableBoard();
        }
    }
}
