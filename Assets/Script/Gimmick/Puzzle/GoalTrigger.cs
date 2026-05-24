// GoalTrigger：触れたら部屋パズルをクリアするトリガー

using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [Header("プレイヤーのみ反応")]
    public bool playerOnly = true;

    [Header("一度だけ")]
    public bool triggerOnce = true;

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && triggered)
            return;

        if (playerOnly && !other.CompareTag("Player"))
            return;

        triggered = true;

        RoomPuzzleTarget target = GetComponent<RoomPuzzleTarget>();

        if (target != null)
            target.ClearTargetRoom();
        else if (RoomRuntimeManager.Instance != null)
            RoomRuntimeManager.Instance.ClearCurrentRoomPuzzle();
    }
}
