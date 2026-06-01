using System.Collections.Generic;
using UnityEngine;

// RunActionLogger
//
// 行動ログと、部屋ごとのクリア方法を保存する。
// スコア加算は次部屋Triggerに触れた時。
// クリア方法は各部屋で別途記録する。

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
    public int attackCount = 0;
    public int destroyedDoorCount = 0;
    public int destroyedWallCount = 0;

    [Header("到達")]
    public int reachedRoomEventCount = 0;

    [Header("クリア方法")]
    public int normalPuzzleClearCount = 0;
    public int pressurePlateClearCount = 0;
    public int goalTriggerClearCount = 0;
    public int destroyedClearCount = 0;
    public int bypassedPuzzleCount = 0;
    public int shortcutClearCount = 0;

    [Header("時間")]
    public float runStartTime;
    public float totalRunTime;

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

    public void LogJump() { jumpCount++; }
    public void LogItemUse() { itemUseCount++; }
    public void LogThrow() { throwCount++; }
    public void LogAttack() { attackCount++; }
    public void LogDoorDestroyed() { destroyedDoorCount++; }
    public void LogWallDestroyed() { destroyedWallCount++; }

    public void LogRoomReached(int level, string roomId)
    {
        if (reachedLevels.Contains(level))
            return;

        reachedLevels.Add(level);
        reachedRoomEventCount++;

        // 前の部屋のクリア方法が未記録なら、パズル無視扱いにできる余地を残す。
        Debug.Log("部屋到達記録: Level " + level + " / " + roomId);
    }

    public void SetRoomClearMethod(string roomId, RoomClearMethod method)
    {
        if (string.IsNullOrEmpty(roomId))
            return;

        if (roomClearMethods.ContainsKey(roomId))
            return;

        roomClearMethods.Add(roomId, method);

        switch (method)
        {
            case RoomClearMethod.NormalPuzzle:
                normalPuzzleClearCount++;
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

        Debug.Log("部屋クリア方法記録: " + roomId + " / " + method);
    }

    public RoomClearMethod GetRoomClearMethod(string roomId)
    {
        if (roomClearMethods.TryGetValue(roomId, out RoomClearMethod method))
            return method;

        return RoomClearMethod.Unknown;
    }

    public void ResetLog()
    {
        movedDistance = 0f;
        dashTime = 0f;
        jumpCount = 0;
        itemUseCount = 0;
        throwCount = 0;
        attackCount = 0;
        destroyedDoorCount = 0;
        destroyedWallCount = 0;
        reachedRoomEventCount = 0;

        normalPuzzleClearCount = 0;
        pressurePlateClearCount = 0;
        goalTriggerClearCount = 0;
        destroyedClearCount = 0;
        bypassedPuzzleCount = 0;
        shortcutClearCount = 0;

        roomClearMethods.Clear();
        reachedLevels.Clear();

        runStartTime = Time.time;
        totalRunTime = 0f;

        hasLastPosition = false;
    }
}
