using UnityEngine;

public class TimeDamageReceiver : MonoBehaviour
{
    [Header("連続ダメージ防止")]
    public float damageCooldown = 0.5f;

    [Header("ダメージ時の差分UI")]
    public DeltaPopupUI damageDeltaPopup;

    [Header("デバッグ")]
    public bool debugLog = true;

    private float lastDamageTime = -999f;

    public void ApplyTimeDamage(float timeDamage)
    {
        if (Time.time - lastDamageTime < damageCooldown)
            return;

        lastDamageTime = Time.time;

        float damage = Mathf.Abs(timeDamage);

        if (RoomRunTimer.RunInstance != null)
        {
            RoomRunTimer.RunInstance.AddTime(-damage);
        }
        else if (GamePhaseTimer.Instance != null)
        {
            GamePhaseTimer.Instance.AddTime(-damage);
        }
        else
        {
            Debug.LogWarning("時間ダメージを与えようとしましたが Timer がありません");
        }

        if (damageDeltaPopup != null)
            damageDeltaPopup.AddDelta(-damage);

        if (debugLog)
            Debug.Log("時間ダメージ: -" + damage);
    }
}
