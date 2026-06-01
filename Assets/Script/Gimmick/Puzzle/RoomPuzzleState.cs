using UnityEngine;

// RoomPuzzleState
//
// この版ではスコア加算はしない。
// スコアは次の部屋Triggerに触れた時にRunScoreManagerが加算する。
//
// ここでは「この部屋をどう攻略状態にしたか」だけを記録する。

public class RoomPuzzleState : MonoBehaviour
{
    [Header("この部屋の扉")]
    public DoorController[] roomDoors;

    [Header("状態")]
    public bool puzzleCleared = false;

    [Header("クリア方法")]
    public RoomClearMethod clearMethod = RoomClearMethod.Unknown;

    [Header("デバッグ")]
    public bool debugLog = true;

    public void ClearPuzzle()
    {
        SetPuzzleState(true, RoomClearMethod.NormalPuzzle);
    }

    public void ClearPuzzle(RoomClearMethod method)
    {
        SetPuzzleState(true, method);
    }

    public void ResetPuzzle()
    {
        SetPuzzleState(false, clearMethod);
    }

    public void SetPuzzleState(bool cleared)
    {
        SetPuzzleState(cleared, RoomClearMethod.Unknown);
    }

    public void SetPuzzleState(bool cleared, RoomClearMethod method)
    {
        if (puzzleCleared == cleared)
            return;

        puzzleCleared = cleared;

        if (cleared && method != RoomClearMethod.Unknown)
        {
            clearMethod = method;
            RegisterClearMethodOnce(method);
        }

        if (debugLog)
        {
            Debug.Log(
                name
                + " のパズル状態: "
                + (puzzleCleared ? "クリア" : "未クリア")
                + " / method="
                + clearMethod
            );
        }

        if (puzzleCleared)
        {
            OpenDoors();
        }
        else
        {
            CloseDoors();
        }
    }

    void RegisterClearMethodOnce(RoomClearMethod method)
    {
        RoomIdentity identity = GetComponent<RoomIdentity>();

        if (identity == null)
            return;

        if (RunActionLogger.Instance != null)
            RunActionLogger.Instance.SetRoomClearMethod(identity.roomId, method);
    }

    public void RegisterBypassedIfNotCleared()
    {
        if (puzzleCleared)
            return;

        RoomIdentity identity = GetComponent<RoomIdentity>();

        if (identity == null)
            return;

        if (RunActionLogger.Instance != null)
            RunActionLogger.Instance.SetRoomClearMethod(identity.roomId, RoomClearMethod.BypassedPuzzle);
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
