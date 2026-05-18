// AttributeDurability：耐久を持つ物すべてに使うスクリプト
//
// 今回の版：破壊方法を FractureThis に統一しやすい版
//
// 仕組み：
// 耐久が0になる
// ↓
// 同じオブジェクトに FractureThis があれば BreakNow() を呼ぶ
// ↓
// 壁や箱が破片に砕ける
//
// FractureThis が付いていない場合だけ、予備として breakEffectPrefab を使う。
// つまり「基本はFracture、例外だけPrefab破壊」ができる。

using UnityEngine;
using Project.Scripts.Fractures;

public class AttributeDurability : MonoBehaviour
{
    // --------------------------------------------------
    // 耐久値
    // --------------------------------------------------

    [Header("耐久値")]
    [Tooltip("0になると壊れる")]
    public int durability = 5;

    [Header("耐久0時の挙動")]
    public DurabilityBreakMode breakMode = DurabilityBreakMode.DestroyObject;

    [Header("耐久0時に使用不可にする")]
    public bool disableUseOnBreak = true;

    [Header("耐久0時に投擲不可にする")]
    public bool disableThrowOnBreak = false;

    [Header("手に持っている間はダメージを受けない")]
    public bool ignoreDamageWhileHeld = true;

    // --------------------------------------------------
    // 属性
    // --------------------------------------------------

    [Header("この物体の材質属性")]
    public MaterialAttributeType[] materialAttributes;

    [Header("弱点：効果属性")]
    public EffectAttributeType[] weakEffectAttributes;

    [Header("材質相性を有効にする")]
    public bool useMaterialDamage = true;

    // --------------------------------------------------
    // 効果音
    // --------------------------------------------------

    [Header("被ダメSE 複数登録可")]
    public AudioClip[] hitSEClips;

    [Range(0f, 1f)]
    public float hitSEVolume = 1f;

    [Header("破壊SE 複数登録可")]
    public AudioClip[] breakSEClips;

    [Range(0f, 1f)]
    public float breakSEVolume = 1f;

    // --------------------------------------------------
    // Fracture破壊
    // --------------------------------------------------

    [Header("Fracture破壊")]
    [Tooltip("ONなら、FractureThisが付いている時に破砕処理を使う")]
    public bool useFractureThis = true;

    [Tooltip("FractureThisが無い時だけ、下の簡易破壊エフェクトを使う")]
    public bool usePrefabEffectIfNoFracture = true;

    // --------------------------------------------------
    // 予備の破壊エフェクト
    // --------------------------------------------------

    [Header("予備：破壊エフェクトPrefab")]
    [Tooltip("FractureThisが無い時だけ使う")]
    public GameObject breakEffectPrefab;

    [Header("予備：破壊エフェクト出現位置")]
    public Transform breakPoint;

    [Header("予備：破壊エフェクトの向き")]
    public bool useBreakPointRotation = true;

    // --------------------------------------------------
    // デバッグ
    // --------------------------------------------------

    [Header("デバッグ")]
    public bool debugLog = true;

    private PickupItem pickupItem;
    private bool alreadyBroken = false;

    void Awake()
    {
        pickupItem = GetComponent<PickupItem>();
    }

    public bool IsBroken()
    {
        return durability <= 0 || alreadyBroken;
    }

    // --------------------------------------------------
    // アイテムからダメージ
    // --------------------------------------------------

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

    // ライターの火など、効果属性だけでダメージを入れる用。
    public bool DamageByEffect(EffectAttributeType effect, int damage)
    {
        if (IsBroken())
            return false;

        if (!IsWeakToEffect(effect))
            return false;

        return ApplyDamage(damage);
    }

    bool IsWeakToEffect(EffectAttributeType effect)
    {
        if (weakEffectAttributes == null)
            return false;

        foreach (EffectAttributeType weak in weakEffectAttributes)
        {
            if (weak == effect)
                return true;
        }

        return false;
    }

    // --------------------------------------------------
    // 固定ダメージ
    // --------------------------------------------------

    public bool ApplyDamage(int damage)
    {
        if (damage <= 0)
            return false;

        if (IsBroken())
            return false;

        durability -= damage;

        RandomAudioPlayer.PlayRandom(hitSEClips, transform.position, hitSEVolume);

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

    // --------------------------------------------------
    // ダメージ計算
    // --------------------------------------------------

    int CalculateDamage(PickupItem attacker)
    {
        int totalDamage = 0;

        foreach (EffectAttributeType weak in weakEffectAttributes)
        {
            if (attacker.HasEffectAttribute(weak))
                totalDamage += 1;
        }

        if (useMaterialDamage)
            totalDamage += CalculateMaterialDamage(attacker);

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
                    bestDamage = damage;
            }
        }

        return bestDamage;
    }

    int GetMaterialDamage(MaterialAttributeType attacker, MaterialAttributeType target)
    {
        if (attacker == MaterialAttributeType.Wood &&
            target == MaterialAttributeType.Wood)
            return 0;

        if (attacker == MaterialAttributeType.Metal &&
            target == MaterialAttributeType.Wood)
            return 1;

        if (attacker == MaterialAttributeType.Stone &&
            target == MaterialAttributeType.Wood)
            return 1;

        return 0;
    }

    // --------------------------------------------------
    // 壊れる処理
    // --------------------------------------------------

    void Break()
    {
        if (alreadyBroken)
            return;

        alreadyBroken = true;

        Vector3 breakPos = GetBreakEffectPosition();

        RandomAudioPlayer.PlayRandom(breakSEClips, breakPos, breakSEVolume);

        if (pickupItem != null)
        {
            if (disableUseOnBreak)
                pickupItem.canUseWithLeftClick = false;

            if (disableThrowOnBreak)
                pickupItem.canThrow = false;
        }

        if (debugLog)
        {
            Debug.Log(name + " は耐久0になりました");
        }

        // 1. 基本はFractureThisで破砕する
        if (useFractureThis)
        {
            FractureThis fracture = GetComponent<FractureThis>();

            if (fracture != null)
            {
                fracture.BreakNow();
                return;
            }

            if (debugLog)
            {
                Debug.LogWarning(name + " に FractureThis が無いため、Prefab破壊へフォールバックします");
            }
        }

        // 2. FractureThisが無い場合だけ簡易破壊エフェクト
        if (usePrefabEffectIfNoFracture)
        {
            SpawnBreakEffect();
        }

        // 3. DestroyObjectなら本体を消す
        if (breakMode == DurabilityBreakMode.DestroyObject)
        {
            Destroy(gameObject);
            return;
        }
    }

    void SpawnBreakEffect()
    {
        if (breakEffectPrefab == null)
            return;

        Instantiate(
            breakEffectPrefab,
            GetBreakEffectPosition(),
            GetBreakEffectRotation()
        );
    }

    Vector3 GetBreakEffectPosition()
    {
        if (breakPoint != null)
            return breakPoint.position;

        return transform.position;
    }

    Quaternion GetBreakEffectRotation()
    {
        if (breakPoint != null && useBreakPointRotation)
            return breakPoint.rotation;

        return transform.rotation;
    }
}
