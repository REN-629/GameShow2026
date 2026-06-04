//鈍器攻撃 バールなどに付ける
//左クリックで前方にSphereCastを飛ばし、壊せる物にダメージを与える

using UnityEngine;

public class BluntUseAction : ItemUseAction
{
    [Header("攻撃判定")]
    public Camera playerCamera;
    public float hitDistance = 2f;
    public float hitRadius = 0.35f;

    [Header("デバッグ")]
    public bool debugLog = true;

    public override bool Use(PickupItem item)
    {
        //バールを振る音は、当たらなくても鳴る
        PlayUseSE();

        if (item == null)
            return false;

        if (!item.HasEffectAttribute(EffectAttributeType.Blunt))
            return false;

        if (playerCamera == null)
        {
            Debug.LogWarning("BluntUseAction: playerCamera割り当て");
            playerCamera = Camera.main;
            return false;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.SphereCast(ray, hitRadius, out RaycastHit hit, hitDistance))
        {
            AttributeDurability target =
                hit.collider.GetComponentInParent<AttributeDurability>();

            if (target != null)
            {
                bool damaged = target.DamageByItem(item);

                if (debugLog)
                {
                    Debug.Log(damaged ? "鈍器攻撃ヒット" : "鈍器攻撃は効かなかった");
                }

                return damaged;
            }
        }

        if (debugLog)
        {
            Debug.Log("鈍器攻撃は空振り");
        }

        return false;
    }
}
