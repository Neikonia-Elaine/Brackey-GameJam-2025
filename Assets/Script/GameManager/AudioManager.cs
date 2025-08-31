using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sources")]
    public AudioSource musicSource; // BGM
    public AudioSource sfxSource;   // SFX

    [Header("People SFX (4 clips)")]
    public AudioClip[] peopleSFX = new AudioClip[4];

    [Header("Car SFX (3 clips)")]
    public AudioClip[] carSFX = new AudioClip[3];

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        // DontDestroyOnLoad(gameObject);

        // 兜底，防止忘拖 AudioSource
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }
    }

    // —— 你原有的方法（可保留/合并） ——
    public void PlayMusic(AudioClip clip, float volume = 0.2f)
    {
        if (clip == null || musicSource == null) return;
        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    public void StopSFX()
    {
        if (sfxSource != null) sfxSource.Stop();
    }

    // —— 新增：随机播 People 组（4个） ——
    public void PlayRandomPeopleSFX(float volume = 1f)
    {
        var clip = PickRandom(peopleSFX);
        if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip, volume);
    }

    // —— 新增：随机播 Car 组（3个） ——
    public void PlayRandomCarSFX(float volume = 1f)
    {
        var clip = PickRandom(carSFX);
        if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip, volume);
    }

    // 从给定数组中随机取一个非空 clip（最简单实现）
    private static AudioClip PickRandom(AudioClip[] set)
    {
        if (set == null || set.Length == 0) return null;

        // 统计非空数量
        int count = 0;
        for (int i = 0; i < set.Length; i++)
            if (set[i] != null) count++;

        if (count == 0) return null;

        // 在非空项里随机
        int k = Random.Range(0, count);
        for (int i = 0; i < set.Length; i++)
        {
            if (set[i] == null) continue;
            if (k == 0) return set[i];
            k--;
        }
        return null;
    }
}
