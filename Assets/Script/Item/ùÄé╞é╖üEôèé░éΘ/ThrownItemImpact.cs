// ThrownItemImpact：投げたアイテムがぶつかった時のダメージ処理
//
// 相手に属性ダメージを与える。
// 必要なら自分自身の耐久も減らす。
// 投げた直後の自爆防止時間あり。

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
        rb = GetComponent<Rigidbody>();

        if (pickupItem == null)
        {
            pickupItem = GetComponent<PickupItem>();
        }
    }

    void OnEnable()
    {
        enabledTime = Time.time;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (pickupItem == null || rb == null)
            return;

        float speed = rb.linearVelocity.magnitude;

        if (speed < minImpactSpeed)
            return;

        AttributeDurability target =
            collision.collider.GetComponentInParent<AttributeDurability>();

        if (target != null)
        {
            target.DamageByItem(pickupItem);
        }

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
