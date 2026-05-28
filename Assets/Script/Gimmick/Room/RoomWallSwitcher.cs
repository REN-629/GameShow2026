using UnityEngine;

// RoomWallSwitcher
//
// 通常壁と出口パターンを切り替える。
// Southは入口専用なので、Wall_Southも基本OFFにできる。
// 入口穴をPrefab側で用意する場合、Wall_Southは登録しなくてOK。
public class RoomWallSwitcher : MonoBehaviour
{
    [Header("通常壁")]
    public GameObject wallNorth;
    public GameObject wallSouth;
    public GameObject wallEast;
    public GameObject wallWest;

    [Header("South入口壁")]
    [Tooltip("ONならSouth側の通常壁を常にOFFにする")]
    public bool alwaysHideSouthWall = true;

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
        ApplyDirection(room, RoomDirection.East, wallEast);
        ApplyDirection(room, RoomDirection.West, wallWest);

        if (wallSouth != null)
        {
            wallSouth.SetActive(!alwaysHideSouthWall);
        }
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
                + (wall != null ? wall.name : "未設定")
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
