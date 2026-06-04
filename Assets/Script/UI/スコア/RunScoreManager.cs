using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// RunScoreManager
//
// スコア加算条件:
// パズルクリア時ではなく、次の部屋のTriggerに触れた時。
//
// ただし、同じroomLevelは1回しか加算しない。
// 例:
// Room1から次の部屋が2つ生成される
// どちらもLevel2
// 先に入った方で+1
// もう片方に入ってもLevel2は加算済みなので+0

public class RunScoreManager : MonoBehaviour
{
    public static RunScoreManager Instance { get; private set; }

    [Header("現在の到達部屋数")]
    public int reachedRoomCount = 0;

    [Header("最高到達部屋数")]
    public int bestReachedRoomCount = 0;

    [Header("現在到達済み最大レベル")]
    public int highestReachedLevel = 1;

    [Header("UI / TMP")]
    public TextMeshProUGUI reachedRoomTMP;
    public TextMeshProUGUI bestRoomTMP;

    [Header("UI / Legacy Text")]
    public Text reachedRoomText;
    public Text bestRoomText;

    [Header("保存キー")]
    public string bestScoreKey = "BestReachedRoomCount";

    [Header("デバッグ")]
    public bool debugLog = true;

    private HashSet<int> countedLevels =
        new HashSet<int>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        bestReachedRoomCount = PlayerPrefs.GetInt(bestScoreKey, 0);

        // スタート部屋Level1は最初から到達済み扱い。
        countedLevels.Add(1);
        highestReachedLevel = 1;
        reachedRoomCount = 0;

        UpdateUI();
    }

    public bool IsLevelCounted(int level)
    {
        return countedLevels.Contains(level);
    }

    public void RegisterRoomReached(RoomIdentity roomIdentity)
    {
        if (roomIdentity == null)
            return;

        RegisterRoomReached(roomIdentity.roomLevel, roomIdentity.roomId);
    }

    public void RegisterRoomReached(int roomLevel, string roomId)
    {
        if (roomLevel <= 1)
            return;

        if (countedLevels.Contains(roomLevel))
        {
            if (debugLog)
                Debug.Log("既に到達スコア加算済み Level: " + roomLevel);

            return;
        }

        countedLevels.Add(roomLevel);
        highestReachedLevel = Mathf.Max(highestReachedLevel, roomLevel);
        reachedRoomCount++;

        if (RunActionLogger.Instance != null)
            RunActionLogger.Instance.LogRoomReached(roomLevel, roomId);

        if (reachedRoomCount > bestReachedRoomCount)
        {
            bestReachedRoomCount = reachedRoomCount;
            PlayerPrefs.SetInt(bestScoreKey, bestReachedRoomCount);
            PlayerPrefs.Save();
        }

        if (debugLog)
        {
            Debug.Log(
                "到達部屋数 +1: "
                + reachedRoomCount
                + " / level="
                + roomLevel
                + " / roomId="
                + roomId
            );
        }

        UpdateUI();
    }

    public void ResetRunScore()
    {
        countedLevels.Clear();
        countedLevels.Add(1);

        reachedRoomCount = 0;
        highestReachedLevel = 1;

        UpdateUI();
    }

    public void UpdateUI()
    {
        string reachedTextValue = "ROOM : " + reachedRoomCount;
        string bestTextValue = "BEST : " + bestReachedRoomCount;

        if (reachedRoomTMP != null)
            reachedRoomTMP.text = reachedTextValue;

        if (bestRoomTMP != null)
            bestRoomTMP.text = bestTextValue;

        if (reachedRoomText != null)
            reachedRoomText.text = reachedTextValue;

        if (bestRoomText != null)
            bestRoomText.text = bestTextValue;
    }

    public void BindTMP(TextMeshProUGUI reachedText, TextMeshProUGUI bestText)
    {
        reachedRoomTMP = reachedText;
        bestRoomTMP = bestText;
        UpdateUI();
    }

    public void BindLegacyText(Text reachedText, Text bestText)
    {
        reachedRoomText = reachedText;
        bestRoomText = bestText;
        UpdateUI();
    }
}
