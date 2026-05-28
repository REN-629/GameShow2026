// RoomCell.cs
//
// 部屋1つ分の管理。
//
// 修正版:
// ・プレイヤーが一度侵入した部屋では、再侵入しても部屋生成しない
// ・ただし CurrentRoom の更新は毎回行う
//
// つまり:
// 初回侵入
//   → CurrentRoom更新
//   → 周囲生成
//
// 再侵入
//   → CurrentRoom更新のみ
//   → GenerateAround() は呼ばない
//
// これで、過去に入った部屋のTriggerに触れても再生成されない。

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
    [Tooltip("ONなら、この部屋で周囲生成するのは初回侵入時だけ")]
    public bool generateOnlyOnce = true;

    [Header("状態")]
    [Tooltip("プレイヤーがこの部屋に一度でも入ったか")]
    public bool hasPlayerEntered = false;

    [Tooltip("この部屋を中心に周囲生成を行ったか")]
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

        // CurrentRoomは再侵入時も更新する
        UpdateCurrentRoom();

        // 既に入ったことがある部屋なら、再生成しない
        if (hasPlayerEntered && generateOnlyOnce)
        {
            if (debugLog)
            {
                Debug.Log(
                    name
                    + " は侵入済みなので再生成なし: "
                    + gridPosition
                );
            }

            return;
        }

        hasPlayerEntered = true;

        if (generateOnlyOnce && hasGeneratedAround)
        {
            if (debugLog)
            {
                Debug.Log(
                    name
                    + " は既に周囲生成済み: "
                    + gridPosition
                );
            }

            return;
        }

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

    void UpdateCurrentRoom()
    {
        RoomPuzzleState puzzleState = GetComponent<RoomPuzzleState>();

        if (puzzleState != null && RoomRuntimeManager.Instance != null)
        {
            RoomRuntimeManager.Instance.SetCurrentRoom(puzzleState);
        }
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
