using UnityEngine;

public class RoomPuzzleTarget : MonoBehaviour
{
    [Header("このパズルが操作する部屋")]
    public RoomPuzzleState targetRoom;

    [Header("直接制御するドア")]
    public DoorController[] directDoors;

    [Header("targetRoomが空ならRoomRuntimeManagerを使う")]
    public bool useRuntimeCurrentRoomIfTargetMissing = true;

    [Header("解法カテゴリ")]
    public PuzzleSolveMethod solveMethod = PuzzleSolveMethod.Normal;

    [Header("デバッグ")]
    public bool debugLog = true;

    public void SetDoorOpen(bool open)
    {
        RoomPuzzleState room = GetTargetRoom();

        if (room != null)
        {
            room.SetDoorOpenCondition(open, solveMethod);
            return;
        }

        if (HasDirectDoors())
        {
            SetDirectDoors(open);
            return;
        }

        if (debugLog)
            Debug.LogWarning(name + " 対象RoomPuzzleStateまたはDirectDoorsがありません");
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

        if (useRuntimeCurrentRoomIfTargetMissing &&
            RoomRuntimeManager.Instance != null)
            return RoomRuntimeManager.Instance.currentRoom;

        return null;
    }

    bool HasDirectDoors()
    {
        if (directDoors == null)
            return false;

        foreach (DoorController door in directDoors)
        {
            if (door != null)
                return true;
        }

        return false;
    }

    void SetDirectDoors(bool open)
    {
        foreach (DoorController door in directDoors)
        {
            if (door == null)
                continue;

            if (open)
                door.Open();
            else
                door.Close();
        }
    }
}
