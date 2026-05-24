//部屋1つ分の管理

using System.Collections.Generic;
using UnityEngine;

//一旦無しで試す
//[RequireComponent(typeof(Collider))]
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

    [Header("デバッグ")]
    public bool debugLog = true;

    private bool hasGenerated = false;

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

        RoomPuzzleState puzzleState = GetComponent<RoomPuzzleState>();

        if (puzzleState != null && RoomRuntimeManager.Instance != null)
            RoomRuntimeManager.Instance.SetCurrentRoom(puzzleState);

        if (generateOnlyOnce && hasGenerated)
            return;

        if (generator == null)
        {
            Debug.LogWarning(name + " の generator が未設定");
            return;
        }

        hasGenerated = true;

        if (debugLog)
            Debug.Log("プレイヤー侵入: " + gridPosition);

        generator.GenerateAround(this);
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
