// 属性耐久：耐久、被ダメSE、破壊SE、破壊エフェクトを管理
// 被ダメSEと破壊SEは複数登録してランダム再生できる
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

    [Header("被ダメSE 複数登録可")]
    public AudioClip[] hitSEClips;

    [Range(0f, 1f)]
    public float hitSEVolume = 1f;

    [Header("破壊SE 複数登録可")]
    public AudioClip[] breakSEClips;

    [Range(0f, 1f)]
    public float breakSEVolume = 1f;

    [Header("破壊エフェクト")]
    public GameObject breakEffectPrefab;

    [Header("デバッグ")]
    public bool debugLog = true;

    private PickupItem pickupItem;

    void Awake()
    {
        pickupItem = GetComponent<PickupItem>();
    }

    public bool IsBroken()
    {
        return durability <= 0;
    }

    public bool DamageByItem(PickupItem attacker)
    {
        if (attacker == null)
            return false;

        if (ignoreDamageWhileHeld && pickupItem != null && pickupItem.IsHeld)
            return false;

        if (IsBroken())
            return false;

        int damage = CalculateDamage(attacker);

        if (damage <= 0)
            return false;

        return ApplyDamage(damage);
    }

    public bool ApplyDamage(int damage)
    {
        if (damage <= 0)
            return false;

        if (IsBroken())
            return false;

        durability -= damage;

        PlayHitSE();

        if (debugLog)
        {
            Debug.Log(name + " に " + damage + " ダメージ / 残り耐久: " + durability);
        }

        if (durability <= 0)
        {
            durability = 0;
            Break();
        }

        return true;
    }

    int CalculateDamage(PickupItem attacker)
    {
        int totalDamage = 0;

        foreach (EffectAttributeType weak in weakEffectAttributes)
        {
            if (attacker.HasEffectAttribute(weak))
            {
                totalDamage += 1;
            }
        }

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
        if (attacker == MaterialAttributeType.Wood &&
            target == MaterialAttributeType.Wood)
        {
            return 0;
        }

        if (attacker == MaterialAttributeType.Metal &&
            target == MaterialAttributeType.Wood)
        {
            return 1;
        }

        if (attacker == MaterialAttributeType.Stone &&
            target == MaterialAttributeType.Wood)
        {
            return 1;
        }

        return 0;
    }

    void PlayHitSE()
    {
        RandomAudioPlayer.PlayRandom(
            hitSEClips,
            transform.position,
            hitSEVolume
        );
    }

    void PlayBreakSE()
    {
        RandomAudioPlayer.PlayRandom(
            breakSEClips,
            transform.position,
            breakSEVolume
        );
    }

    void SpawnBreakEffect()
    {
        if (breakEffectPrefab != null)
        {
            Instantiate(
                breakEffectPrefab,
                transform.position,
                transform.rotation
            );
        }
    }

    void Break()
    {
        if (debugLog)
        {
            Debug.Log(name + " が耐久切れ");
        }

        PlayBreakSE();
        SpawnBreakEffect();

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
            if (debugLog)
            {
                Debug.Log(name + " は使用機能だけ停止した");
            }

            return;
        }

        if (breakMode == DurabilityBreakMode.KeepAsMaterial)
        {
            if (debugLog)
            {
                Debug.Log(name + " は物体として残る");
            }

            return;
        }
    }
}
