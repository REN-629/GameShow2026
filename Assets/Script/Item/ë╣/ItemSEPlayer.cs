using UnityEngine;

[DisallowMultipleComponent]
public class ItemSEPlayer : MonoBehaviour
{
    [Header("AudioSource")]
    public AudioSource audioSource;

    [Header("スイング")]
    public AudioClip[] swingClips;
    [Range(0f, 1f)]
    public float swingVolume = 1f;

    [Header("ヒット")]
    public AudioClip[] hitClips;
    [Range(0f, 1f)]
    public float hitVolume = 1f;

    [Header("耐久値減少")]
    public AudioClip[] durabilityDamageClips;
    [Range(0f, 1f)]
    public float durabilityDamageVolume = 1f;

    [Header("破壊")]
    public AudioClip[] breakClips;
    [Range(0f, 1f)]
    public float breakVolume = 1f;

    [Header("3D音響")]
    public bool force3D = true;

    [Range(0f, 1f)]
    public float spatialBlend = 1f;

    public float minDistance = 1f;
    public float maxDistance = 20f;

    [Range(0f, 5f)]
    public float dopplerLevel = 0.5f;

    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

    void Awake()
    {
        SetupAudioSource();
    }

    void OnValidate()
    {
        SetupAudioSource();
    }

    void SetupAudioSource()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            return;

        audioSource.playOnAwake = false;

        if (!force3D)
            return;

        audioSource.spatialBlend = spatialBlend;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.dopplerLevel = dopplerLevel;
        audioSource.rolloffMode = rolloffMode;
    }

    public void PlaySwing()
    {
        PlayRandom(swingClips, swingVolume);
    }

    public void PlayHit()
    {
        PlayRandom(hitClips, hitVolume);
    }

    public void PlayDurabilityDamage()
    {
        PlayRandom(durabilityDamageClips, durabilityDamageVolume);
    }

    public void PlayBreak()
    {
        PlayRandom(breakClips, breakVolume);
    }

    public void PlayRandom(AudioClip[] clips, float volume)
    {
        if (audioSource == null)
            SetupAudioSource();

        if (audioSource == null)
            return;

        if (clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        if (clip == null)
            return;

        audioSource.PlayOneShot(clip, volume);
    }
}
