using UnityEngine;

public class ItemPhaseExitController : MonoBehaviour
{
    public GameObject fractureTarget;
    public GameObject deleteTarget;

    public string fractureMessage = "BreakNow";
    public float deleteDelay = 3f;

    public bool onlyOnce = true;
    public bool debugLog = true;

    private bool triggered = false;

    public void TriggerExit()
    {
        if (onlyOnce && triggered)
            return;

        triggered = true;

        if (debugLog)
            Debug.Log("アイテムフェーズ終了処理開始");

        if (fractureTarget != null)
            fractureTarget.SendMessage(fractureMessage, SendMessageOptions.DontRequireReceiver);

        StartCoroutine(DeleteAfterDelay());
    }

    System.Collections.IEnumerator DeleteAfterDelay()
    {
        yield return new WaitForSeconds(deleteDelay);

        if (deleteTarget != null)
        {
            if (debugLog)
                Debug.Log("アイテム部屋削除: " + deleteTarget.name);

            Destroy(deleteTarget);
        }
    }
}
