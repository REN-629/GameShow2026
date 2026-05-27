// RoomWallSwitcher.cs
//
// 通常壁と出口パターンを切り替えるスクリプト。
//
// 出口が選ばれた方向:
//   Wall_NorthなどをOFF
//   ExitGroup_Northの選択済みPatternをON
//
// 通常壁が選ばれた方向:
//   Wall_NorthなどをON
//   ExitGroup_NorthのPatternは全部OFF
//
// 部屋PrefabのRootに付ける。

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
        RoomExitPatternGroup group = room.GetExitGroup(direction);

        bool hasActiveExit =
            group != null &&
            group.enableExit &&
            group.selectedPattern != null;

        if (wall != null)
            wall.SetActive(!hasActiveExit);

        if (debugLog)
        {
            Debug.Log(
                name
                + " / "
                + direction
                + " / exit="
                + hasActiveExit
                + " / wall="
                + (wall != null ? wall.name : "none")
            );
        }
    }
}
