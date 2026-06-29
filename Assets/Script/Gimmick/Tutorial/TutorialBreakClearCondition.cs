using UnityEngine;

public class TutorialBreakClearCondition : MonoBehaviour
{
    public TutorialPuzzleDoorOpener doorOpener;
    public AttributeDurability durability;

    private bool cleared;

    void Awake()
    {
        if (doorOpener == null)
            doorOpener = GetComponent<TutorialPuzzleDoorOpener>();

        if (doorOpener == null)
            doorOpener = GetComponentInParent<TutorialPuzzleDoorOpener>();

        if (durability == null)
            durability = GetComponent<AttributeDurability>();

        if (durability == null)
            durability = GetComponentInChildren<AttributeDurability>();
    }

    void Update()
    {
        if (cleared)
            return;

        if (durability == null)
            return;

        if (!durability.IsBroken())
            return;

        cleared = true;

        if (doorOpener != null)
            doorOpener.Clear();
    }

    public void ClearByBreak()
    {
        if (cleared)
            return;

        cleared = true;

        if (doorOpener != null)
            doorOpener.Clear();
    }
}
