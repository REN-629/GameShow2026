//リザルトで使うデータ、表示する内容
using UnityEngine;
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
            return "破壊衝動が強め";

        if (log.dashTime > log.totalRunTime * 0.45f && log.totalRunTime > 1f)
            return "急ぎすぎ、焦りすぎ";

        if (log.shortcutClearCount >= 2)
            return "発想力豊か、想定外の攻略法多め";

        if (reachedCount >= 5 && log.normalPuzzleClearCount >= reachedCount * 0.7f)
            return "真面目、いたって普通";

        if (log.throwCount >= 5)
            return "道具の扱いが雑";

        if (log.itemUseCount >= 6)
            return "道具に頼りがち";

        if (log.bypassedPuzzleCount >= 2)
            return "パズルが嫌い？もしくは強引";

        return "攻略よりも周囲の探索が多い";
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

        text += "実験結果\n";
        text += "Room" + highestLevel + "内で活動停止\n";
        text += "攻略済みの部屋数 : " + reachedCount + "\n";
        text += "最高記録 : " + best + "\n";
        text += "特筆事項 : " + GetPlayStyleTitle() + "\n";

        text += "\n";

        text += "攻略データ\n";
        text += "- 正規クリア : " + log.normalPuzzleClearCount + "\n";
        text += "- 重量ボタン : " + log.pressurePlateClearCount + "\n";
        text += "- 到達トリガー : " + log.goalTriggerClearCount + "\n";
        text += "- 破壊による突破 : " + log.destroyedClearCount + "\n";
        text += "- ショートカット : " + log.shortcutClearCount + "\n";
        text += "- パズル無視 : " + log.bypassedPuzzleCount + "\n";
        text += "\n";

        text += "行動データ\n";
        text += "- 移動した距離 : " + Mathf.RoundToInt(log.movedDistance) + "\n";
        text += "- ダッシュしていた時間 : " + log.dashTime.ToString("F1") + "秒\n";
        text += "- ジャンプした回数 : " + log.jumpCount + "\n";
        text += "- 道具の使用回数 : " + log.itemUseCount + "\n";

        return text;
    }
}
