// LighterUseAction：ライター専用の使用処理
// 左クリックでON/OFF。
// ON中だけ火の見た目を出し、前方に火判定を出す。
// Fireに弱いAttributeDurabilityへ継続ダメージを与える。

using UnityEngine;

public class LighterUseAction : ToggleUseAction
{
    [Header("火の見た目")]
    public GameObject[] flameObjects;

    [Header("火の発生位置")]
    public Transform flamePoint;

    [Header("火ダメージ判定")]
    public float fireDistance = 1.2f;
    public float fireRadius = 0.18f;
    public float fireDamageInterval = 1f;
    public int fireDamage = 1;

    [Header("火が当たるLayer")]
    public LayerMask fireLayerMask = ~0;

    [Header("デバッグ")]
    public bool debugFireHit = false;

    private float fireTimer = 0f;

    protected override void OnTurnOn()
    {
        SetFlameVisible(true);
        fireTimer = 0f;
    }

    protected override void OnTurnOff()
    {
        SetFlameVisible(false);
        fireTimer = 0f;
    }

    protected override void OnToggleUpdate()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer < fireDamageInterval)
            return;

        fireTimer = 0f;
        TryApplyFireDamage();
    }

    void SetFlameVisible(bool visible)
    {
        if (flameObjects == null)
            return;

        foreach (GameObject obj in flameObjects)
        {
            if (obj != null)
                obj.SetActive(visible);
        }
    }

    void TryApplyFireDamage()
    {
        Transform origin = flamePoint != null ? flamePoint : transform;
        Ray ray = new Ray(origin.position, origin.forward);

        if (Physics.SphereCast(ray, fireRadius, out RaycastHit hit, fireDistance, fireLayerMask, QueryTriggerInteraction.Ignore))
        {
            AttributeDurability target = hit.collider.GetComponentInParent<AttributeDurability>();

            if (target != null)
            {
                bool damaged = target.DamageByEffect(EffectAttributeType.Fire, fireDamage);

                if (debugFireHit)
                    Debug.Log(damaged ? "火ダメージ成功: " + target.name : "火は効かなかった: " + target.name);
            }
        }
    }

    void OnDisable()
    {
        SetFlameVisible(false);
    }
}
