using UnityEngine;

// RoomIdentity
//
// 部屋のIDとレベルを持つ。
// この版ではスコアは「部屋ID」ではなく「roomLevel」で加算する。
//
// 例:
// Room1にいる時に次の部屋が2つ生成された場合
// どちらも roomLevel = 2
//
// そのため、どちらに入ってもスコア+1は最初の1回だけ。

public class RoomIdentity : MonoBehaviour
{
    [Header("部屋ID")]
    public string roomId;

    [Header("部屋レベル")]
    public int roomLevel = 1;

    [Header("親レベル")]
    public int parentRoomLevel = 0;

    [Header("グリッド座標")]
    public Vector2Int gridPosition;

    public void SetupId(Vector2Int grid, int level, int parentLevel)
    {
        gridPosition = grid;
        roomLevel = level;
        parentRoomLevel = parentLevel;

        roomId = "Level_" + roomLevel + "_Grid_" + grid.x + "_" + grid.y;
    }
}
