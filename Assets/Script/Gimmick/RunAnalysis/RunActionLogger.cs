using System.Collections.Generic;
using UnityEngine;

public class RunActionLogger : MonoBehaviour
{
    public static RunActionLogger Instance { get; private set; }

    [Header("移動")]
    public float movedDistance = 0f;
    public float dashTime = 0f;
    public int jumpCount = 0;

    [Header("行動")]
    public int itemUseCount = 0;
    public int throwCount = 0;
    public int carryObjectUseCount = 0;
    public int carryObjectThrowCount = 0;
    public int attackCount = 0;
    public int destroyedDoorCount = 0;
    public int destroyedWallCount = 0;

    [Header("到達")]
    public int reachedRoomEventCount = 0;

    [Header("クリア方法")]
    public int normalPuzzleClearCount = 0;
    public int ideaUniqueClearCount = 0;
    public int pressurePlateClearCount = 0;
    public int goalTriggerClearCount = 0;
    public int destroyedClearCount = 0;
    public int bypassedPuzzleCount = 0;
    public int shortcutClearCount = 0;

    [Header("時間")]
    public float runStartTime;
    public float totalRunTime;

    private readonly Dictionary<string, RoomActionRecord> roomRecords =
        new Dictionary<string, RoomActionRecord>();

    private readonly Dictionary<string, RoomClearMethod> roomClearMethods =
        new Dictionary<string, RoomClearMethod>();

    private readonly HashSet<int> reachedLevels =
        new HashSet<int>();

