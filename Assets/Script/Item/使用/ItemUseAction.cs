// ItemUseAction：アイテム使用処理の親クラス
//
// バールなら BluntUseAction
// ライターなら FireUseAction
// みたいに、これを継承して増やす。

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

    // true = 使用が有効だった。道具耐久を減らす。
    // false = 空振りや無効。耐久を減らさない。
    public abstract bool Use(PickupItem item);
}
