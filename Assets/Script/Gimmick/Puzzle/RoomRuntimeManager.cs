// RoomRuntimeManager：現在プレイヤーがいる部屋を記録する
//
// PressurePlateやGoalTriggerなどのパズルは、
// このCurrentRoomを使って「今いる部屋だけ」をクリアする。

using UnityEngine;

public class RoomRuntimeManager : MonoBehaviour
{
    public static RoomRuntimeManager Instance { get; private set; }

    [Header("現在プレイヤーがいる部屋")]
    public RoomPuzzleState currentRoom;

    [Header("デバッグ")]
    public bool debugLog = true;

    void Awake()
    {
        Instance = this;
    }

    public void SetCurrentRoom(RoomPuzzleState room)
    {
        currentRoom = room;

        if (debugLog && room != null)
            Debug.Log("現在の部屋: " + room.name);
    }

    public void ClearCurrentRoomPuzzle()
    {
        if (currentRoom == null)
        {
            Debug.LogWarning("CurrentRoomが無いのでパズルクリアできません");
            return;
        }

        currentRoom.ClearPuzzle();
    }
}
