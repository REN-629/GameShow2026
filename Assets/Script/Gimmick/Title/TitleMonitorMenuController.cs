using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleMonitorMenuController : MonoBehaviour
{
    public enum MenuPhase
    {
        Main,
        ModeSelect,
        Starting
    }

    public enum StartMode
    {
        Tutorial,
        Easy,
        Hard
    }

    [Header("TV Canvas")]
    public Canvas tvCanvas;
    public RawImage roomRawImage;
    public Texture tutorialRawTexture;
    public Texture startRawTexture;
    public TMP_FontAsset menuFont;

    [Header("Scene Names")]
    public string tutorialSceneName;
    public string easySceneName;
    public string hardSceneName;

    [Header("Spawn")]
    public GameObject tutorialSpawnPrefab;
    public GameObject easySpawnPrefab;
    public GameObject hardSpawnPrefab;
    public Transform spawnPoint;

    [Header("Main Camera")]
    public Camera mainCamera;
    public Transform cameraTarget;
    [Min(0f)] public float cameraMoveDuration = 1.6f;
    public AnimationCurve cameraMoveCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Opening Fade In")]
    [Min(0f)] public float openingFadeDuration = 1.2f;

    [Header("Start Sequence")]
    [Min(0f)] public float rawSwitchDelay = 0.5f;
    [Min(0f)] public float flickerDuration = 0.8f;
    [Min(0.01f)] public float flickerInterval = 0.06f;
    [Min(0f)] public float fadeOutDuration = 1f;
    [Min(0f)] public float sceneLoadDelay = 0.1f;
    public bool useUnscaledTime = true;

    [Header("Button Audio")]
    public AudioClip selectSE;
    public AudioClip backSE;
    public AudioClip startSE;
    [Range(0f, 1f)] public float seVolume = 1f;

    [Header("Cursor")]
    public Sprite normalCursorSprite;
    public Sprite hoverCursorSprite;
    public Vector2 cursorSize = new Vector2(18f, 18f);
    public Vector2 cursorOffset = new Vector2(3f, -3f);
    public bool hideSystemCursor = true;

    [Header("Button Layout")]
    public Vector2 buttonSize = new Vector2(190f, 42f);
    public float buttonSpacing = 12f;
    public float buttonFontSize = 24f;

    [Header("Runtime State")]
    [SerializeField] private MenuPhase phase = MenuPhase.Main;

    private RectTransform tvRect;
    private RectTransform cursorRect;
    private Image cursorImage;
    private GameObject mainPanel;
    private GameObject modePanel;
    private RawImage tutorialRawImage;
    private RawImage startRawImage;
    private Image monitorFlickerImage;
    private Image fullScreenFadeImage;
    private AudioSource audioSource;
    private Coroutine transitionRoutine;
    private bool pointerInsideMonitor;
    private int tutorialHoverCount;

    public MenuPhase Phase => phase;
    public bool IsStarting => phase == MenuPhase.Starting;

    void Awake()
    {
        Debug.Log(
            "TitleMonitorMenuController: Awake開始",
            this
        );

        ResolveReferences();
        BuildRuntimeObjects();
        ConfigureInitialState();

        Debug.Log(
            $"TitleMonitorMenuController: 初期化完了 / TV={tvCanvas != null} / Raw={roomRawImage != null}",
            this
        );
    }

    void Start()
    {
        StartCoroutine(FadeInRoutine());
    }

    void OnEnable()
    {
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
        UpdateMonitorCursor();
    }

    void ResolveReferences()
    {
        if (tvCanvas == null)
        {
            GameObject tvObject = GameObject.Find("TV");

            if (tvObject != null)
                tvCanvas = tvObject.GetComponent<Canvas>();
        }

        if (tvCanvas != null)
            tvRect = tvCanvas.transform as RectTransform;

        if (roomRawImage == null && tvCanvas != null)
            roomRawImage = tvCanvas.GetComponentInChildren<RawImage>(true);

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (menuFont == null)
        {
            TMP_Text existingText =
                Object.FindFirstObjectByType<TMP_Text>(
                    FindObjectsInactive.Include
                );

            if (existingText != null)
                menuFont = existingText.font;
        }

        if (normalCursorSprite == null)
            normalCursorSprite =
                Resources.Load<Sprite>(
                    "ResultCursor/ResultCursor_Arrow"
                );

        if (hoverCursorSprite == null)
            hoverCursorSprite =
                Resources.Load<Sprite>(
                    "ResultCursor/ResultCursor_Magnifier"
                );

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    void BuildRuntimeObjects()
    {
        if (tvCanvas == null)
        {
            Debug.LogError(
                "TitleMonitorMenuController: TV Canvasがありません。",
                this
            );
            return;
        }

        if (tvCanvas.renderMode == RenderMode.WorldSpace &&
            tvCanvas.worldCamera == null)
        {
            tvCanvas.worldCamera = mainCamera;
        }

        GraphicRaycaster raycaster =
            tvCanvas.GetComponent<GraphicRaycaster>();

        if (raycaster != null)
            raycaster.enabled = true;

        CreateExclusiveRawImages();
        CreateMonitorFlicker();
        CreateMenuPanels();
        CreateMonitorCursor();
        CreateFullScreenFade();
    }

    void CreateExclusiveRawImages()
    {
        if (roomRawImage == null)
            return;

        tutorialRawImage =
            CreateRawClone("TutorialRawImage", tutorialRawTexture);

        startRawImage =
            CreateRawClone("StartRawImage", startRawTexture);

        roomRawImage.raycastTarget = false;
        tutorialRawImage.raycastTarget = false;
        startRawImage.raycastTarget = false;

        roomRawImage.transform.SetAsFirstSibling();
        tutorialRawImage.transform.SetAsFirstSibling();
        startRawImage.transform.SetAsFirstSibling();
    }

    RawImage CreateRawClone(string objectName, Texture texture)
    {
        GameObject clone =
            Instantiate(roomRawImage.gameObject, roomRawImage.transform.parent);

        clone.name = objectName;

        RawImage raw = clone.GetComponent<RawImage>();
        raw.texture = texture;
        raw.color = Color.white;

        clone.SetActive(false);
        return raw;
    }

    void CreateMonitorFlicker()
    {
        GameObject flickerObject =
            new GameObject(
                "MonitorFlicker",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

        flickerObject.transform.SetParent(tvCanvas.transform, false);

        RectTransform rect =
            flickerObject.GetComponent<RectTransform>();

        StretchFull(rect);

        monitorFlickerImage =
            flickerObject.GetComponent<Image>();

        monitorFlickerImage.color =
            new Color(1f, 1f, 1f, 0f);

        monitorFlickerImage.raycastTarget = false;
        flickerObject.transform.SetAsLastSibling();
    }

    void CreateMenuPanels()
    {
        mainPanel = CreatePanel("MainMenuPanel");
        modePanel = CreatePanel("ModeSelectPanel");

        CreateButton(
            mainPanel.transform,
            "モード選択",
            new Vector2(0f, 30f),
            OpenModeSelect
        );

        TitleMenuHoverRelay tutorialRelay =
            CreateButton(
                mainPanel.transform,
                "チュートリアル",
                new Vector2(0f, -30f),
                StartTutorial
            );

        tutorialRelay.onHoverEnter = BeginTutorialPreview;
        tutorialRelay.onHoverExit = EndTutorialPreview;

        CreateButton(
            modePanel.transform,
            "イージー",
            new Vector2(0f, 55f),
            StartEasy
        );

        CreateButton(
            modePanel.transform,
            "ハード",
            new Vector2(0f, 0f),
            StartHard
        );

        CreateButton(
            modePanel.transform,
            "戻る",
            new Vector2(0f, -55f),
            BackToMain
        );

        mainPanel.transform.SetAsLastSibling();
        modePanel.transform.SetAsLastSibling();

        Debug.Log(
            "TitleMonitorMenuController: メニューボタン生成完了",
            this
        );
    }

    GameObject CreatePanel(string panelName)
    {
        Transform panelParent =
            roomRawImage != null && roomRawImage.transform.parent != null
            ? roomRawImage.transform.parent
            : tvCanvas.transform;

        GameObject panel =
            new GameObject(
                panelName,
                typeof(RectTransform),
                typeof(CanvasGroup)
            );

        panel.transform.SetParent(panelParent, false);

        RectTransform rect =
            panel.GetComponent<RectTransform>();

        if (roomRawImage != null)
        {
            RectTransform rawRect =
                roomRawImage.rectTransform;

            rect.anchorMin = rawRect.anchorMin;
            rect.anchorMax = rawRect.anchorMax;
            rect.pivot = rawRect.pivot;
            rect.anchoredPosition = rawRect.anchoredPosition;
            rect.sizeDelta = rawRect.sizeDelta;
            rect.localRotation = rawRect.localRotation;
            rect.localScale = rawRect.localScale;
        }
        else
        {
            StretchFull(rect);
        }

        panel.transform.SetAsLastSibling();
        return panel;
    }

    TitleMenuHoverRelay CreateButton(
        Transform parent,
        string label,
        Vector2 anchoredPosition,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject =
            new GameObject(
                label + "Button",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button),
                typeof(TitleMenuHoverRelay)
            );

        buttonObject.transform.SetParent(parent, false);

        RectTransform rect =
            buttonObject.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        RectTransform parentRect =
            parent as RectTransform;

        Vector2 resolvedSize = buttonSize;
        Vector2 resolvedPosition = anchoredPosition;

        if (parentRect != null)
        {
            float width =
                Mathf.Max(1f, parentRect.rect.width);
            float height =
                Mathf.Max(1f, parentRect.rect.height);

            resolvedSize = new Vector2(
                width * 0.58f,
                height * 0.19f
            );

            resolvedPosition = new Vector2(
                anchoredPosition.x / 190f * width,
                anchoredPosition.y / 180f * height
            );
        }

        rect.sizeDelta = resolvedSize;
        rect.anchoredPosition = resolvedPosition;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.05f, 0.05f, 0.05f, 0.82f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.72f, 0.95f, 1f, 1f);
        colors.pressedColor = new Color(0.45f, 0.75f, 0.85f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        GameObject textObject =
            new GameObject(
                "Label",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI)
            );

        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect =
            textObject.GetComponent<RectTransform>();

        StretchFull(textRect);

        TextMeshProUGUI text =
            textObject.GetComponent<TextMeshProUGUI>();

        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = buttonFontSize;
        text.color = Color.white;
        text.raycastTarget = false;
        text.enableAutoSizing = true;
        text.fontSizeMin = 10f;
        text.fontSizeMax = buttonFontSize;

        if (menuFont != null)
            text.font = menuFont;

        TitleMenuHoverRelay relay =
            buttonObject.GetComponent<TitleMenuHoverRelay>();

        relay.owner = this;
        return relay;
    }

    void CreateMonitorCursor()
    {
        GameObject cursorObject =
            new GameObject(
                "TitleMonitorCursor",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

        cursorObject.transform.SetParent(tvCanvas.transform, false);

        cursorRect =
            cursorObject.GetComponent<RectTransform>();

        cursorRect.anchorMin = Vector2.zero;
        cursorRect.anchorMax = Vector2.zero;
        cursorRect.pivot = new Vector2(0f, 1f);
        cursorRect.sizeDelta = cursorSize;

        cursorImage = cursorObject.GetComponent<Image>();
        cursorImage.sprite = normalCursorSprite;
        cursorImage.raycastTarget = false;
        cursorImage.preserveAspect = true;

        cursorObject.transform.SetAsLastSibling();
        cursorObject.SetActive(false);
    }

    void CreateFullScreenFade()
    {
        GameObject canvasObject =
            new GameObject(
                "TitleFadeCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler)
            );

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1920f, 1080f);

        GameObject imageObject =
            new GameObject(
                "FadeImage",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rect =
            imageObject.GetComponent<RectTransform>();

        StretchFull(rect);

        fullScreenFadeImage =
            imageObject.GetComponent<Image>();

        fullScreenFadeImage.color = Color.black;
        fullScreenFadeImage.raycastTarget = true;
    }

    void ConfigureInitialState()
    {
        phase = MenuPhase.Main;

        if (mainPanel != null)
            mainPanel.SetActive(true);

        if (modePanel != null)
            modePanel.SetActive(false);

        SetExclusiveRaw(null);

        if (fullScreenFadeImage != null)
        {
            Color color = fullScreenFadeImage.color;
            color.a = 1f;
            fullScreenFadeImage.color = color;
        }
    }

    public void OpenModeSelect()
    {
        if (IsStarting)
            return;

        PlaySE(selectSE);
        phase = MenuPhase.ModeSelect;

        mainPanel.SetActive(false);
        modePanel.SetActive(true);

        SetExclusiveRaw(roomRawImage);
    }

    public void BackToMain()
    {
        if (IsStarting)
            return;

        PlaySE(backSE);
        phase = MenuPhase.Main;

        modePanel.SetActive(false);
        mainPanel.SetActive(true);

        tutorialHoverCount = 0;
        SetExclusiveRaw(null);
    }

    public void BeginTutorialPreview()
    {
        if (phase != MenuPhase.Main || IsStarting)
            return;

        tutorialHoverCount++;

        if (tutorialRawTexture != null)
        {
            tutorialRawImage.texture = tutorialRawTexture;
            SetExclusiveRaw(tutorialRawImage);
        }
    }

    public void EndTutorialPreview()
    {
        tutorialHoverCount =
            Mathf.Max(0, tutorialHoverCount - 1);

        if (phase == MenuPhase.Main &&
            tutorialHoverCount == 0 &&
            !IsStarting)
        {
            SetExclusiveRaw(null);
        }
    }

    public void StartTutorial()
    {
        BeginStart(StartMode.Tutorial);
    }

    public void StartEasy()
    {
        BeginStart(StartMode.Easy);
    }

    public void StartHard()
    {
        BeginStart(StartMode.Hard);
    }

    void BeginStart(StartMode mode)
    {
        if (IsStarting)
            return;

        phase = MenuPhase.Starting;
        PlaySE(startSE);

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine =
            StartCoroutine(StartSequence(mode));
    }

    IEnumerator StartSequence(StartMode mode)
    {
        DisableAllButtons();

        GameObject prefab = GetSpawnPrefab(mode);

        if (prefab != null)
        {
            Vector3 position =
                spawnPoint != null
                ? spawnPoint.position
                : Vector3.zero;

            Quaternion rotation =
                spawnPoint != null
                ? spawnPoint.rotation
                : Quaternion.identity;

            Instantiate(prefab, position, rotation);
        }

        if (rawSwitchDelay > 0f)
            yield return Wait(rawSwitchDelay);

        if (startRawImage != null)
        {
            startRawImage.texture = startRawTexture;
            SetExclusiveRaw(startRawImage);
        }

        Coroutine cameraRoutine = null;

        if (mainCamera != null && cameraTarget != null)
        {
            cameraRoutine =
                StartCoroutine(MoveCameraRoutine());
        }

        yield return FlickerRoutine();

        if (cameraRoutine != null)
            yield return cameraRoutine;

        yield return FadeOutRoutine();

        if (sceneLoadDelay > 0f)
            yield return Wait(sceneLoadDelay);

        string sceneName = GetSceneName(mode);

        if (!string.IsNullOrWhiteSpace(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogWarning(
                $"{mode} の遷移先シーンが未設定です。",
                this
            );
    }

    IEnumerator FlickerRoutine()
    {
        if (monitorFlickerImage == null ||
            flickerDuration <= 0f)
            yield break;

        float timer = 0f;
        bool visible = false;

        while (timer < flickerDuration)
        {
            visible = !visible;

            monitorFlickerImage.color =
                visible
                ? new Color(1f, 1f, 1f, 0.78f)
                : new Color(1f, 1f, 1f, 0.08f);

            yield return Wait(flickerInterval);
            timer += flickerInterval;
        }

        monitorFlickerImage.color =
            new Color(1f, 1f, 1f, 0f);
    }

    IEnumerator MoveCameraRoutine()
    {
        Transform cameraTransform = mainCamera.transform;

        Vector3 startPosition = cameraTransform.position;
        Quaternion startRotation = cameraTransform.rotation;

        Vector3 endPosition = cameraTarget.position;
        Quaternion endRotation = cameraTarget.rotation;

        float timer = 0f;

        while (timer < cameraMoveDuration)
        {
            float delta =
                useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            timer += delta;

            float t =
                cameraMoveDuration > 0f
                ? Mathf.Clamp01(timer / cameraMoveDuration)
                : 1f;

            float eased =
                cameraMoveCurve != null
                ? cameraMoveCurve.Evaluate(t)
                : t;

            cameraTransform.SetPositionAndRotation(
                Vector3.LerpUnclamped(
                    startPosition,
                    endPosition,
                    eased
                ),
                Quaternion.SlerpUnclamped(
                    startRotation,
                    endRotation,
                    eased
                )
            );

            yield return null;
        }

        cameraTransform.SetPositionAndRotation(
            endPosition,
            endRotation
        );
    }

    IEnumerator FadeInRoutine()
    {
        if (fullScreenFadeImage == null)
            yield break;

        float timer = 0f;

        while (timer < openingFadeDuration)
        {
            float delta =
                useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            timer += delta;

            float t =
                openingFadeDuration > 0f
                ? Mathf.Clamp01(timer / openingFadeDuration)
                : 1f;

            Color color = fullScreenFadeImage.color;
            color.a = 1f - t;
            fullScreenFadeImage.color = color;

            yield return null;
        }

        Color finalColor = fullScreenFadeImage.color;
        finalColor.a = 0f;
        fullScreenFadeImage.color = finalColor;
        fullScreenFadeImage.raycastTarget = false;
    }

    IEnumerator FadeOutRoutine()
    {
        if (fullScreenFadeImage == null)
            yield break;

        fullScreenFadeImage.raycastTarget = true;

        Color color = fullScreenFadeImage.color;
        float startAlpha = color.a;
        float timer = 0f;

        while (timer < fadeOutDuration)
        {
            float delta =
                useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            timer += delta;

            float t =
                fadeOutDuration > 0f
                ? Mathf.Clamp01(timer / fadeOutDuration)
                : 1f;

            color.a = Mathf.Lerp(startAlpha, 1f, t);
            fullScreenFadeImage.color = color;

            yield return null;
        }

        color.a = 1f;
        fullScreenFadeImage.color = color;
    }

    void UpdateMonitorCursor()
    {
        if (tvRect == null || cursorRect == null)
            return;

        Camera eventCamera =
            tvCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : mainCamera;

        pointerInsideMonitor =
            RectTransformUtility.RectangleContainsScreenPoint(
                tvRect,
                Input.mousePosition,
                eventCamera
            );

        cursorRect.gameObject.SetActive(pointerInsideMonitor);

        if (!pointerInsideMonitor)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tvRect,
            Input.mousePosition,
            eventCamera,
            out Vector2 localPoint))
        {
            cursorRect.localPosition =
                localPoint + cursorOffset;
        }
    }

    public void SetCursorHover(bool hovering)
    {
        if (cursorImage == null)
            return;

        cursorImage.sprite =
            hovering && hoverCursorSprite != null
            ? hoverCursorSprite
            : normalCursorSprite;
    }

    void SetExclusiveRaw(RawImage target)
    {
        if (roomRawImage != null)
            roomRawImage.gameObject.SetActive(
                target == roomRawImage
            );

        if (tutorialRawImage != null)
            tutorialRawImage.gameObject.SetActive(
                target == tutorialRawImage
            );

        if (startRawImage != null)
            startRawImage.gameObject.SetActive(
                target == startRawImage
            );
    }

    void DisableAllButtons()
    {
        Button[] buttons =
            tvCanvas.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
            button.interactable = false;
    }

    void PlaySE(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, seVolume);
    }

    GameObject GetSpawnPrefab(StartMode mode)
    {
        switch (mode)
        {
            case StartMode.Tutorial:
                return tutorialSpawnPrefab;
            case StartMode.Easy:
                return easySpawnPrefab;
            case StartMode.Hard:
                return hardSpawnPrefab;
        }

        return null;
    }

    string GetSceneName(StartMode mode)
    {
        switch (mode)
        {
            case StartMode.Tutorial:
                return tutorialSceneName;
            case StartMode.Easy:
                return easySceneName;
            case StartMode.Hard:
                return hardSceneName;
        }

        return string.Empty;
    }

    YieldInstruction Wait(float seconds)
    {
        return useUnscaledTime
            ? new WaitForSecondsRealtime(seconds)
            : new WaitForSeconds(seconds);
    }

    static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
