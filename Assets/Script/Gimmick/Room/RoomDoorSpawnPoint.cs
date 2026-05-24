// RoomDoorSpawnPoint：扉を生成する位置
//
// 出口パターンの中に置く。
// 木/石/金属などの扉Prefabは、この位置にランダム生成される。

using UnityEngine;

public class RoomDoorSpawnPoint : MonoBehaviour
{
    [Header("このスポーンポイントで扉を生成する")]
    public bool spawnDoor = true;

    [Header("生成済み扉")]
    public DoorController spawnedDoor;
}
