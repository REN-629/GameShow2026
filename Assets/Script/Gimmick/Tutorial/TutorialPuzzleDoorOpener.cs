using UnityEngine;

[System.Obsolete("RoomPuzzleTargetを推奨。古いチュートリアル用ドア制御互換です。")]\npublic class TutorialPuzzleDoorOpener : MonoBehaviour
{
    [Header("制御するドア")]
    public DoorController[] doors;

    [Header("開放時に有効化")]
    public GameObject[] enableOnClear;

    [Header("開放時に無効化")]
    public GameObject[] disableOnClear;

    [Header("一度だけ")]
    public bool triggerOnce = true;

    [Header("デバッグ")]
    public bool debugLog = true;

    private bool cleared;

    public bool IsCleared()
    {
        return cleared;
    }

    public void Clear()
    {
        if (triggerOnce && cleared)
            return;

        cleared = true;

        OpenDoors();
        ApplyObjects();

        if (debugLog)
            Debug.Log(name + " チュートリアルPuzzleクリア");
    }

    public void OpenDoors()
    {
        if (doors == null)
            return;

        foreach (DoorController door in doors)
        {
            if (door != null)
                door.Open();
        }
    }

    public void CloseDoors()
    {
        if (doors == null)
            return;

        foreach (DoorController door in doors)
        {
            if (door != null)
                door.Close();
        }
    }

    void ApplyObjects()
    {
        if (enableOnClear != null)
        {
            foreach (GameObject obj in enableOnClear)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }

        if (disableOnClear != null)
        {
            foreach (GameObject obj in disableOnClear)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
    }
}
