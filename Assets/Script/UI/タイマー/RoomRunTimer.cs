using UnityEngine;

// RoomRunTimer
//
// 攻略フェーズ用タイマー。
// アイテムフェーズ終了後に ItemPhaseController から StartTimer() される想定。

public class RoomRunTimer : GamePhaseTimer
{
    public static RoomRunTimer RunInstance { get; private set; }

    [Header("部屋到達時の基本加算時間")]
    public float baseAddTimePerLevel = 8f;

    [Header("速さボーナス")]
    public bool useSpeedBonus = true;
    public float fastClearThreshold = 15f;
    public float fastClearBonus = 5f;

    [Header("クリア方法ボーナス")]
    public float normalPuzzleBonus = 0f;
    public float pressurePlateBonus = 1f;
    public float goalTriggerBonus = 0f;
    public float destroyedBonus = 2f;
    public float shortcutBonus = 3f;
    public float bypassedPenalty = -2f;

    [Header("最低加算時間")]
    public float minAddTime = 1f;

    private float lastLevelReachTime = 0f;
    private int lastAddedLevel = 1;

    protected override void Awake()
    {
        base.Awake();
        RunInstance = this;
        lastLevelReachTime = Time.time;
    }

    public override void AddTime(float amount)
    {
        base.AddTime(amount);
    }

    public void OnRoomLevelReached(RoomIdentity identity)
    {
        if (!GameModeManager.UsesTimers)
            return;

        if (identity == null)
            return;

        if (identity.roomLevel <= lastAddedLevel)
            return;

        float addTime =
            CalculateAddTime(identity);

        AddTime(addTime);

        lastAddedLevel =
            identity.roomLevel;

        lastLevelReachTime =
            Time.time;
    }

    float CalculateAddTime(RoomIdentity identity)
    {
        float add =
            baseAddTimePerLevel;

        float elapsed =
            Time.time - lastLevelReachTime;

        if (useSpeedBonus && elapsed <= fastClearThreshold)
            add += fastClearBonus;

        RoomClearMethod method =
            RoomClearMethod.Unknown;

        if (RunActionLogger.Instance != null)
            method = RunActionLogger.Instance.GetRoomClearMethod(identity.roomId);

        add += GetMethodBonus(method);

        return Mathf.Max(minAddTime, add);
    }

    float GetMethodBonus(RoomClearMethod method)
    {
        switch (method)
        {
            case RoomClearMethod.NormalPuzzle:
                return normalPuzzleBonus;

            case RoomClearMethod.PressurePlate:
                return pressurePlateBonus;

            case RoomClearMethod.GoalTrigger:
                return goalTriggerBonus;

            case RoomClearMethod.DoorDestroyed:
            case RoomClearMethod.WallDestroyed:
            case RoomClearMethod.ForceBreak:
                return destroyedBonus;

            case RoomClearMethod.ItemShortcut:
                return shortcutBonus;

            case RoomClearMethod.BypassedPuzzle:
                return bypassedPenalty;
        }

        return 0f;
    }
}
