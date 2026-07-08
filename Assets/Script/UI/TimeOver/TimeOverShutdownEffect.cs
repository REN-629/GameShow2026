using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TimeOverShutdownEffect : MonoBehaviour
{
    public static TimeOverShutdownEffect Instance { get; private set; }

    [Header("暗転UI")]
    public Image blackoutImage;

    [Header("演出時間")]
    public float shutdownDuration = 2.5f;
    public float waitAfterBlackout = 1.0f;

    [Header("電源落ち風")]
    public bool useFlicker = true;
    public float flickerInterval = 0.06f;
    public float flickerStrength = 0.25f;

    [Header("最後に一気に暗くする")]
    public bool useFinalCut = true;
    public float finalCutTime = 0.15f;

    [Header("操作停止")]
    public MonoBehaviour[] disableOnShutdown;

    [Header("Cursor")]
    public bool unlockCursor = false;

    [Header("シーン遷移")]
    public bool loadSceneAfterEffect = true;
    public string nextSceneName;

    [Header("時間")]
    public bool useUnscaledTime = true;

    [Header("デバッグ")]
    public bool debugLog = true;

    private bool playing;

    void Awake()
    {
        Instance = this;

        if (blackoutImage != null)
        {
            Color c = blackoutImage.color;
            c.a = 0f;
            blackoutImage.color = c;
            blackoutImage.gameObject.SetActive(true);
        }
    }

    public void Play()
    {
        if (playing)
            return;

        StartCoroutine(ShutdownRoutine());
    }

    public void PlayAndLoad(string sceneName)
    {
        nextSceneName = sceneName;
        loadSceneAfterEffect = true;
        Play();
    }

    IEnumerator ShutdownRoutine()
    {
        playing = true;

        DisableControls();

        if (unlockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (debugLog)
            Debug.Log("TimeOver Shutdown Effect Start");

        float timer = 0f;
        float flickerTimer = 0f;
        float randomFlicker = 0f;

        while (timer < shutdownDuration)
        {
            float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            timer += delta;

            float t = Mathf.Clamp01(timer / shutdownDuration);
            float baseAlpha = Mathf.SmoothStep(0f, 0.92f, t);

            if (useFlicker)
            {
                flickerTimer += delta;

                if (flickerTimer >= flickerInterval)
                {
                    flickerTimer = 0f;
                    randomFlicker = Random.Range(-flickerStrength, flickerStrength);
                }

                baseAlpha = Mathf.Clamp01(baseAlpha + randomFlicker * (1f - t));
            }

            SetBlackoutAlpha(baseAlpha);

            yield return null;
        }

        if (useFinalCut)
        {
            float cutTimer = 0f;

            while (cutTimer < finalCutTime)
            {
                float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                cutTimer += delta;

                float t = Mathf.Clamp01(cutTimer / finalCutTime);
                SetBlackoutAlpha(Mathf.Lerp(0.92f, 1f, t));

                yield return null;
            }
        }

        SetBlackoutAlpha(1f);

        if (waitAfterBlackout > 0f)
        {
            if (useUnscaledTime)
                yield return new WaitForSecondsRealtime(waitAfterBlackout);
            else
                yield return new WaitForSeconds(waitAfterBlackout);
        }

        if (loadSceneAfterEffect && !string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    void DisableControls()
    {
        if (disableOnShutdown == null)
            return;

        foreach (MonoBehaviour behaviour in disableOnShutdown)
        {
            if (behaviour != null)
                behaviour.enabled = false;
        }
    }

    void SetBlackoutAlpha(float alpha)
    {
        if (blackoutImage == null)
            return;

        Color c = blackoutImage.color;
        c.a = Mathf.Clamp01(alpha);
        blackoutImage.color = c;
    }
}
