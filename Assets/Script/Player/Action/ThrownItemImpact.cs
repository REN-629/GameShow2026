// 投擲衝突：投げたアイテムが一定速度以上でぶつかった時、相手と自分にダメージ処理を行う
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

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();

        if (pickupItem == null)
        {
            pickupItem = GetComponentInParent<PickupItem>();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (pickupItem == null || rb == null)
            return;

        float speed = rb.linearVelocity.magnitude;

        if (speed < minImpactSpeed)
            return;

        // 相手へ属性ダメージ
        AttributeDurability target =
            collision.collider.GetComponentInParent<AttributeDurability>();

        if (target != null)
        {
            target.DamageByItem(pickupItem);
        }

        // 自分自身も落下・衝突で壊れるようにする
        if (damageSelfOnImpact)
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
