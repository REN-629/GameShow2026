using UnityEngine;

public class TutorialManualClearCondition : MonoBehaviour
{
    public TutorialPuzzleDoorOpener doorOpener;

    void Awake()
    {
        if (doorOpener == null)
            doorOpener = GetComponent<TutorialPuzzleDoorOpener>();

        if (doorOpener == null)
            doorOpener = GetComponentInParent<TutorialPuzzleDoorOpener>();
    }

    public void Clear()
    {
        if (doorOpener != null)
            doorOpener.Clear();
    }
}
