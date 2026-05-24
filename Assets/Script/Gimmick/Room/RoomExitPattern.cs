// RoomExitPattern：出口パターン1つ分
//
// 例:
// Pattern_01
// ├ WallVisual
// └ DoorSpawnPoint
//
// 出口の高さ・位置・ドア枠形状などを、このパターンごとに変えられる。

using UnityEngine;

public class RoomExitPattern : MonoBehaviour
{
    [Header("このパターン内の扉スポーンポイント")]
    public RoomDoorSpawnPoint doorSpawnPoint;

    void Reset()
    {
        doorSpawnPoint = GetComponentInChildren<RoomDoorSpawnPoint>(true);
    }

    public DoorController GetSpawnedDoor()
    {
        if (doorSpawnPoint == null)
            return null;

        return doorSpawnPoint.spawnedDoor;
    }
}
