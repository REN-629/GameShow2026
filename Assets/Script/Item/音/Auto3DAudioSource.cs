using UnityEngine;

[DisallowMultipleComponent]
public class Auto3DAudioSource : MonoBehaviour
{
    [Header("3D音響設定")]
    [Range(0f, 1f)]
    public float spatialBlend = 1f;

    public float minDistance = 1f;
    public float maxDistance = 20f;

    [Range(0f, 5f)]
    public float dopplerLevel = 0.5f;

    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

    [Header("子オブジェクトも対象")]
    public bool applyToChildren = true;

    void Awake()
    {
        Apply();
    }

    void OnValidate()
    {
        Apply();
    }

    public void Apply()
    {
        AudioSource[] sources =
            applyToChildren
            ? GetComponentsInChildren<AudioSource>(true)
            : GetComponents<AudioSource>();

        foreach (AudioSource source in sources)
        {
            if (source == null)
                continue;

            source.spatialBlend = spatialBlend;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.dopplerLevel = dopplerLevel;
            source.rolloffMode = rolloffMode;
        }
    }
}
