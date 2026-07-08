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
        int clearCount = log.GetClearedRoomCount();

        if (reachedCount <= 0 && clearCount <= 0)
        {
            if (log.destroyedDoorCount + log.destroyedWallCount > 0)
                return "脱出前から破壊衝動あり";

            if (log.itemUseCount > 0 || log.throwCount > 0 || log.carryObjectUseCount > 0)
                return "準備運動で終了";

            if (log.movedDistance > 40f)
                return "迷子、まだ攻略前";

            return "測定前に終了";
        }

        if (log.destroyedClearCount > 0 || log.destroyedDoorCount > 0)
            return "強引な突破者";

        if (log.ideaUniqueClearCount >= Mathf.Max(1, clearCount * 0.5f))
            return "発想力豊か、斬新な攻略が多め";

        if (log.normalPuzzleClearCount >= Mathf.Max(1, clearCount * 0.7f))
            return "真面目、正規攻略多め";

        if (log.dashTime > log.totalRunTime * 0.45f && log.totalRunTime > 1f)
            return "急ぎすぎ、焦りすぎ、記録を意識している？";

        if (log.throwCount + log.carryObjectThrowCount >= 5)
            return "物の扱いが雑";

        if (log.itemUseCount >= 6)
            return "道具に頼りがち";

        if (log.bypassedPuzzleCount >= 2)
            return "パズル回避傾向";

        if (reachedCount >= 3 && log.movedDistance > reachedCount * 35f)
            return "探索多め";

        return "標準的な攻略傾向";
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
        int clearCount = log.GetClearedRoomCount();

        string text = "";

        text += "実験結果\n";
        text += "Room" + highestLevel + "内で活動停止\n";
        text += "攻略済みの部屋数 : " + reachedCount + "\n";
        text += "記録済みクリア数 : " + clearCount + "\n";
        text += "最高記録 : " + best + "\n";
        text += "特筆事項 : " + GetPlayStyleTitle() + "\n";
        text += "\n";

        text += "攻略データ\n";
        text += "- 正規攻略 : " + log.normalPuzzleClearCount + "\n";
        text += "- アイデア攻略 : " + log.ideaUniqueClearCount + "\n";
        text += "- 破壊突破 : " + log.destroyedClearCount + "\n";
        text += "\n";

        text += "行動データ\n";
        text += "- 移動した距離 : " + Mathf.RoundToInt(log.movedDistance) + "\n";
        text += "- ダッシュしていた時間 : " + log.dashTime.ToString("F1") + "秒\n";
        text += "- ジャンプした回数 : " + log.jumpCount + "\n";
        text += "- 道具の使用回数 : " + log.itemUseCount + "\n";
        text += "- 道具を投げた回数 : " + log.throwCount + "\n";
        text += "- 出口破壊 : " + log.destroyedDoorCount + "\n";

        return text;
    }
}
