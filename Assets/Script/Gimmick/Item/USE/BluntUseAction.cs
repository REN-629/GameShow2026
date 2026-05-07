// 鈍器使用：左クリックで前方に攻撃判定を出し、属性耐久にダメージを与える
using UnityEngine;

public class BluntUseAction : ItemUseAction
{
    [Header("攻撃判定")]
    public Camera playerCamera;
    public float hitDistance = 2f;
    public float hitRadius = 0.35f;

    public override void Use(PickupItem item)
    {
        if (item == null)
            return;

        // 鈍器属性を持たないなら鈍器使用はしない
        if (!item.HasEffectAttribute(EffectAttributeType.Blunt))
            return;

        if (playerCamera == null)
        {
            Debug.LogWarning("BluntUseAction: playerCamera が未設定");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.SphereCast(ray, hitRadius, out RaycastHit hit, hitDistance))
        {
            AttributeDurability target =
                hit.collider.GetComponentInParent<AttributeDurability>();

            if (target != null)
            {
                target.DamageByItem(item);
            }
        }
    }
}
