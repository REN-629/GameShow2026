using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialDialogueUI : MonoBehaviour
{
    public static TutorialDialogueUI Instance { get; private set; }

    [Header("UI")]
    public GameObject root;
    public Text messageText;

    [Header("手動送り")]
    public KeyCode nextKey = KeyCode.Return;
    public bool clickToNext = true;

    [Header("自動送り")]
    public bool autoAdvance = true;
    public float autoAdvanceDelay = 5f;

    [Header("タイピング")]
    public float typeInterval = 0.035f;
    public bool useUnscaledTime = false;

    [Header("最後の文章後")]
    public bool hideAfterLastMessage = true;
    public float hideDelayAfterLastMessage = 2f;

    private string[] currentMessages;
    private int currentIndex;
    private Coroutine typingRoutine;
    private Coroutine autoAdvanceRoutine;
    private bool isTyping;
    private string fullText;
    private bool showing;

    void Awake()
    {
        Instance = this;
        Hide();
    }

    void Update()
    {
        if (!showing)
            return;

        if (root == null || !root.activeSelf)
            return;

        if (Input.GetKeyDown(nextKey) || (clickToNext && Input.GetMouseButtonDown(0)))
            Next();
    }

    public void Show(params string[] messages)
    {
        if (messages == null || messages.Length == 0)
            return;

        currentMessages = messages;
        currentIndex = 0;
        showing = true;

        if (root != null)
            root.SetActive(true);

        StartTyping(currentMessages[currentIndex]);
    }

    public void Next()
    {
        if (!showing)
            return;

        if (isTyping)
        {
            FinishTyping();
            RestartAutoAdvance();
            return;
        }

        currentIndex++;

        if (currentMessages == null || currentIndex >= currentMessages.Length)
        {
            if (hideAfterLastMessage)
                Hide();
            return;
        }

        StartTyping(currentMessages[currentIndex]);
    }

    void StartTyping(string text)
    {
        StopTypingRoutine();
        StopAutoAdvanceRoutine();

        fullText = text;
        typingRoutine = StartCoroutine(TypeRoutine(text));
    }

    IEnumerator TypeRoutine(string text)
    {
        isTyping = true;

        if (messageText != null)
            messageText.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            if (messageText != null)
                messageText.text += text[i];

            if (useUnscaledTime)
                yield return new WaitForSecondsRealtime(typeInterval);
            else
                yield return new WaitForSeconds(typeInterval);
        }

        isTyping = false;
        typingRoutine = null;

        RestartAutoAdvance();
    }

    void FinishTyping()
    {
        StopTypingRoutine();

        if (messageText != null)
            messageText.text = fullText;

        isTyping = false;
    }

    void RestartAutoAdvance()
    {
        StopAutoAdvanceRoutine();

        if (!autoAdvance)
            return;

        autoAdvanceRoutine = StartCoroutine(AutoAdvanceRoutine());
    }

    IEnumerator AutoAdvanceRoutine()
    {
        float waitTime = autoAdvanceDelay;

        bool isLast =
            currentMessages != null &&
            currentIndex >= currentMessages.Length - 1;

        if (isLast && hideAfterLastMessage)
            waitTime = hideDelayAfterLastMessage;

        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(waitTime);
        else
            yield return new WaitForSeconds(waitTime);

        if (!showing)
            yield break;

        if (isLast && hideAfterLastMessage)
            Hide();
        else
            Next();
    }

    void StopTypingRoutine()
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = null;
    }

    void StopAutoAdvanceRoutine()
    {
        if (autoAdvanceRoutine != null)
            StopCoroutine(autoAdvanceRoutine);

        autoAdvanceRoutine = null;
    }

    public void Hide()
    {
        StopTypingRoutine();
        StopAutoAdvanceRoutine();

        showing = false;
        isTyping = false;

        if (messageText != null)
            messageText.text = "";

        if (root != null)
            root.SetActive(false);
    }
}
