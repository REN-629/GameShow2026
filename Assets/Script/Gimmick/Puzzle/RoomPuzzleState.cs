using UnityEngine;

public class RoomPuzzleState : MonoBehaviour
{
    [Header("この部屋の扉")]
    public DoorController[] roomDoors;

    [Header("ドア開放条件")]
    public bool doorOpenCondition = false;

    [Header("最後にドアを開けた方法")]
    public PuzzleSolveMethod lastSolveMethod = PuzzleSolveMethod.Unknown;

    [Header("デバッグ")]
    public bool debugLog = true;

    public void SetDoorOpenCondition(bool open)
    {
        SetDoorOpenCondition(open, PuzzleSolveMethod.Unknown);
    }

    public void SetDoorOpenCondition(bool open, PuzzleSolveMethod method)
    {
        doorOpenCondition = open;

        if (method != PuzzleSolveMethod.Unknown)
            lastSolveMethod = method;

        if (debugLog)
        {
            Debug.Log(
                name
                + " DoorCondition="
                + doorOpenCondition
                + " / method="
                + lastSolveMethod
            );
        }

        if (doorOpenCondition)
        {
            OpenDoors();
            RegisterSolveMethod(lastSolveMethod);
        }
        else
        {
            CloseDoors();
        }
    }

    public void ClearPuzzle()
    {
        SetDoorOpenCondition(true, PuzzleSolveMethod.Normal);
    }

    public void ResetPuzzle()
    {
        SetDoorOpenCondition(false, lastSolveMethod);
    }

    void RegisterSolveMethod(PuzzleSolveMethod method)
    {
        if (method == PuzzleSolveMethod.Unknown)
            return;

        RoomIdentity identity = GetComponent<RoomIdentity>();

        if (identity == null)
            return;

        if (RunActionLogger.Instance != null)
        {
            RunActionLogger.Instance.SetRoomClearMethod(
                identity.roomId,
                ConvertToRoomClearMethod(method)
            );
        }
    }

    RoomClearMethod ConvertToRoomClearMethod(PuzzleSolveMethod method)
    {
        switch (method)
        {
            case PuzzleSolveMethod.Normal:
                return RoomClearMethod.NormalPuzzle;

            case PuzzleSolveMethod.Weight:
            case PuzzleSolveMethod.Item:
                return RoomClearMethod.ItemShortcut;

            case PuzzleSolveMethod.Break:
            case PuzzleSolveMethod.Force:
                return RoomClearMethod.ForceBreak;

            case PuzzleSolveMethod.Shortcut:
                return RoomClearMethod.ItemShortcut;

            case PuzzleSolveMethod.Bypass:
                return RoomClearMethod.BypassedPuzzle;
        }

        return RoomClearMethod.Unknown;
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
