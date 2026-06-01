using UnityEngine;

public class RoomPuzzleTarget : MonoBehaviour
{
    [Header("このパズルが操作する部屋")]
    public RoomPuzzleState targetRoom;

    [Header("このパズルのクリア方法")]
    public RoomClearMethod clearMethod = RoomClearMethod.NormalPuzzle;

    public void SetCleared(bool cleared)
    {
        RoomPuzzleState room = GetTargetRoom();

        if (room == null)
        {
            Debug.LogWarning(name + " 対象RoomPuzzleStateがありません");
            return;
        }

        if (cleared)
            room.SetPuzzleState(true, clearMethod);
        else
            room.SetPuzzleState(false, clearMethod);
    }

    public void ClearTargetRoom()
    {
        SetCleared(true);
    }

    public void ResetTargetRoom()
    {
        SetCleared(false);
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
