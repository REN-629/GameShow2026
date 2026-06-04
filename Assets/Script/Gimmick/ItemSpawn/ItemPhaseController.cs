using UnityEngine;

// ItemPhaseController
//
// アイテム収集フェーズ管理。
// 条件:
// 1. アイテムフェーズタイマーが0になる
// 2. プレイヤーがアイテム部屋から退出する
//
// どちらでも同じ処理:
// ・アイテム部屋タイマー停止
// ・アイテム部屋をFractureThisで崩壊
// ・指定時間後にアイテム部屋Rootを削除
// ・攻略フェーズ用タイマーへ切り替え
//
// シーン遷移なし。

public class ItemPhaseController : MonoBehaviour
{
    public static ItemPhaseController Instance { get; private set; }

    [Header("アイテム部屋タイマー")]
    public ItemPhaseTimer itemPhaseTimer;

    [Header("攻略フェーズタイマー")]
    public RoomRunTimer roomRunTimer;

    [Header("アイテム部屋")]
    [Tooltip("FractureThisなどが付いている崩壊対象")]
    public GameObject fractureTarget;

    [Tooltip("指定時間後に削除するアイテム部屋全体Root")]
    public GameObject itemRoomRoot;

    [Header("崩壊")]
    public string fractureMessage = "BreakNow";

    [Header("削除")]
    public float deleteDelay = 3f;

    [Header("攻略タイマー開始")]
    [Tooltip("ONならアイテムフェーズ終了時に攻略タイマーを開始")]
    public bool startRoomTimerOnEnd = true;

    [Tooltip("ONなら攻略タイマーを最初は止めておき、このControllerから開始する")]
    public bool roomTimerStartsStopped = true;

    [Header("一度だけ")]
    public bool onlyOnce = true;

    [Header("デバッグ")]
    public bool debugLog = true;

    private bool ended = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (roomRunTimer != null && roomTimerStartsStopped)
        {
            roomRunTimer.StopTimer();
        }
    }

    public void EndItemPhase()
    {
        if (onlyOnce && ended)
            return;

        ended = true;

        if (debugLog)
            Debug.Log("アイテムフェーズ終了");

        if (itemPhaseTimer != null)
        {
            itemPhaseTimer.StopTimer();
            itemPhaseTimer.gameObject.SetActive(false);
        }

        if (fractureTarget != null)
        {
            fractureTarget.SendMessage(fractureMessage, SendMessageOptions.DontRequireReceiver);
        }

        if (startRoomTimerOnEnd && roomRunTimer != null)
        {
            roomRunTimer.gameObject.SetActive(true);
            roomRunTimer.StartTimer();
        }

        StartCoroutine(DeleteItemRoomAfterDelay());
    }

    System.Collections.IEnumerator DeleteItemRoomAfterDelay()
    {
        yield return new WaitForSeconds(deleteDelay);

        if (itemRoomRoot != null)
        {
            if (debugLog)
                Debug.Log("アイテム部屋削除: " + itemRoomRoot.name);

            Destroy(itemRoomRoot);
        }
    }
}
