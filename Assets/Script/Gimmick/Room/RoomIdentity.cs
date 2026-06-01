using UnityEngine;

public class RoomIdentity : MonoBehaviour
{
    [Header("部屋ID")]
    public string roomId;

    [Header("グリッド座標からIDを作る")]
    public bool useGridPositionAsId = true;

    public void SetupId(Vector2Int gridPosition)
    {
        if (useGridPositionAsId)
        {
            roomId = "Room_" + gridPosition.x + "_" + gridPosition.y;
        }

        if (string.IsNullOrEmpty(roomId))
        {
            roomId = System.Guid.NewGuid().ToString();
        }
    }
}
