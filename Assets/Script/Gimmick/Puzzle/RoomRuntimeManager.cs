//¡‚¢‚é•”‰®‚ÌŠÇ—
using UnityEngine;

public class RoomRuntimeManager : MonoBehaviour
{
    public static RoomRuntimeManager Instance { get; private set; }

    [Header("Œ»İƒvƒŒƒCƒ„[‚ª‚¢‚é•”‰®")]
    public RoomPuzzleState currentRoom;

    void Awake()
    {
        Instance = this;
    }

    public void SetCurrentRoom(RoomPuzzleState room)
    {
        currentRoom = room;
    }

    public void ClearCurrentRoomPuzzle()
    {
        if (currentRoom == null)
            return;

        currentRoom.ClearPuzzle();
    }
}