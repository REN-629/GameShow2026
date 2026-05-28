using UnityEngine;

public class RoomExitPatternGroup : MonoBehaviour
{
    [Header("方向")]
    public RoomDirection direction;

    [Header("この方向に出口を作る")]
    public bool enableExit = false;

    [Header("出口になる確率")]
    [Range(0f, 1f)]
    public float exitChance = 0.5f;

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

    public void SetExitActive(bool active)
    {
        enableExit = active;

        if (patterns == null || patterns.Length == 0)
            patterns = GetComponentsInChildren<RoomExitPattern>(true);

        if (!enableExit)
        {
            selectedPattern = null;
            SetAllPatternsActive(false);

            if (debugLog)
                Debug.Log(name + " は通常壁 / dir=" + direction);

            return;
        }

        SelectRandomPattern();
    }

    public void SelectRandomPattern()
    {
        if (patterns == null || patterns.Length == 0)
            patterns = GetComponentsInChildren<RoomExitPattern>(true);

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

    public void HideSelectedPattern()
    {
        if (selectedPattern != null)
            selectedPattern.gameObject.SetActive(false);
    }

    void SetAllPatternsActive(bool active)
    {
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
