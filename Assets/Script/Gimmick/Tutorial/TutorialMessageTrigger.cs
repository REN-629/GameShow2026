using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialMessageTrigger : MonoBehaviour
{
    [TextArea(2, 6)]
    public string message;

    public bool showOnlyInTutorial = true;
    public bool triggerOnce = true;
    public string playerTag = "Player";

    private bool triggered = false;

    void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && triggered)
            return;

        if (!other.CompareTag(playerTag))
            return;

        if (showOnlyInTutorial && !GameModeManager.IsTutorial)
            return;

        triggered = true;

        if (TutorialMessageUI.Instance != null)
            TutorialMessageUI.Instance.Show(message);
    }
}
