// 投擲衝突：投げたアイテムが一定速度以上でぶつかった時、相手と自分にダメージ処理を行う
// 投げた直後の自爆・即破壊を防ぐため、自分へのダメージだけ一定時間無効化する
// 衝突音は ImpactSoundEmitter 側に分離
using UnityEngine;

public class ThrownItemImpact : MonoBehaviour
{
    public PickupItem pickupItem;

    [Header("この速度以上で衝突した時だけダメージ判定")]
    public float minImpactSpeed = 4f;

    [Header("衝突した自分自身にもダメージを入れる")]
    public bool damageSelfOnImpact = true;

    [Header("自分自身への衝突ダメージ")]
    public int selfDamage = 1;

    [Header("投げた直後の自爆防止時間")]
    public float ignoreSelfDamageTime = 0.3f;

    private Rigidbody rb;
    private float enabledTime;

    void Awake()
    {
        // RigidbodyはPickupItemと同じRootに付けるのが理想
        rb = GetComponentInParent<Rigidbody>();

        if (pickupItem == null)
        {
            pickupItem = GetComponentInParent<PickupItem>();
        }
    }

    void OnEnable()
    {
        // 投げた直後や再表示された直後の時刻を記録
        enabledTime = Time.time;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (pickupItem == null || rb == null)
            return;

        float speed = rb.linearVelocity.magnitude;

        if (speed < minImpactSpeed)
            return;

        // 相手側へ属性ダメージ
        AttributeDurability target =
            collision.collider.GetComponentInParent<AttributeDurability>();

        if (target != null)
        {
            target.DamageByItem(pickupItem);
        }

        // 自分自身への衝突ダメージ
        // 投げた直後はプレイヤーや近くのColliderに当たりやすいので一定時間無効化
        if (damageSelfOnImpact && Time.time - enabledTime >= ignoreSelfDamageTime)
        {
            AttributeDurability self =
                pickupItem.GetComponent<AttributeDurability>();

            if (self != null)
            {
                self.ApplyDamage(selfDamage);
            }
        }
    }
}
