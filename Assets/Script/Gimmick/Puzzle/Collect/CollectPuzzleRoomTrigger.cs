using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollectPuzzleRoomTrigger : MonoBehaviour
{
    public CollectPuzzleManager puzzle;
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

        if (puzzle == null)
            puzzle = GetComponentInParent<CollectPuzzleManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (puzzle != null)
            puzzle.BindUI();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (puzzle != null)
            puzzle.UnbindUI();
    }
}
