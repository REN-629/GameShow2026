using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialChecklistRoomTrigger : MonoBehaviour
{
    public TutorialChecklistClearCondition checklist;
    public string playerTag = "Player";

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

        if (checklist == null)
            checklist = GetComponentInParent<TutorialChecklistClearCondition>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (checklist != null)
            checklist.ActivateChecklist();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (checklist != null)
            checklist.DeactivateChecklist();
    }
}
