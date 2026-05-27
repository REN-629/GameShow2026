// RoomPuzzleState.cs
//
// 部屋のパズル状態と、その部屋の扉を管理するスクリプト
//
// 修正版:
// ・ClearPuzzle() でクリア状態ON、扉を開く
// ・ResetPuzzle() でクリア状態OFF、扉を閉じる
// ・SetPuzzleState(bool) でON/OFFをまとめて制御できる
//
// 重量ボタンのような
// 「重りが乗っている間だけ開く」仕組みに対応。

using UnityEngine;

public class RoomPuzzleState : MonoBehaviour
{
    [Header("この部屋の扉")]
    public DoorController[] roomDoors;

    [Header("状態")]
    public bool puzzleCleared = false;

    [Header("デバッグ")]
    public bool debugLog = true;

    public void ClearPuzzle()
    {
        SetPuzzleState(true);
    }

    public void ResetPuzzle()
    {
        SetPuzzleState(false);
    }

    public void SetPuzzleState(bool cleared)
    {
        if (puzzleCleared == cleared)
            return;

        puzzleCleared = cleared;

        if (debugLog)
        {
            Debug.Log(
                name
                + " のパズル状態: "
                + (puzzleCleared ? "クリア" : "未クリア")
            );
        }

        if (puzzleCleared)
            OpenDoors();
        else
            CloseDoors();
    }

    public void OpenDoors()
    {
        if (roomDoors == null)
            return;

        foreach (DoorController door in roomDoors)
        {
            if (door != null)
                door.Open();
        }
    }

    public void CloseDoors()
    {
        if (roomDoors == null)
            return;

        foreach (DoorController door in roomDoors)
        {
            if (door != null)
                door.Close();
        }
    }
}
