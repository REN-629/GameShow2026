// 衝突音：衝突速度に応じて弱/中/強のSEをランダム再生する
// 床に置いた程度の弱い衝突でも鳴らせる
using UnityEngine;

public class ImpactSoundEmitter : MonoBehaviour
{
    [Header("弱い衝突SE 複数登録可")]
    public AudioClip[] weakImpactSEClips;

    [Header("中くらいの衝突SE 複数登録可")]
    public AudioClip[] mediumImpactSEClips;

    [Header("強い衝突SE 複数登録可")]
    public AudioClip[] strongImpactSEClips;

    [Header("音量")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("速度しきい値")]
    public float weakSpeed = 0.3f;
    public float mediumSpeed = 3f;
    public float strongSpeed = 7f;

    [Header("連続再生防止")]
    public float soundCooldown = 0.12f;

    [Header("手に持ってる間は衝突音を鳴らさない")]
    public bool muteWhileHeld = true;

    private PickupItem pickupItem;
    private float lastPlayTime = -999f;

    void Awake()
    {
        pickupItem = GetComponentInParent<PickupItem>();
    }

    void OnCollisionEnter(Collision collision)
    {
        TryPlayImpactSound(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        // 低速で置いた時、Enterが弱すぎて聞こえない場合の保険。
        // クールダウンがあるので鳴り続けすぎるのを防ぐ。
        TryPlayImpactSound(collision);
    }

    void TryPlayImpactSound(Collision collision)
    {
        if (Time.time - lastPlayTime < soundCooldown)
            return;

        if (muteWhileHeld && pickupItem != null && pickupItem.IsHeld)
            return;

        float speed = collision.relativeVelocity.magnitude;

        if (speed < weakSpeed)
            return;

        AudioClip[] clips = weakImpactSEClips;

        if (speed >= strongSpeed)
        {
            clips = strongImpactSEClips;
        }
        else if (speed >= mediumSpeed)
        {
            clips = mediumImpactSEClips;
        }

        Vector3 playPos = collision.contactCount > 0
            ? collision.GetContact(0).point
            : transform.position;

        RandomAudioPlayer.PlayRandom(
            clips,
            playPos,
            volume
        );

        lastPlayTime = Time.time;
    }
}
