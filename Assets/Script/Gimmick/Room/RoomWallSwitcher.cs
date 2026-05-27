
using UnityEngine;

public class RoomWallSwitcher : MonoBehaviour
{
    [Header("通常壁")]
    public GameObject wallNorth;
    public GameObject wallSouth;
    public GameObject wallEast;
    public GameObject wallWest;

    [Header("デバッグ")]
    public bool debugLog = false;

    public void Apply(RoomCell room)
    {
        if (room == null)
            return;

        ApplyDirection(room, RoomDirection.North, wallNorth);
        ApplyDirection(room, RoomDirection.South, wallSouth);
        ApplyDirection(room, RoomDirection.East, wallEast);
        ApplyDirection(room, RoomDirection.West, wallWest);
    }

    void ApplyDirection(RoomCell room, RoomDirection direction, GameObject wall)
    {
        if (wall == null)
            return;

        RoomExitPatternGroup group = room.GetExitGroup(direction);

        bool hasActiveExit =
            group != null &&
            group.enableExit &&
            group.selectedPattern != null;

        // 出口があるなら普通壁OFF、出口がないなら普通壁ON
        wall.SetActive(!hasActiveExit);

        if (debugLog)
        {
            Debug.Log(
                name + " / " + direction +
                " / wall=" + wall.name +
                " / active=" + wall.activeSelf
            );
        }
    }
}
