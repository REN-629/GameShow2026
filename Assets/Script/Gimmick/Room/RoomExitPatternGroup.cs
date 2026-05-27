// RoomExitPatternGroup.cs
//
// 方向ごとの出口パターン管理。
//
// 修正版:
// ・北など、必ず出口にしたい方向を forceExit で固定できる
// ・それ以外の方向は、通常壁 or 出口をランダムにできる
// ・通常壁が選ばれた場合は出口パターンを全部OFFにする
//
// 例:
// ExitGroup_North
//   forceExit = true
//   → 必ず出口
//
// ExitGroup_South / East / West
//   randomizeWallOrExit = true
//   exitChance = 0.5
//   → 50%で出口、50%で普通壁

using UnityEngine;

public class RoomExitPatternGroup : MonoBehaviour
{
    [Header("方向")]
    public RoomDirection direction;

    [Header("出口固定")]
    [Tooltip("ONなら、この方向は必ず出口になる。北出口固定などに使う")]
    public bool forceExit = false;

    [Header("壁/出口ランダム")]
    [Tooltip("ONなら、通常壁にするか出口にするかをランダムで決める")]
    public bool randomizeWallOrExit = true;

    [Range(0f, 1f)]
    [Tooltip("出口になる確率。0なら必ず壁、1なら必ず出口")]
    public float exitChance = 0.5f;

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

        DecideWallOrExit();

        if (!enableExit)
        {
            SetAllPatternsActive(false);
            selectedPattern = null;

            if (debugLog)
                Debug.Log(name + " は通常壁 / dir=" + direction);

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
        {
            Debug.Log(
                name
                + " selected "
                + selectedPattern.name
                + " / dir="
                + direction
            );
        }
    }

    void DecideWallOrExit()
    {
        if (forceExit)
        {
            enableExit = true;
            return;
        }

        if (!randomizeWallOrExit)
        {
            // Inspectorで設定したenableExitをそのまま使う
            return;
        }

        enableExit = Random.value <= exitChance;
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
        if (!enableExit)
            return null;

        if (selectedPattern == null)
            return null;

        return selectedPattern.doorSpawnPoint;
    }

    public DoorController GetSpawnedDoor()
    {
        if (!enableExit)
            return null;

        if (selectedPattern == null)
            return null;

        return selectedPattern.GetSpawnedDoor();
    }
}
