// RoomPuzzleSpawnPoint：パズルを生成する位置
//
// 部屋Prefab内に置く。
// ここへランダムなパズルPrefabが生成される。

using UnityEngine;

public class RoomPuzzleSpawnPoint : MonoBehaviour
{
    [Header("ここにパズルを生成する")]
    public bool spawnPuzzle = true;

    [Header("生成済みパズル")]
    public GameObject spawnedPuzzle;
}
