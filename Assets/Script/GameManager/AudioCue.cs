using UnityEngine;

[CreateAssetMenu(menuName = "Audio/AudioCue", fileName = "NewAudioCue")]
public class AudioCue : ScriptableObject
{
    public AudioClip[] clips;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(-3f, 3f)] public float pitchMin = 1f, pitchMax = 1f;
    [Range(0f, 1f)] public float spatialBlend = 1f; // 0=2D,1=3D
    public float maxDistance = 25f;                 // 3D 衰减距离
    public int maxSimultaneous = 4;                 // 同时最多几个实例
    public float cooldown = 0f;                     // 同一 Cue 播放间隔限制（秒）

    public AudioClip RandomClip() =>
        (clips == null || clips.Length == 0) ? null : clips[Random.Range(0, clips.Length)];

    public float RandomPitch() => Random.Range(pitchMin, pitchMax);
}
