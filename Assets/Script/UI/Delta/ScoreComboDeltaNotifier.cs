using UnityEngine;

public class ScoreComboDeltaNotifier : MonoBehaviour
{
    public static ScoreComboDeltaNotifier Instance { get; private set; }

    public ComboDeltaPopupUI scorePopup;

    void Awake()
    {
        Instance = this;
    }

    public void AddScoreDelta(int amount)
    {
        if (scorePopup != null)
            scorePopup.AddDelta(amount);
    }
}
