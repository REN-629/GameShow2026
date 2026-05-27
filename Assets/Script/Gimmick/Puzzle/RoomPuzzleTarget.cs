// RoomPuzzleTarget.cs
//
// パズルPrefabが「どの部屋をクリア/解除するか」を持つための補助スクリプト
//
// InfiniteRoomGenerator / RoomPuzzleSpawner によって
// 生成時に targetRoom が自動設定される想定。
//
// targetRoomが無い場合は RoomRuntimeManager.CurrentRoom を使う。

using UnityEngine;

public class RoomPuzzleTarget : MonoBehaviour
{
    [Header("このパズルが操作する部屋")]
    public RoomPuzzleState targetRoom;

    public void SetCleared(bool cleared)
    {
        RoomPuzzleState room = GetTargetRoom();

        if (room == null)
        {
            Debug.LogWarning(name + " 対象RoomPuzzleStateがありません");
            return;
        }

        room.SetPuzzleState(cleared);
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
