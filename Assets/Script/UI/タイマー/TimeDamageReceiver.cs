using UnityEngine;

public class TimeDamageReceiver : MonoBehaviour
{
    public float damageCooldown = 0.5f;
    public bool debugLog = true;

    private float lastDamageTime = -999f;

    public void ApplyTimeDamage(float timeDamage)
    {
        if (Time.time - lastDamageTime < damageCooldown)
            return;

        lastDamageTime = Time.time;

        float damage = Mathf.Abs(timeDamage);

        if (RoomRunTimer.RunInstance != null)
            RoomRunTimer.RunInstance.AddTime(-damage);
        else if (GamePhaseTimer.Instance != null)
            GamePhaseTimer.Instance.AddTime(-damage);

        if (debugLog)
            Debug.Log("時間ダメージ: -" + damage);
    }
}
