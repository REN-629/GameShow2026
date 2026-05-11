// RandomAudioPlayer：複数の効果音からランダムに1つ鳴らす便利クラス
// どこかのGameObjectに付ける必要はない
using UnityEngine;

public static class RandomAudioPlayer
{
    public static void PlayRandom(AudioClip[] clips, Vector3 position, float volume = 1f)
    {
        if (clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        if (clip == null)
            return;

        AudioSource.PlayClipAtPoint(clip, position, volume);
    }
}
