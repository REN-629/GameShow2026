using UnityEngine;

public class TestPuzzleModeController : MonoBehaviour
{
    [Header("自動設定")]
    public bool forceTestModeOnAwake = true;

    [Header("テスト用Room")]
    public RoomPuzzleState testRoom;

    void Awake()
    {
        if (!forceTestModeOnAwake)
            return;

        GameModeManager manager = GameModeManager.Instance;

        if (manager == null)
        {
            GameObject obj = new GameObject("GameModeManager");
            manager = obj.AddComponent<GameModeManager>();
        }

        manager.currentMode = GameMode.Test;
    }

    void Start()
    {
        if (testRoom != null && RoomRuntimeManager.Instance != null)
            RoomRuntimeManager.Instance.SetCurrentRoom(testRoom);
    }
}
