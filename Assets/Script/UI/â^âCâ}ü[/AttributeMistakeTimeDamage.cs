using UnityEngine;

public static class AttributeMistakeTimeDamage
{
    public static void ApplyToPlayer(GameObject player, float damageTime)
    {
        if (player == null)
            return;

        TimeDamageReceiver receiver =
            player.GetComponentInParent<TimeDamageReceiver>();

        if (receiver != null)
            receiver.ApplyTimeDamage(damageTime);
    }

    public static void ApplyToCollider(Collider playerCollider, float damageTime)
    {
        if (playerCollider == null)
            return;

        TimeDamageReceiver receiver =
            playerCollider.GetComponentInParent<TimeDamageReceiver>();

        if (receiver != null)
            receiver.ApplyTimeDamage(damageTime);
    }
}
