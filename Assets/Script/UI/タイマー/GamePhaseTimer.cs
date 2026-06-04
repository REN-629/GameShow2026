using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// GamePhaseTimer
//
// 汎用タイマー。
// ItemPhaseTimer / RoomRunTimer の親。
// ItemPhaseTimerではTimeOutを上書きしてシーン遷移なしにする。

public class GamePhaseTimer : MonoBehaviour
{
    public static GamePhaseTimer Instance { get; private set; }

    [Header("時間")]
    public float startTime = 60f;
    public float currentTime = 60f;
    public bool timerRunning = true;

    [Header("タイムアウト後")]
    public float waitAfterTimeOut = 3f;
    public string nextSceneName;

    [Header("崩壊/演出対象")]
    public GameObject collapseTarget;
    public string collapseMessage = "BreakNow";
    public bool sendCollapseMessageOnTimeout = false;

    [Header("UI / TMP")]
    public TextMeshProUGUI timerTMP;

    [Header("UI / Legacy Text")]
    public Text timerText;

    [Header("差分UI")]
    public DeltaPopupUI deltaPopup;

    [Header("表示設定")]
    public string prefix = "TIME : ";
    public bool showDecimal = false;

    [Header("デバッグ")]
    public bool debugLog = true;

    protected bool timedOut = false;

    protected virtual void Awake()
    {
        Instance = this;
    }

    protected virtual void Start()
    {
        currentTime = startTime;
        UpdateUI();
    }

    protected virtual void Update()
    {
        if (!timerRunning || timedOut)
            return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            UpdateUI();
            TimeOut();
            return;
        }

        UpdateUI();
    }

    public virtual void AddTime(float amount)
    {
        if (timedOut)
            return;

        currentTime += amount;

        if (currentTime < 0f)
            currentTime = 0f;

        if (deltaPopup != null && Mathf.Abs(amount) > 0.01f)
            deltaPopup.AddDelta(amount);

        UpdateUI();

        if (currentTime <= 0f)
            TimeOut();
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public void StartTimer()
    {
        if (timedOut)
            timedOut = false;

        timerRunning = true;
        UpdateUI();
    }

    public void ResetTimer()
    {
        timedOut = false;
        currentTime = startTime;
        UpdateUI();
    }

    protected virtual void TimeOut()
    {
        if (timedOut)
            return;

        timedOut = true;
        timerRunning = false;

        if (debugLog)
            Debug.Log(name + " タイムアウト");

        if (sendCollapseMessageOnTimeout && collapseTarget != null)
            collapseTarget.SendMessage(collapseMessage, SendMessageOptions.DontRequireReceiver);

        StartCoroutine(LoadSceneAfterWait());
    }

    System.Collections.IEnumerator LoadSceneAfterWait()
    {
        yield return new WaitForSeconds(waitAfterTimeOut);

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    void UpdateUI()
    {
        string value =
            showDecimal
            ? prefix + currentTime.ToString("F1")
            : prefix + Mathf.CeilToInt(currentTime).ToString();

        if (timerTMP != null)
            timerTMP.text = value;

        if (timerText != null)
            timerText.text = value;
    }
}
