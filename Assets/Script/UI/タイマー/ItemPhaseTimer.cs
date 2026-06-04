using UnityEngine;

// ItemPhaseTimer
//
// アイテム収集フェーズ専用タイマー。
// 0秒になったらシーン遷移はせず、ItemPhaseController.EndItemPhase() を呼ぶ。

public class ItemPhaseTimer : GamePhaseTimer
{
    [Header("アイテムフェーズController")]
    public ItemPhaseController controller;

    protected override void TimeOut()
    {
        if (controller != null)
            controller.EndItemPhase();
        else if (ItemPhaseController.Instance != null)
            ItemPhaseController.Instance.EndItemPhase();
        else
            Debug.LogWarning("ItemPhaseController がありません");

        // base.TimeOut() は呼ばない。
        // シーン遷移なしにするため。
        StopTimer();
    }
}
