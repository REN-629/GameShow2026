using UnityEngine;

public class TutorialModeSceneSetup : MonoBehaviour
{
    [Header("自動でTutorialモードにする")]
    public bool forceTutorialMode = true;

    [Header("停止対象")]
    public MonoBehaviour[] disableOnStart;

    void Awake()
    {
        if (!forceTutorialMode)
            return;

        GameModeManager manager = GameModeManager.Instance;

        if (manager == null)
        {
            GameObject obj = new GameObject("GameModeManager");
            manager = obj.AddComponent<GameModeManager>();
        }

        manager.currentMode = GameMode.Tutorial;
    }

    void Start()
    {
        if (disableOnStart == null)
            return;

        foreach (MonoBehaviour behaviour in disableOnStart)
        {
            if (behaviour != null)
                behaviour.enabled = false;
        }
    }
}
