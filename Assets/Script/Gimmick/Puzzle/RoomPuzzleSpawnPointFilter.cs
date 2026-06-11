using UnityEngine;

public class RoomPuzzleSpawnPointFilter : MonoBehaviour
{
    [Header("このSpawnPointで生成可能なPuzzle")]
    public GameObject[] allowedPuzzlePrefabs;

    [Header("空なら部屋側/共通リストを使う")]
    public bool useParentListIfEmpty = true;

    public bool HasAllowedPuzzles()
    {
        return allowedPuzzlePrefabs != null && allowedPuzzlePrefabs.Length > 0;
    }

    public GameObject[] Filter(GameObject[] parentList)
    {
        if (HasAllowedPuzzles())
            return allowedPuzzlePrefabs;

        if (useParentListIfEmpty)
            return parentList;

        return null;
    }
}
