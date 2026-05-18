// CharacterPushRigidbodies：CharacterControllerでRigidbodyを押すための補助スクリプト
//
// 付ける場所：
// Player本体
//
// 必要な理由：
// CharacterControllerはRigidbodyを自動で気持ちよく押してくれないことがある。
// このスクリプトは、プレイヤーがRigidbodyに当たった時に押す力を加える。
//
// 破片側のRigidbodyが isKinematic = false の時だけ押せる。

using UnityEngine;

public class CharacterPushRigidbodies : MonoBehaviour
{
    [Header("押す力")]
    public float pushPower = 2.5f;

    [Header("上方向へ押さない")]
    public bool ignoreVerticalDirection = true;

    [Header("重い物ほど押しにくくする")]
    public bool reduceByMass = true;

    [Header("最大質量補正")]
    public float maxMassForPush = 8f;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb == null)
            return;

        if (rb.isKinematic)
            return;

        Vector3 pushDir = hit.moveDirection;

        if (ignoreVerticalDirection)
        {
            pushDir.y = 0f;
        }

        if (pushDir.sqrMagnitude <= 0.0001f)
            return;

        pushDir.Normalize();

        float finalPower = pushPower;

        if (reduceByMass)
        {
            float massRate = Mathf.Clamp01(rb.mass / Mathf.Max(0.01f, maxMassForPush));
            finalPower *= 1f - massRate * 0.7f;
        }

        rb.AddForce(pushDir * finalPower, ForceMode.Impulse);
    }
}
