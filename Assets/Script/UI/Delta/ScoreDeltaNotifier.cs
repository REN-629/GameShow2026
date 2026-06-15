using UnityEngine;

public class ScoreDeltaNotifier : MonoBehaviour
{
    public static ScoreDeltaNotifier Instance { get; private set; }

    public DeltaPopupUI deltaPopup;

    void Awake()
    {
        Instance = this;
    }

    public void AddScoreDelta(int amount)
    {
        if (deltaPopup != null)
            deltaPopup.AddDelta(amount);
    }
}
