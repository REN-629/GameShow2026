// RoomPuzzleState：その部屋のパズル状態と扉を管理する
//
// 生成された扉はInfiniteRoomGeneratorからroomDoorsに自動登録される。
// パズルがクリアされると、この部屋の扉だけ開く。

using UnityEngine;

public class RoomPuzzleState : MonoBehaviour
{
    [Header("この部屋の扉")]
    public DoorController[] roomDoors;

    [Header("状態")]
    public bool puzzleCleared = false;
    public bool doorsOpened = false;

    [Header("デバッグ")]
    public bool debugLog = true;

    public void ClearPuzzle()
    {
        if (puzzleCleared)
        {
            if (debugLog)
                Debug.Log(name + " のパズルは既にクリア済み");
            return;
        }

        puzzleCleared = true;

        if (debugLog)
            Debug.Log(name + " のパズルクリア");

        OpenDoors();
    }

    public void OpenDoors()
    {
        if (doorsOpened)
            return;

        doorsOpened = true;

        if (roomDoors == null)
            return;

        foreach (DoorController door in roomDoors)
        {
            if (door != null)
                door.Open();
        }
    }
}
