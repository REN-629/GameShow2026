using UnityEngine;

public class RoomRunTimer : GamePhaseTimer
{
    public static RoomRunTimer RunInstance { get; private set; }

    [Header("部屋到達時の基本加算時間")]
    public float baseAddTimePerLevel = 8f;
    public string baseAddReason = "部屋到達";

    [Header("速さボーナス")]
    public bool useSpeedBonus = true;
    public float fastClearThreshold = 15f;
    public float fastClearBonus = 5f;
    public string fastClearReason = "早い突破";

    [Header("クリア方法ボーナス")]
    public float normalPuzzleBonus = 0f;
    public string normalPuzzleReason = "正規突破";

    public float pressurePlateBonus = 1f;
    public string pressurePlateReason = "重量突破";

    public float goalTriggerBonus = 0f;
    public string goalTriggerReason = "ゴール到達";

    public float destroyedBonus = 2f;
    public string destroyedReason = "強引な突破";

    public float shortcutBonus = 3f;
    public string shortcutReason = "ショートカット";

    public float bypassedPenalty = -2f;
    public string bypassedReason = "無視突破";

    [Header("最低加算時間")]
    public float minAddTime = 1f;

    [Header("表示")]
    public bool showEachReasonSeparately = true;
    public string combinedReason = "時間加算";

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

    public override void AddTime(float amount, string reason)
    {
        base.AddTime(amount, reason);
    }

    public override void RemoveTime(float amount, string reason)
    {
        base.RemoveTime(amount, reason);
    }

    public void OnRoomLevelReached(RoomIdentity identity)
    {
        if (!GameModeManager.UsesTimers)
            return;

        if (identity == null)
            return;

        if (identity.roomLevel <= lastAddedLevel)
            return;

        TimeBonusResult result = CalculateAddTime(identity);

        if (showEachReasonSeparately)
        {
            AddTimeWithReason(baseAddTimePerLevel, baseAddReason);

            if (result.speedBonusApplied)
                AddTimeWithReason(fastClearBonus, fastClearReason);

            if (Mathf.Abs(result.methodBonus) > 0.01f)
                AddTimeWithReason(result.methodBonus, result.methodReason);
        }
        else
        {
            AddTime(result.total, combinedReason);
        }

        lastAddedLevel = identity.roomLevel;
        lastLevelReachTime = Time.time;
    }

    void AddTimeWithReason(float amount, string reason)
    {
        if (Mathf.Abs(amount) <= 0.01f)
            return;

        AddTime(amount, reason);
    }

    TimeBonusResult CalculateAddTime(RoomIdentity identity)
    {
        TimeBonusResult result = new TimeBonusResult();
        result.total = baseAddTimePerLevel;

        float elapsed = Time.time - lastLevelReachTime;

        if (useSpeedBonus && elapsed <= fastClearThreshold)
        {
            result.speedBonusApplied = true;
            result.total += fastClearBonus;
        }

        RoomClearMethod method = RoomClearMethod.Unknown;

        if (RunActionLogger.Instance != null)
            method = RunActionLogger.Instance.GetRoomClearMethod(identity.roomId);

        result.methodBonus = GetMethodBonus(method);
        result.methodReason = GetMethodReason(method);
        result.total += result.methodBonus;

        if (result.total < minAddTime)
            result.total = minAddTime;

        return result;
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

    string GetMethodReason(RoomClearMethod method)
    {
        switch (method)
        {
            case RoomClearMethod.NormalPuzzle:
                return normalPuzzleReason;

            case RoomClearMethod.PressurePlate:
                return pressurePlateReason;

            case RoomClearMethod.GoalTrigger:
                return goalTriggerReason;

            case RoomClearMethod.DoorDestroyed:
            case RoomClearMethod.WallDestroyed:
            case RoomClearMethod.ForceBreak:
                return destroyedReason;

            case RoomClearMethod.ItemShortcut:
                return shortcutReason;

            case RoomClearMethod.BypassedPuzzle:
                return bypassedReason;
        }

        return combinedReason;
    }

    struct TimeBonusResult
    {
        public float total;
        public bool speedBonusApplied;
        public float methodBonus;
        public string methodReason;
    }
}
