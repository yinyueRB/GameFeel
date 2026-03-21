using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("播放器组件 (Audio Sources)")]
    public AudioSource bgmSource; // 专门放背景音乐
    public AudioSource sfxSource; // 专门放音效

    void Awake()
    {
        // 经典的单例模式，确保全游戏只有一个 AudioManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切场景时不要销毁它
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 播放背景音乐的接口
    public void PlayBGM(AudioClip bgmClip)
    {
        if (bgmSource.clip == bgmClip) return; // 如果已经在放这首歌了，就不管
        bgmSource.clip = bgmClip;
        bgmSource.loop = true; // BGM 必须循环
        bgmSource.Play();
    }

    // 播放单次音效的接口 (核心：PlayOneShot 允许多个音效叠加播放)
    public void PlaySFX(AudioClip clip, float pitch = 1f)
    {
        if (clip != null)
        {
            sfxSource.pitch = pitch; // 允许改变音调
            sfxSource.PlayOneShot(clip); // PlayOneShot 不会打断正在播放的其他音效！
        }
    }
}