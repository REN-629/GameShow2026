// 属性耐久：道具・壊せる壁・箱など、耐久を持つ物すべてに使う統合スクリプト
using UnityEngine;

public class AttributeDurability : MonoBehaviour
{
    [Header("耐久値")]
    public int durability = 5;

    [Header("耐久0時の挙動")]
    public DurabilityBreakMode breakMode = DurabilityBreakMode.DestroyObject;

    [Header("耐久0時に使用不可にする")]
    public bool disableUseOnBreak = true;

    [Header("耐久0時に投擲不可にする")]
    public bool disableThrowOnBreak = false;

    [Header("手に持っている間はダメージを受けない")]
    public bool ignoreDamageWhileHeld = true;

    [Header("この物体の材質属性")]
    public MaterialAttributeType[] materialAttributes;

    [Header("弱点：効果属性")]
    public EffectAttributeType[] weakEffectAttributes;

    [Header("材質相性を有効にする")]
    public bool useMaterialDamage = true;

    private PickupItem pickupItem;

    void Awake()
    {
        pickupItem = GetComponent<PickupItem>();
    }

    public bool IsBroken()
    {
        return durability <= 0;
    }

    public void DamageByItem(PickupItem attacker)
    {
        if (attacker == null)
            return;

        if (ignoreDamageWhileHeld && pickupItem != null && pickupItem.IsHeld)
            return;

        if (IsBroken())
            return;

        int damage = CalculateDamage(attacker);

        if (damage <= 0)
            return;

        ApplyDamage(damage);
    }

    public void ApplyDamage(int damage)
    {
        if (damage <= 0)
            return;

        if (IsBroken())
            return;

        durability -= damage;

        Debug.Log(name + " に " + damage + " ダメージ / 残り耐久: " + durability);

        if (durability <= 0)
        {
            durability = 0;
            Break();
        }
    }

    int CalculateDamage(PickupItem attacker)
    {
        int totalDamage = 0;

        // 1. 効果属性ダメージ
        // 例：木の壁が Blunt に弱い場合、Blunt属性のバールで+1
        foreach (EffectAttributeType weak in weakEffectAttributes)
        {
            if (attacker.HasEffectAttribute(weak))
            {
                totalDamage += 1;
            }
        }

        // 2. 材質相性ダメージ
        // 木と木は相殺気味なので+0
        // 金属で木を叩く/ぶつけると+1
        if (useMaterialDamage)
        {
            totalDamage += CalculateMaterialDamage(attacker);
        }

        return totalDamage;
    }

    int CalculateMaterialDamage(PickupItem attacker)
    {
        int bestDamage = 0;

        foreach (MaterialAttributeType attackerMaterial in attacker.materialAttributes)
        {
            foreach (MaterialAttributeType targetMaterial in materialAttributes)
            {
                int damage = GetMaterialDamage(attackerMaterial, targetMaterial);

                if (damage > bestDamage)
                {
                    bestDamage = damage;
                }
            }
        }

        return bestDamage;
    }

    int GetMaterialDamage(MaterialAttributeType attacker, MaterialAttributeType target)
    {
        // 木 → 木
        // 木製の物で木製の物を殴った場合、材質分は相殺される。
        // 鈍器属性があれば、鈍器分だけで1ダメージになる。
        if (attacker == MaterialAttributeType.Wood &&
            target == MaterialAttributeType.Wood)
        {
            return 0;
        }

        // 金属 → 木
        // バールや金属小物が木に効く。
        if (attacker == MaterialAttributeType.Metal &&
            target == MaterialAttributeType.Wood)
        {
            return 1;
        }

        // 石 → 木
        // 今後、石を追加した時用。
        if (attacker == MaterialAttributeType.Stone &&
            target == MaterialAttributeType.Wood)
        {
            return 1;
        }

        return 0;
    }

    void Break()
    {
        Debug.Log(name + " が耐久切れ");

        if (pickupItem != null)
        {
            if (disableUseOnBreak)
            {
                pickupItem.canUseWithLeftClick = false;
            }

            if (disableThrowOnBreak)
            {
                pickupItem.canThrow = false;
            }
        }

        if (breakMode == DurabilityBreakMode.DestroyObject)
        {
            Destroy(gameObject);
            return;
        }

        if (breakMode == DurabilityBreakMode.DisableUseOnly)
        {
            Debug.Log(name + " は使用機能だけ停止した");
            return;
        }

        if (breakMode == DurabilityBreakMode.KeepAsMaterial)
        {
            Debug.Log(name + " は物体として残る");
            return;
        }
    }
}
