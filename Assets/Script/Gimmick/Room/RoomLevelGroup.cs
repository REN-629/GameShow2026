using UnityEngine;

// RoomLevelGroup
//
// 同じ部屋レベルのクローン群をまとめる親。
// 例:
//
// Level_2
// ├ Room(Clone)
// └ Room(Clone)
//
// 古い部屋削除時はこのLevel単位で消せる。

public class RoomLevelGroup : MonoBehaviour
{
    public int roomLevel;

    public void Setup(int level)
    {
        roomLevel = level;
        gameObject.name = "Level_" + roomLevel;
    }
}
