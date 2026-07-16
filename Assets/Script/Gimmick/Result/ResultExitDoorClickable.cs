using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class ResultExitDoorClickable : MonoBehaviour
{
    public Transform moveTarget;
    public Transform targetPoint;

    [Min(0f)]
    public float moveDuration = 1.2f;

    public AnimationCurve moveCurve =
        new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 3f),
            new Keyframe(1f, 1f, 0f, 0f)
        );

    public AudioClip clickSE;

    [Range(0f, 1f)]
    public float clickSEVolume = 1f;

    public Image fadeImage;
    public float fadeDuration = 1f;
    public float waitBeforeFade = 0.1f;
    public string nextSceneName = "Title";
    public bool useUnscaledTime = true;

    private bool activated;
    private AudioSource oneShotAudioSource;

    void Awake()
    {
        if (moveTarget == null)
            moveTarget = transform;

        Collider col = GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = false;
        }

        oneShotAudioSource = GetComponent<AudioSource>();

        if (oneShotAudioSource == null)
            oneShotAudioSource = gameObject.AddComponent<AudioSource>();

        oneShotAudioSource.playOnAwake = false;
        oneShotAudioSource.loop = false;
        oneShotAudioSource.spatialBlend = 0f;

        EnsureFadeImage();
    }

    public void Activate()
    {
        if (activated || targetPoint == null)
            return;

        if (ResultCursorController.Instance != null &&
            !ResultCursorController.Instance.CanUseDoor(this))
        {
            return;
        }

        activated = true;

        if (ResultCursorController.Instance != null)
            ResultCursorController.Instance.RegisterActiveDoor(this);

        if (clickSE != null)
            oneShotAudioSource.PlayOneShot(clickSE, clickSEVolume);

        StartCoroutine(ExitRoutine());
    }

    IEnumerator ExitRoutine()
    {
        yield return MoveDoorRoutine();

        if (waitBeforeFade > 0f)
        {
            if (useUnscaledTime)
                yield return new WaitForSecondsRealtime(waitBeforeFade);
            else
                yield return new WaitForSeconds(waitBeforeFade);
        }

        yield return FadeOutRoutine();

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator MoveDoorRoutine()
    {
        Vector3 startPosition = moveTarget.position;
        Quaternion startRotation = moveTarget.rotation;
        Vector3 targetPosition = targetPoint.position;
        Quaternion targetRotation = targetPoint.rotation;

        float timer = 0f;

        while (timer < moveDuration)
        {
            float delta =
                useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            timer += delta;

            float t =
                moveDuration > 0f
                ? Mathf.Clamp01(timer / moveDuration)
                : 1f;

            float eased =
                moveCurve != null
                ? moveCurve.Evaluate(t)
                : 1f - Mathf.Pow(1f - t, 3f);

            moveTarget.SetPositionAndRotation(
                Vector3.LerpUnclamped(
                    startPosition,
                    targetPosition,
                    eased
                ),
                Quaternion.SlerpUnclamped(
                    startRotation,
                    targetRotation,
                    eased
                )
            );

            yield return null;
        }

        moveTarget.SetPositionAndRotation(
            targetPosition,
            targetRotation
        );
    }

    IEnumerator FadeOutRoutine()
    {
        if (fadeImage == null)
            yield break;

        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
        float startAlpha = color.a;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            float delta =
                useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            timer += delta;

            float t =
                fadeDuration > 0f
                ? Mathf.Clamp01(timer / fadeDuration)
                : 1f;

            color.a = Mathf.Lerp(startAlpha, 1f, t);
            fadeImage.color = color;

            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }

    void EnsureFadeImage()
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
            return;
        }

        GameObject canvasObject = new GameObject(
            "ResultFadeCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler)
        );

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 2000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject imageObject = new GameObject(
            "FadeImage",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        fadeImage = imageObject.GetComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.raycastTarget = true;
    }
}
