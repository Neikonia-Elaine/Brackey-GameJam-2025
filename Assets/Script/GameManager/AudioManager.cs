using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource musicSource; // 用来播 BGM
    public AudioSource sfxSource;   // 用来播音效

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 播放背景音乐
    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.loop = true;
        musicSource.Play();
    }

    // 播放音效
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    // 停止背景音乐
    public void StopMusic()
    {
        musicSource.Stop();
    }

    // 停止音效
    public void StopSFX()
    {
        sfxSource.Stop();
    }

    // 如果要调用 AudioCue 播放音效、BGM，可以用下面的方式

    // public class PlayerAttack : MonoBehaviour
    // {
    //     public AudioClip attackClip; // 在 Inspector 拖一个音效资源

    //     public void Attack()
    //     {
    //         // 做攻击逻辑...
        
    //         // 播放音效
    //         AudioManager.Instance.PlaySFX(attackClip);
    //     }
    // }

}
