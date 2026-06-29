using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialDialogueTrigger : MonoBehaviour
{
    [TextArea(2, 6)]
    public string[] messages;

    public string playerTag = "Player";
    public bool triggerOnce = true;

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

        if (TutorialDialogueUI.Instance != null)
            TutorialDialogueUI.Instance.Show(messages);
    }
}
