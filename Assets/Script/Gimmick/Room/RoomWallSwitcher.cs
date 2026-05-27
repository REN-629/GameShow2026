// RoomWallSwitcher.cs
//
// 通常壁と出口パターンを切り替えるスクリプト。
//
// 修正版:
// ・Wall_NorthなどをInspectorで入れ忘れても名前から自動検索する
// ・出口がある方向は通常壁をOFF
// ・出口がない方向は通常壁をON
//
// 部屋PrefabのRootに付ける。
//
// 推奨構成:
// Room_A
// ├ RoomWallSwitcher
// ├ Wall_North
// ├ Wall_South
// ├ Wall_East
// ├ Wall_West
// ├ ExitGroup_North
// ├ ExitGroup_South
// ├ ExitGroup_East
// └ ExitGroup_West

using UnityEngine;

public class RoomWallSwitcher : MonoBehaviour
{
    [Header("通常壁")]
    public GameObject wallNorth;
    public GameObject wallSouth;
    public GameObject wallEast;
    public GameObject wallWest;

    [Header("自動検索")]
    public bool autoFindWallsByName = true;

    [Header("デバッグ")]
    public bool debugLog = true;

    void Awake()
    {
        if (autoFindWallsByName)
            AutoFindWalls();
    }

    void Start()
    {
        if (autoFindWallsByName)
            AutoFindWalls();
    }

    public void Apply(RoomCell room)
    {
        if (room == null)
            return;

        if (autoFindWallsByName)
            AutoFindWalls();

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
            group.selectedPattern != null &&
            group.selectedPattern.gameObject.activeSelf;

        if (wall != null)
        {
            // 出口がある → 普通壁OFF
            // 出口がない → 普通壁ON
            wall.SetActive(!hasActiveExit);
        }

        if (debugLog)
        {
            Debug.Log(
                name
                + " / "
                + direction
                + " / exit="
                + hasActiveExit
                + " / wall="
                + (wall != null ? wall.name : "未設定")
                + " / wallActive="
                + (wall != null ? wall.activeSelf.ToString() : "none")
            );
        }
    }

    void AutoFindWalls()
    {
        if (wallNorth == null)
            wallNorth = FindChildByName("Wall_North");

        if (wallSouth == null)
            wallSouth = FindChildByName("Wall_South");

        if (wallEast == null)
            wallEast = FindChildByName("Wall_East");

        if (wallWest == null)
            wallWest = FindChildByName("Wall_West");
    }

    GameObject FindChildByName(string targetName)
    {
        Transform[] children =
            GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == targetName)
                return child.gameObject;
        }

        return null;
    }
}