    private Vector3 lastPlayerPosition;
    private bool hasLastPosition = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        runStartTime = Time.time;
    }

    void Update()
    {
        totalRunTime = Time.time - runStartTime;
    }

    public void TrackPlayerPosition(Transform player)
    {
        if (player == null)
            return;

        if (!hasLastPosition)
        {
            lastPlayerPosition = player.position;
            hasLastPosition = true;
            return;
        }

        Vector3 now = player.position;
        now.y = 0f;

        Vector3 last = lastPlayerPosition;
        last.y = 0f;

        movedDistance += Vector3.Distance(last, now);
        lastPlayerPosition = player.position;
    }

    public void AddDashTime(float deltaTime)
    {
        dashTime += deltaTime;
    }

    public void LogJump()
    {
        jumpCount++;
    }

    public void LogAttack()
    {
        attackCount++;
    }

    public void LogItemUse()
    {
        itemUseCount++;

        RoomActionRecord record = GetCurrentRoomRecord();

        if (record != null)
            record.usedInventoryItem = true;
    }

    public void LogThrow()
    {
        throwCount++;

        RoomActionRecord record = GetCurrentRoomRecord();

        if (record != null)
        {
            record.usedInventoryItem = true;
            record.threwInventoryItem = true;
        }
    }

    public void LogCarryObjectUse()
    {
        carryObjectUseCount++;

        RoomActionRecord record = GetCurrentRoomRecord();

        if (record != null)
            record.usedCarryObject = true;
    }

    public void LogCarryObjectThrow()
    {
        carryObjectThrowCount++;

        RoomActionRecord record = GetCurrentRoomRecord();

        if (record != null)
        {
            record.usedCarryObject = true;
            record.threwCarryObject = true;
        }
    }

    public void LogDoorDestroyed()
    {
        destroyedDoorCount++;

        RoomActionRecord record = GetCurrentRoomRecord();

        if (record != null)
        {
            record.brokeExit = true;
            SetRoomClearMethod(record.roomId, RoomClearMethod.DoorDestroyed);
        }
    }

    public void LogWallDestroyed()
    {
        destroyedWallCount++;

        RoomActionRecord record = GetCurrentRoomRecord();

        if (record != null)
            record.brokeWall = true;
    }

    public void LogRoomReached(int level, string roomId)
    {
        if (reachedLevels.Contains(level))
            return;

        reachedLevels.Add(level);
        reachedRoomEventCount++;

        RoomActionRecord record = GetOrCreateRecord(roomId);
        record.reachedLevel = level;

        Debug.Log("部屋到達記録: Level " + level + " / " + roomId);
    }

    public void SetRoomClearMethod(string roomId, RoomClearMethod method)
    {
        if (string.IsNullOrEmpty(roomId))
            return;

        RoomActionRecord record = GetOrCreateRecord(roomId);
        record.rawClearMethod = method;
        record.cleared = true;

        RoomClearMethod resolved = ResolveClearMethod(record, method);

        if (roomClearMethods.TryGetValue(roomId, out RoomClearMethod oldMethod))
        {
            if (GetMethodPriority(oldMethod) > GetMethodPriority(resolved))
                return;

            roomClearMethods[roomId] = resolved;
        }
        else
        {
            roomClearMethods.Add(roomId, resolved);
        }

        RecalculateClearCounts();

        Debug.Log("部屋クリア方法記録: " + roomId + " / raw=" + method + " / result=" + resolved);
    }

    public RoomClearMethod GetRoomClearMethod(string roomId)
    {
        if (roomClearMethods.TryGetValue(roomId, out RoomClearMethod method))
            return method;

        return RoomClearMethod.Unknown;
    }

    public int GetClearedRoomCount()
    {
        return roomClearMethods.Count;
    }

    RoomActionRecord GetCurrentRoomRecord()
    {
        string roomId = GetCurrentRoomId();

        if (string.IsNullOrEmpty(roomId))
            return null;

        return GetOrCreateRecord(roomId);
    }

    string GetCurrentRoomId()
    {
        if (RoomRuntimeManager.Instance == null)
            return "";

        if (RoomRuntimeManager.Instance.currentRoom == null)
            return "";

        RoomIdentity identity =
            RoomRuntimeManager.Instance.currentRoom.GetComponent<RoomIdentity>();

        if (identity == null)
            return "";

        return identity.roomId;
    }

    RoomActionRecord GetOrCreateRecord(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
            return null;

        if (roomRecords.TryGetValue(roomId, out RoomActionRecord record))
            return record;

        record = new RoomActionRecord(roomId);
        roomRecords.Add(roomId, record);
        return record;
    }

    RoomClearMethod ResolveClearMethod(RoomActionRecord record, RoomClearMethod method)
    {
        if (record == null)
            return method;

        if (record.brokeExit || method == RoomClearMethod.DoorDestroyed)
            return RoomClearMethod.DoorDestroyed;

        if (method == RoomClearMethod.BypassedPuzzle)
            return RoomClearMethod.BypassedPuzzle;

        if (method == RoomClearMethod.GoalTrigger)
            return RoomClearMethod.GoalTrigger;

        if (record.usedInventoryItem && !record.brokeExit)
            return RoomClearMethod.IdeaUnique;

        if (record.usedCarryObject &&
            !record.usedInventoryItem &&
            !record.brokeExit)
            return RoomClearMethod.NormalPuzzle;

        if (method == RoomClearMethod.WallDestroyed ||
            method == RoomClearMethod.ForceBreak)
            return RoomClearMethod.IdeaUnique;

        if (method == RoomClearMethod.ItemShortcut)
            return RoomClearMethod.IdeaUnique;

        if (method == RoomClearMethod.PressurePlate)
        {
            if (record.usedCarryObject && !record.usedInventoryItem)
                return RoomClearMethod.NormalPuzzle;

            return RoomClearMethod.IdeaUnique;
        }

        if (method == RoomClearMethod.NormalPuzzle)
        {
            if (record.usedCarryObject && !record.usedInventoryItem)
                return RoomClearMethod.NormalPuzzle;

            return RoomClearMethod.IdeaUnique;
        }

        return method;
    }

    int GetMethodPriority(RoomClearMethod method)
    {
        switch (method)
        {
            case RoomClearMethod.DoorDestroyed:
                return 100;

            case RoomClearMethod.ForceBreak:
            case RoomClearMethod.WallDestroyed:
                return 90;

            case RoomClearMethod.IdeaUnique:
            case RoomClearMethod.ItemShortcut:
                return 80;

            case RoomClearMethod.NormalPuzzle:
                return 70;

            case RoomClearMethod.PressurePlate:
                return 60;

            case RoomClearMethod.GoalTrigger:
                return 50;

            case RoomClearMethod.BypassedPuzzle:
                return 40;
        }

        return 0;
    }

    void RecalculateClearCounts()
    {
        normalPuzzleClearCount = 0;
        ideaUniqueClearCount = 0;
        pressurePlateClearCount = 0;
        goalTriggerClearCount = 0;
        destroyedClearCount = 0;
        bypassedPuzzleCount = 0;
        shortcutClearCount = 0;

        foreach (RoomClearMethod method in roomClearMethods.Values)
        {
            switch (method)
            {
                case RoomClearMethod.NormalPuzzle:
                    normalPuzzleClearCount++;
                    break;

                case RoomClearMethod.IdeaUnique:
                    ideaUniqueClearCount++;
                    break;

                case RoomClearMethod.PressurePlate:
                    pressurePlateClearCount++;
                    break;

                case RoomClearMethod.GoalTrigger:
                    goalTriggerClearCount++;
                    break;

                case RoomClearMethod.DoorDestroyed:
                case RoomClearMethod.WallDestroyed:
                case RoomClearMethod.ForceBreak:
                    destroyedClearCount++;
                    break;

                case RoomClearMethod.ItemShortcut:
                    shortcutClearCount++;
                    break;

                case RoomClearMethod.BypassedPuzzle:
                    bypassedPuzzleCount++;
                    break;
            }
        }
    }

    public void ResetLog()
    {
        movedDistance = 0f;
        dashTime = 0f;
        jumpCount = 0;
        itemUseCount = 0;
        throwCount = 0;
        carryObjectUseCount = 0;
        carryObjectThrowCount = 0;
        attackCount = 0;
        destroyedDoorCount = 0;
        destroyedWallCount = 0;
        reachedRoomEventCount = 0;

        normalPuzzleClearCount = 0;
        ideaUniqueClearCount = 0;
        pressurePlateClearCount = 0;
        goalTriggerClearCount = 0;
        destroyedClearCount = 0;
        bypassedPuzzleCount = 0;
        shortcutClearCount = 0;

        roomRecords.Clear();
        roomClearMethods.Clear();
        reachedLevels.Clear();

        runStartTime = Time.time;
        totalRunTime = 0f;

        hasLastPosition = false;
    }

    class RoomActionRecord
    {
        public string roomId;
        public int reachedLevel;
        public bool cleared;
        public bool usedCarryObject;
        public bool usedInventoryItem;
        public bool threwCarryObject;
        public bool threwInventoryItem;
        public bool brokeExit;
        public bool brokeWall;
        public RoomClearMethod rawClearMethod = RoomClearMethod.Unknown;

        public RoomActionRecord(string roomId)
        {
            this.roomId = roomId;
        }
    }
}
