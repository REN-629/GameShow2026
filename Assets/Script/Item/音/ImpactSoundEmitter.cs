using UnityEngine;

public class ImpactSoundEmitter : MonoBehaviour
{
    [Header("弱い衝突SE")]
    public AudioClip[] weakImpactSEClips;

    [Header("中くらいの衝突SE")]
    public AudioClip[] mediumImpactSEClips;

    [Header("強い衝突SE")]
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

    [Header("手に持ってる間は鳴らさない")]
    public bool muteWhileHeld = true;

    [Header("常駐AudioSource再生")]
    public bool useItemSEPlayer = true;
    public ItemSEPlayer itemSEPlayer;

    [Header("ItemSEPlayerが無い時だけ従来方式")]
    public bool fallbackToRandomAudioPlayer = true;

    private PickupItem pickupItem;
    private float lastPlayTime = -999f;

    void Awake()
    {
        pickupItem = GetComponent<PickupItem>();

        if (itemSEPlayer == null)
            itemSEPlayer = GetComponent<ItemSEPlayer>();
    }

    void OnCollisionEnter(Collision collision)
    {
        TryPlayImpactSound(collision);
    }

    void OnCollisionStay(Collision collision)
    {
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

        AudioClip[] clips = GetClipsBySpeed(speed);

        if (clips == null || clips.Length == 0)
            return;

        Vector3 playPos =
            collision.contactCount > 0
            ? collision.GetContact(0).point
            : transform.position;

        bool played = false;

        if (useItemSEPlayer)
        {
            if (itemSEPlayer == null)
                itemSEPlayer = GetComponent<ItemSEPlayer>();

            if (itemSEPlayer != null)
            {
                itemSEPlayer.PlayRandom(clips, volume);
                played = true;
            }
        }

        if (!played && fallbackToRandomAudioPlayer)
        {
            RandomAudioPlayer.PlayRandom(clips, playPos, volume);
            played = true;
        }

        if (played)
            lastPlayTime = Time.time;
    }

    AudioClip[] GetClipsBySpeed(float speed)
    {
        if (speed >= strongSpeed)
            return strongImpactSEClips;

        if (speed >= mediumSpeed)
            return mediumImpactSEClips;

        return weakImpactSEClips;
    }
}
