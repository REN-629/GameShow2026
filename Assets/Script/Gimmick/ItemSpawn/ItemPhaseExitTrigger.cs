using UnityEngine;

// ItemPhaseExitTrigger
//
// アイテム部屋の出口に置くTrigger。
// プレイヤーが一度でも触れたら、アイテムフェーズを即終了する。

[RequireComponent(typeof(Collider))]
public class ItemPhaseExitTrigger : MonoBehaviour
{
    public ItemPhaseController controller;
    public string playerTag = "Player";

    void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        ItemPhaseController target =
            controller != null ? controller : ItemPhaseController.Instance;

        if (target != null)
            target.EndItemPhase();
    }
}
