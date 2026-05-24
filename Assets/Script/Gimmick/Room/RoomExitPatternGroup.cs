// RoomExitPatternGroup：方向ごとの出口パターン管理
//
// 例:
// ExitGroup_North
// ├ Pattern_01
// ├ Pattern_02
// └ Pattern_03
//
// 役割:
// ・この出口が北/南/東/西どれか覚える
// ・複数パターンから1つだけランダム表示する
// ・選ばれなかったパターンは非表示にする

using UnityEngine;

public class RoomExitPatternGroup : MonoBehaviour
{
    [Header("方向")]
    public RoomDirection direction;

    [Header("この方向に出口を作る")]
    public bool enableExit = true;

    [Header("出口パターン")]
    public RoomExitPattern[] patterns;

    [Header("選ばれたパターン")]
    public RoomExitPattern selectedPattern;

    [Header("デバッグ")]
    public bool debugLog = true;

    void Reset()
    {
        patterns = GetComponentsInChildren<RoomExitPattern>(true);
    }

    public void SelectRandomPattern()
    {
        if (patterns == null || patterns.Length == 0)
            patterns = GetComponentsInChildren<RoomExitPattern>(true);

        if (!enableExit)
        {
            SetAllPatternsActive(false);
            selectedPattern = null;
            return;
        }

        if (patterns == null || patterns.Length == 0)
        {
            Debug.LogWarning(name + " に出口パターンがありません");
            return;
        }

        int index = Random.Range(0, patterns.Length);
        selectedPattern = patterns[index];

        for (int i = 0; i < patterns.Length; i++)
        {
            if (patterns[i] != null)
                patterns[i].gameObject.SetActive(patterns[i] == selectedPattern);
        }

        if (debugLog)
            Debug.Log(name + " selected " + selectedPattern.name + " / dir=" + direction);
    }

    void SetAllPatternsActive(bool active)
    {
        if (patterns == null || patterns.Length == 0)
            patterns = GetComponentsInChildren<RoomExitPattern>(true);

        if (patterns == null)
            return;

        foreach (RoomExitPattern pattern in patterns)
        {
            if (pattern != null)
                pattern.gameObject.SetActive(active);
        }
    }

    public RoomDoorSpawnPoint GetSelectedDoorSpawnPoint()
    {
        if (selectedPattern == null)
            return null;

        return selectedPattern.doorSpawnPoint;
    }

    public DoorController GetSpawnedDoor()
    {
        if (selectedPattern == null)
            return null;

        return selectedPattern.GetSpawnedDoor();
    }
}
