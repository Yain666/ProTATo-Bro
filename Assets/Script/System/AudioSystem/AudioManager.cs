using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
     public static AudioManager Instance { get; private set; }

    [Header("基础设置")]
    public int maxSFX = 16;
    private List<AudioSource> _sfxPool = new List<AudioSource>();

    [Header("音量")]
    public float bgmVolume = 1;
    public float sfxVolume = 1;
    public float uiVolume = 1;
    public float voiceVolume = 1;

    private AudioSource _bgmSource;

    // 路径固定：Resources/Audio/...
    private const string AUDIO_PATH_PREFIX = "Audio/";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);
        CreateBGMSource();
        CreateSFXPool();
    }

    // --------------------
    // 创建 BGM 喇叭
    // --------------------
    void CreateBGMSource()
    {
        GameObject go = new GameObject("BGM_AudioSource");
        go.transform.parent = transform;
        _bgmSource = go.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
    }

    // --------------------
    // 创建 SFX 对象池
    // --------------------
    void CreateSFXPool()
    {
        for (int i = 0; i < maxSFX; i++)
        {
            GameObject go = new GameObject($"SFX_{i}");
            go.transform.parent = transform;
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            _sfxPool.Add(source);
        }
    }

    // --------------------
    // 获取空闲喇叭
    // --------------------
    AudioSource GetFreeSource()
    {
        foreach (var s in _sfxPool)
        {
            if (!s.isPlaying) return s;
        }
        return _sfxPool[0];
    }

    // --------------------
    // 【核心】通过名字加载音频
    // --------------------
    AudioClip LoadAudioByName(string audioName)
    {
        string path = AUDIO_PATH_PREFIX + audioName;
        return ResourceManager.Instance.GetAudio(path);
    }

    // --------------------
    // 播放 2D 音效（UI/SFX）
    // --------------------
    public void Play(string audioName, AudioTrack track = AudioTrack.SFX)
    {
        AudioClip clip = LoadAudioByName(audioName);
        if (clip == null) return;

        AudioSource source = GetFreeSource();
        source.clip = clip;
        source.volume = GetVolume(track);
        source.loop = false;
        source.spatialBlend = 0;
        source.Play();
    }

    // --------------------
    // 播放 3D 音效
    // --------------------
    public void Play3D(string audioName, Vector3 worldPos, AudioTrack track = AudioTrack.SFX)
    {
        AudioClip clip = LoadAudioByName(audioName);
        if (clip == null) return;

        AudioSource source = GetFreeSource();
        source.clip = clip;
        source.volume = GetVolume(track);
        source.spatialBlend = 1;
        source.transform.position = worldPos;
        source.loop = false;
        source.Play();
    }

    // --------------------
    // 播放 BGM
    // --------------------
    public void PlayBGM(string audioName)
    {
        AudioClip clip = LoadAudioByName(audioName);
        if (clip == null) return;

        _bgmSource.clip = clip;
        _bgmSource.volume = bgmVolume;
        _bgmSource.Play();
    }

    public void StopBGM() => _bgmSource.Stop();

    // --------------------
    // 获取轨道音量
    // --------------------
    float GetVolume(AudioTrack track)
    {
        return track switch
        {
            AudioTrack.BGM => bgmVolume,
            AudioTrack.SFX => sfxVolume,
            AudioTrack.UI => uiVolume,
            AudioTrack.Voice => voiceVolume,
            _ => 1
        };
    }
}
