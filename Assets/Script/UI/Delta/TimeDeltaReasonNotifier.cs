using UnityEngine;

public class TimeDeltaReasonNotifier : MonoBehaviour
{
    public static TimeDeltaReasonNotifier Instance { get; private set; }

    public ComboDeltaPopupUI timePopup;

    void Awake()
    {
        Instance = this;
    }

    public void AddTimeDelta(string reason, float amount)
    {
        if (timePopup != null)
            timePopup.SetReasonDelta(reason, amount);
    }
}
