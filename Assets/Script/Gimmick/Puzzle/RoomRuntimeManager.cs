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

    public void SetCurrentRoomDoorOpen(bool open, PuzzleSolveMethod method)
    {
        if (currentRoom == null)
        {
            Debug.LogWarning("CurrentRoomが無いのでドア状態を変更できません");
            return;
        }

        currentRoom.SetDoorOpenCondition(open, method);
    }

    public void ClearCurrentRoomPuzzle()
    {
        SetCurrentRoomDoorOpen(true, PuzzleSolveMethod.Normal);
    }
}
