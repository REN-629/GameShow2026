using UnityEngine;

public class ThrowableSpinSettings : MonoBehaviour
{
    [Header("投擲回転")]
    public bool useCustomSpin = false;

    [Tooltip("ため最大時の回転力")]
    public Vector3 maxAngularVelocity = new Vector3(8f, 14f, 10f);

    [Tooltip("ランダム方向を混ぜる")]
    public bool randomizeDirection = true;

    [Tooltip("ランダムの強さ")]
    public float randomMultiplier = 1f;

    public Vector3 GetAngularVelocity(float chargeRate)
    {
        chargeRate = Mathf.Clamp01(chargeRate);

        Vector3 baseSpin = maxAngularVelocity * chargeRate;

        if (!randomizeDirection)
            return baseSpin;

        Vector3 random = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;

        Vector3 randomized = Vector3.Scale(
            random,
            new Vector3(
                Mathf.Abs(maxAngularVelocity.x),
                Mathf.Abs(maxAngularVelocity.y),
                Mathf.Abs(maxAngularVelocity.z)
            )
        );

        return Vector3.Lerp(baseSpin, randomized * chargeRate, randomMultiplier);
    }
}
