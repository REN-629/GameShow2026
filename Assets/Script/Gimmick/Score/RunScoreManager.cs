using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using TMPro;

public class RunScoreManager : MonoBehaviour
{
    public static RunScoreManager Instance { get; private set; }

    [Header("現在のクリア部屋数")]
    public int clearedRoomCount = 0;

    [Header("ベスト記録")]
    public int bestClearedRoomCount = 0;

    [Header("UI")]
    public Text clearedRoomText;
    public Text bestRoomText;

    [Header("保存キー")]
    public string bestScoreKey = "BestClearedRoomCount";

    [Header("デバッグ")]
    public bool debugLog = true;

    private HashSet<string> clearedRoomIds =
        new HashSet<string>();

    void Awake()
    {
        Instance = this;
        bestClearedRoomCount = PlayerPrefs.GetInt(bestScoreKey, 0);
        UpdateUI();
    }

    public bool IsRoomCleared(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
            return false;

        return clearedRoomIds.Contains(roomId);
    }

    public void RegisterRoomCleared(RoomPuzzleState room, RoomClearMethod method)
    {
        if (room == null)
            return;

        RoomIdentity identity = room.GetComponent<RoomIdentity>();

        if (identity == null)
        {
            Debug.LogWarning(room.name + " に RoomIdentity がありません");
            return;
        }

        RegisterRoomCleared(identity.roomId, method);
    }

    public void RegisterRoomCleared(string roomId, RoomClearMethod method)
    {
        if (string.IsNullOrEmpty(roomId))
            return;

        if (clearedRoomIds.Contains(roomId))
        {
            if (debugLog)
                Debug.Log("既にスコア加算済み: " + roomId);

            return;
        }

        clearedRoomIds.Add(roomId);
        clearedRoomCount++;

        if (RunActionLogger.Instance != null)
            RunActionLogger.Instance.LogRoomCleared(roomId, method);

        if (debugLog)
        {
            Debug.Log(
                "クリア部屋数 +1: "
                + clearedRoomCount
                + " / roomId="
                + roomId
                + " / method="
                + method
            );
        }

        if (clearedRoomCount > bestClearedRoomCount)
        {
            bestClearedRoomCount = clearedRoomCount;
            PlayerPrefs.SetInt(bestScoreKey, bestClearedRoomCount);
            PlayerPrefs.Save();

            if (debugLog)
                Debug.Log("ベスト更新: " + bestClearedRoomCount);
        }

        UpdateUI();
    }

    public void ResetRunScore()
    {
        clearedRoomIds.Clear();
        clearedRoomCount = 0;
        UpdateUI();

        if (debugLog)
            Debug.Log("スコアをリセット");
    }

    public void UpdateUI()
    {
        if (clearedRoomText != null)
            clearedRoomText.text = " " + clearedRoomCount;

        if (bestRoomText != null)
            bestRoomText.text = " " + bestClearedRoomCount;
    }
}
