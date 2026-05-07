// アイテム使用処理の基底クラス
// 使用SEを3種類など複数登録し、使用時にランダム再生する
using UnityEngine;

public abstract class ItemUseAction : MonoBehaviour
{
    [Header("使用SE 複数登録可")]
    public AudioClip[] useSEClips;

    [Header("使用SE音量")]
    [Range(0f, 1f)]
    public float useSEVolume = 1f;

    protected void PlayUseSE()
    {
        RandomAudioPlayer.PlayRandom(
            useSEClips,
            transform.position,
            useSEVolume
        );
    }

    public abstract bool Use(PickupItem item);
}
