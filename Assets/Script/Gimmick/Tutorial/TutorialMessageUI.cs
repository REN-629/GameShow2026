using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialMessageUI : MonoBehaviour
{
    public static TutorialMessageUI Instance { get; private set; }

    [Header("UI")]
    public GameObject messageRoot;
    public TextMeshProUGUI messageTMP;
    public Text messageText;

    [Header("表示時間")]
    public bool autoHide = false;
    public float showDuration = 5f;

    private Coroutine hideRoutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Hide();
    }

    public void Show(string message)
    {
        if (messageRoot != null)
            messageRoot.SetActive(true);

        if (messageTMP != null)
            messageTMP.text = message;

        if (messageText != null)
            messageText.text = message;

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        if (autoHide)
            hideRoutine = StartCoroutine(HideAfterDelay());
    }

    public void Hide()
    {
        if (messageRoot != null)
            messageRoot.SetActive(false);
    }

    System.Collections.IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(showDuration);
        Hide();
        hideRoutine = null;
    }
}
