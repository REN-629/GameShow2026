// 鈍器使用：左クリックで前方に攻撃判定を出し、属性耐久にダメージを与える
// 何かに有効ヒットした時だけ true を返す。
// true の時だけ PickupItem 側で道具耐久が減る。
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
        // バールを振る音など。ヒットしなくても使用したので鳴らす。
        PlayUseSE();

        if (item == null)
            return false;

        if (!item.HasEffectAttribute(EffectAttributeType.Blunt))
            return false;

        if (playerCamera == null)
        {
            Debug.LogWarning("BluntUseAction: playerCamera が未設定");
            return false;
        }

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.SphereCast(
            ray,
            hitRadius,
            out RaycastHit hit,
            hitDistance))
        {
            AttributeDurability target =
                hit.collider.GetComponentInParent<AttributeDurability>();

            if (target != null)
            {
                bool damaged = target.DamageByItem(item);

                if (debugLog)
                {
                    Debug.Log(
                        damaged
                        ? item.name + " の鈍器攻撃が有効ヒット"
                        : item.name + " の鈍器攻撃は効かなかった"
                    );
                }

                return damaged;
            }
        }

        if (debugLog)
        {
            Debug.Log(item.name + " の鈍器攻撃は空振り");
        }

        return false;
    }
}
