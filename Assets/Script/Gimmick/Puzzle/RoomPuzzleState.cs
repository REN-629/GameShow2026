using UnityEngine;

public class RoomPuzzleState : MonoBehaviour
{
    [Header("この部屋の扉")]
    public DoorController[] roomDoors;

    [Header("状態")]
    public bool puzzleCleared = false;

    [Header("スコア")]
    public bool scoreCounted = false;

    [Header("クリア方法")]
    public RoomClearMethod clearMethod = RoomClearMethod.Unknown;

    [Header("デバッグ")]
    public bool debugLog = true;

    void Start()
    {
        RoomIdentity identity = GetComponent<RoomIdentity>();

        if (identity != null && RunScoreManager.Instance != null)
        {
            scoreCounted = RunScoreManager.Instance.IsRoomCleared(identity.roomId);
        }
    }

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
            clearMethod = method;

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
            CountScoreOnce();
            OpenDoors();
        }
        else
        {
            CloseDoors();
        }
    }

    void CountScoreOnce()
    {
        if (scoreCounted)
            return;

        scoreCounted = true;

        if (RunScoreManager.Instance != null)
        {
            RunScoreManager.Instance.RegisterRoomCleared(this, clearMethod);
        }
        else
        {
            Debug.LogWarning("RunScoreManager がありません");
        }
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
