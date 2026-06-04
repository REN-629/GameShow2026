using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TimeDamageTrigger : MonoBehaviour
{
    public float timeDamage = 5f;
    public bool damageWhileStay = false;
    public float stayDamageInterval = 1f;
    public string playerTag = "Player";

    private float lastStayDamageTime = -999f;

    void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        DealDamage(other);
    }

    void OnTriggerStay(Collider other)
    {
        if (!damageWhileStay)
            return;

        if (!other.CompareTag(playerTag))
            return;

        if (Time.time - lastStayDamageTime < stayDamageInterval)
            return;

        lastStayDamageTime = Time.time;
        DealDamage(other);
    }

    void DealDamage(Collider other)
    {
        TimeDamageReceiver receiver =
            other.GetComponentInParent<TimeDamageReceiver>();

        if (receiver != null)
            receiver.ApplyTimeDamage(timeDamage);
    }
}
