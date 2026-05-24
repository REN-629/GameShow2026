// RoomPuzzleTarget：生成されたパズルが対象部屋を直接持つための補助
//
// 基本はRoomRuntimeManager.CurrentRoomを使えばOK。
// ただし将来的に「現在部屋に依存しないパズル」にしたい時、
// targetRoomを直接使えるようにする。

using UnityEngine;

public class RoomPuzzleTarget : MonoBehaviour
{
    [Header("このパズルがクリアする部屋")]
    public RoomPuzzleState targetRoom;

    public void ClearTargetRoom()
    {
        if (targetRoom != null)
        {
            targetRoom.ClearPuzzle();
            return;
        }

        if (RoomRuntimeManager.Instance != null)
            RoomRuntimeManager.Instance.ClearCurrentRoomPuzzle();
    }
}
