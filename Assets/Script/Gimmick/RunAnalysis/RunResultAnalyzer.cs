using UnityEngine;

// RunResultAnalyzer
//
// 修正版:
// RunScoreManagerの新しい変数名に対応。
// 旧:
// clearedRoomCount
// bestClearedRoomCount
//
// 新:
// reachedRoomCount
// bestReachedRoomCount

public class RunResultAnalyzer : MonoBehaviour
{
    public static RunResultAnalyzer Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public string GetPlayStyleTitle()
    {
        RunActionLogger log = RunActionLogger.Instance;
        RunScoreManager score = RunScoreManager.Instance;

        if (log == null)
            return "記録なし";

        int reachedCount = score != null ? score.reachedRoomCount : 0;

        if (log.destroyedClearCount >= 3 || log.destroyedDoorCount + log.destroyedWallCount >= 5)
            return "破壊衝動型";

        if (log.dashTime > log.totalRunTime * 0.45f && log.totalRunTime > 1f)
            return "急ぎすぎ突破型";

        if (log.shortcutClearCount >= 2)
            return "抜け道発見型";

        if (reachedCount >= 5 && log.normalPuzzleClearCount >= reachedCount * 0.7f)
            return "正攻法攻略型";

        if (log.throwCount >= 5)
            return "投擲解決型";

        if (log.itemUseCount >= 6)
            return "道具頼り型";

        if (log.bypassedPuzzleCount >= 2)
            return "ギミック無視型";

        return "探索者型";
    }

    public string BuildResultText()
    {
        RunActionLogger log = RunActionLogger.Instance;
        RunScoreManager score = RunScoreManager.Instance;

        if (log == null)
            return "行動ログがありません。";

        int reachedCount = score != null ? score.reachedRoomCount : 0;
        int best = score != null ? score.bestReachedRoomCount : 0;
        int highestLevel = score != null ? score.highestReachedLevel : 1;

        string text = "";

        text += "RESULT\n";
        text += "STYLE : " + GetPlayStyleTitle() + "\n";
        text += "ROOM : " + reachedCount + "\n";
        text += "BEST : " + best + "\n";
        text += "HIGHEST LEVEL : " + highestLevel + "\n";
        text += "\n";

        text += "CLEAR METHOD\n";
        text += "- 正規クリア : " + log.normalPuzzleClearCount + "\n";
        text += "- 重量ボタン : " + log.pressurePlateClearCount + "\n";
        text += "- 到達トリガー : " + log.goalTriggerClearCount + "\n";
        text += "- 破壊突破 : " + log.destroyedClearCount + "\n";
        text += "- ショートカット : " + log.shortcutClearCount + "\n";
        text += "- パズル無視 : " + log.bypassedPuzzleCount + "\n";
        text += "\n";

        text += "ACTION\n";
        text += "- 移動距離 : " + Mathf.RoundToInt(log.movedDistance) + "\n";
        text += "- ダッシュ時間 : " + log.dashTime.ToString("F1") + "秒\n";
        text += "- ジャンプ : " + log.jumpCount + "\n";
        text += "- アイテム使用 : " + log.itemUseCount + "\n";
        text += "- 投擲 : " + log.throwCount + "\n";
        text += "- 攻撃 : " + log.attackCount + "\n";

        return text;
    }
}
