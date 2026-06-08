using UnityEngine;

public class RoomPuzzleTarget : MonoBehaviour
{
    [Header("このパズルが操作する部屋")]
    public RoomPuzzleState targetRoom;

    [Header("解法カテゴリ")]
    public PuzzleSolveMethod solveMethod = PuzzleSolveMethod.Normal;

    public void SetDoorOpen(bool open)
    {
        RoomPuzzleState room = GetTargetRoom();

        if (room == null)
        {
            Debug.LogWarning(name + " 対象RoomPuzzleStateがありません");
            return;
        }

        room.SetDoorOpenCondition(open, solveMethod);
    }

    public void ClearTargetRoom()
    {
        SetDoorOpen(true);
    }

    public void ResetTargetRoom()
    {
        SetDoorOpen(false);
    }

    public void SetCleared(bool cleared)
    {
        SetDoorOpen(cleared);
    }

    RoomPuzzleState GetTargetRoom()
    {
        if (targetRoom != null)
            return targetRoom;

        if (RoomRuntimeManager.Instance != null)
            return RoomRuntimeManager.Instance.currentRoom;

        return null;
    }
}
