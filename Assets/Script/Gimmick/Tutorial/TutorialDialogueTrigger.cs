using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialDialogueTrigger : MonoBehaviour
{
    [TextArea(2, 6)]
    public string[] messages;

    [Header("対象")]
    public string playerTag = "Player";

    [Header("一度だけ")]
    public bool triggerOnce = true;

    [Header("このTriggerだけ自動送り設定を上書き")]
    public bool overrideAutoAdvance = false;
    public bool autoAdvance = true;
    public float autoAdvanceDelay = 5f;
    public float hideDelayAfterLastMessage = 2f;

    private bool triggered;

    void Reset()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
            col.isTrigger = true;
    }

    void Awake()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
            col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && triggered)
            return;

        if (!other.CompareTag(playerTag))
            return;

        triggered = true;

        if (TutorialDialogueUI.Instance == null)
            return;

        if (overrideAutoAdvance)
        {
            TutorialDialogueUI.Instance.autoAdvance = autoAdvance;
            TutorialDialogueUI.Instance.autoAdvanceDelay = autoAdvanceDelay;
            TutorialDialogueUI.Instance.hideDelayAfterLastMessage = hideDelayAfterLastMessage;
        }

        TutorialDialogueUI.Instance.Show(messages);
    }
}
