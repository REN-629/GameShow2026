using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialDialogueUI : MonoBehaviour
{
    public static TutorialDialogueUI Instance { get; private set; }

    [Header("UI")]
    public GameObject root;
    public Text messageText;

    [Header("入力")]
    public KeyCode nextKey = KeyCode.Return;
    public bool clickToNext = true;

    [Header("タイピング")]
    public float typeInterval = 0.035f;
    public bool useUnscaledTime = false;

    private string[] currentMessages;
    private int currentIndex;
    private Coroutine typingRoutine;
    private bool isTyping;
    private string fullText;

    void Awake()
    {
        Instance = this;
        Hide();
    }

    void Update()
    {
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

        if (root != null)
            root.SetActive(true);

        StartTyping(currentMessages[currentIndex]);
    }

    public void Next()
    {
        if (isTyping)
        {
            FinishTyping();
            return;
        }

        currentIndex++;

        if (currentMessages == null || currentIndex >= currentMessages.Length)
        {
            Hide();
            return;
        }

        StartTyping(currentMessages[currentIndex]);
    }

    void StartTyping(string text)
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

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
    }

    void FinishTyping()
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        if (messageText != null)
            messageText.text = fullText;

        isTyping = false;
        typingRoutine = null;
    }

    public void Hide()
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = null;
        isTyping = false;

        if (messageText != null)
            messageText.text = "";

        if (root != null)
            root.SetActive(false);
    }
}
