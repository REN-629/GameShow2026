using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RoomCell : MonoBehaviour
{
    [Header("この部屋のグリッド座標")]
    public Vector2Int gridPosition;

    [HideInInspector]
    public InfiniteRoomGenerator generator;

    [Header("出口パターングループ")]
    public RoomExitPatternGroup[] exitGroups;

    [Header("設定")]
    public bool generateOnlyOnce = true;

    [Header("状態")]
    public bool hasPlayerEntered = false;
    public bool hasGeneratedAround = false;

    [Header("デバッグ")]
    public bool debugLog = true;

    void Reset()
    {
        exitGroups = GetComponentsInChildren<RoomExitPatternGroup>(true);
    }

    void Start()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
            col.isTrigger = true;

        if (exitGroups == null || exitGroups.Length == 0)
            exitGroups = GetComponentsInChildren<RoomExitPatternGroup>(true);

        if (generator != null)
            generator.RegisterRoom(this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        UpdateCurrentRoom();
        RegisterRoomProgress();

        if (hasPlayerEntered && generateOnlyOnce)
        {
            if (debugLog)
                Debug.Log(name + " は侵入済みなので再生成なし: " + gridPosition);

            return;
        }

        hasPlayerEntered = true;

        if (generateOnlyOnce && hasGeneratedAround)
            return;

        if (!GameModeManager.UsesRoomGeneration)
            return;

        if (generator == null)
        {
            Debug.LogWarning(name + " の generator が未設定");
            return;
        }

        hasGeneratedAround = true;

        if (debugLog)
            Debug.Log("プレイヤー初侵入: " + gridPosition);

        generator.GenerateAround(this);
    }

    void RegisterRoomProgress()
    {
        if (!GameModeManager.RecordsRoomProgress)
            return;

        RoomIdentity identity = GetComponent<RoomIdentity>();

        if (identity == null)
        {
            if (debugLog)
                Debug.LogWarning(name + " に RoomIdentity がありません");

            return;
        }

        bool scoreAdded = false;

        if (RunScoreManager.Instance != null)
            scoreAdded = RunScoreManager.Instance.RegisterRoomReached(identity);

        if (!scoreAdded)
        {
            if (ScoreComboDeltaNotifier.Instance != null)
            {
                ScoreComboDeltaNotifier.Instance.AddScoreDelta(1);
            }

            return;
        }

        if (RoomRunTimer.RunInstance != null && GameModeManager.UsesTimers)
            RoomRunTimer.RunInstance.OnRoomLevelReached(identity);

        if (ScoreDeltaNotifier.Instance != null)
            ScoreDeltaNotifier.Instance.AddScoreDelta(1);
    }

    void UpdateCurrentRoom()
    {
        RoomPuzzleState puzzleState = GetComponent<RoomPuzzleState>();

        if (puzzleState != null && RoomRuntimeManager.Instance != null)
            RoomRuntimeManager.Instance.SetCurrentRoom(puzzleState);
    }

    public RoomExitPatternGroup GetExitGroup(RoomDirection direction)
    {
        if (exitGroups == null)
            return null;

        foreach (RoomExitPatternGroup group in exitGroups)
        {
            if (group != null && group.direction == direction)
                return group;
        }

        return null;
    }

    public DoorController[] GetSpawnedDoors()
    {
        List<DoorController> doors = new List<DoorController>();

        if (exitGroups == null)
            return doors.ToArray();

        foreach (RoomExitPatternGroup group in exitGroups)
        {
            if (group == null)
                continue;

            DoorController door = group.GetSpawnedDoor();

            if (door != null)
                doors.Add(door);
        }

        return doors.ToArray();
    }
}
