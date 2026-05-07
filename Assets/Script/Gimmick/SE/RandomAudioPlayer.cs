// ランダム効果音再生：複数AudioClipからランダムに1つ選んで鳴らす共通処理
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
