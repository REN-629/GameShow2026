using UnityEngine;

public class RoomPuzzleSet : MonoBehaviour
{
    [Header("この部屋で生成可能なPuzzle")]
    public GameObject[] allowedPuzzlePrefabs;

    [Header("空なら共通Puzzleリストを使う")]
    public bool useGlobalListIfEmpty = true;

    public bool HasAllowedPuzzles()
    {
        return allowedPuzzlePrefabs != null && allowedPuzzlePrefabs.Length > 0;
    }

    public GameObject[] GetPuzzlePrefabs(GameObject[] globalPuzzlePrefabs)
    {
        if (HasAllowedPuzzles())
            return allowedPuzzlePrefabs;

        if (useGlobalListIfEmpty)
            return globalPuzzlePrefabs;

        return null;
    }
}
