using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    private static bool _isQuitting;

    public static AudioManager Instance
    {
        get
        {
            if (_isQuitting) return null;

            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                }
            }

            return _instance;
        }
    }

    [Header("基础设置")]
    public int maxSFX = 16;
    private List<AudioSource> _sfxPool = new List<AudioSource>();

    [Header("音量")]
    public float bgmVolume = 1;
    public float sfxVolume = 1;
    public float uiVolume = 1;
    public float voiceVolume = 1;

    private AudioSource _bgmSource;
    private bool _isInitialized;

    // 路径固定：Resources/Audio/...
    private const string AUDIO_PATH_PREFIX = "Audio/";

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        DontDestroyOnLoad(gameObject);
        InitializeIfNeeded();
        AudioSettingsData.ApplyTo(this);
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    void InitializeIfNeeded()
    {
        if (_isInitialized) return;

        CreateBGMSource();
        CreateSFXPool();
        _isInitialized = true;
    }

    // --------------------
    // 创建 BGM 喇叭
    // --------------------
    void CreateBGMSource()
    {
        if (this == null) return;
        if (_bgmSource != null) return;

        GameObject go = new GameObject("BGM_AudioSource");
        go.transform.SetParent(transform, false);
        _bgmSource = go.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
    }

    // --------------------
    // 创建 SFX 对象池
    // --------------------
    void CreateSFXPool()
    {
        _sfxPool.Clear();
        for (int i = 0; i < Mathf.Max(1, maxSFX); i++)
        {
            _sfxPool.Add(CreateSFXSource(i));
        }
    }

    AudioSource CreateSFXSource(int index)
    {
        if (this == null) return null;

        GameObject go = new GameObject($"SFX_{index}");
        go.transform.SetParent(transform, false);
        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        return source;
    }

    void EnsureSFXPool()
    {
        if (_isQuitting) return;
        InitializeIfNeeded();

        int targetCount = Mathf.Max(1, maxSFX);

        if (_sfxPool == null)
        {
            _sfxPool = new List<AudioSource>(targetCount);
        }

        for (int i = 0; i < _sfxPool.Count; i++)
        {
            if (_sfxPool[i] == null)
            {
                _sfxPool[i] = CreateSFXSource(i);
            }
        }

        for (int i = _sfxPool.Count; i < targetCount; i++)
        {
            _sfxPool.Add(CreateSFXSource(i));
        }
    }

    // --------------------
    // 获取空闲喇叭
    // --------------------
    AudioSource GetFreeSource()
    {
        EnsureSFXPool();
        if (_sfxPool == null || _sfxPool.Count == 0) return null;

        foreach (var s in _sfxPool)
        {
            if (s != null && !s.isPlaying) return s;
        }

        return _sfxPool[0];
    }

    // --------------------
    // 【核心】通过名字加载音频
    // --------------------
    AudioClip LoadAudioByName(string audioName)
    {
        if (ResourceManager.Instance == null) return null;
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
        if (source == null) return;
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
        if (source == null) return;
        source.clip = clip;
        source.volume = GetVolume(track);
        source.spatialBlend = 1; // 1 表示纯 3D 音效
    
        // --- 建议添加的 3D 衰减设置 ---
        source.rolloffMode = AudioRolloffMode.Logarithmic; // 对数衰减
        source.minDistance = 2f;                           // 2米内声音最大
        source.maxDistance = 15f;                          // 超过15米就听不见了
        // ----------------------------

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
        InitializeIfNeeded();
        if (_bgmSource == null) return;

        if (_bgmSource.isPlaying && _bgmSource.clip == clip)
        {
            _bgmSource.volume = bgmVolume;
            return;
        }

        _bgmSource.clip = clip;
        _bgmSource.volume = bgmVolume;
        _bgmSource.Play();
    }

    public void StopBGM()
    {
        if (_bgmSource != null)
        {
            _bgmSource.Stop();
        }
    }

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

    public void SetBgmVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);
        if (_bgmSource != null)
        {
            _bgmSource.volume = bgmVolume;
        }
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        RefreshActiveSourceVolumes(AudioTrack.SFX);
    }

    public void SetUiVolume(float value)
    {
        uiVolume = Mathf.Clamp01(value);
        RefreshActiveSourceVolumes(AudioTrack.UI);
    }

    private void RefreshActiveSourceVolumes(AudioTrack track)
    {
        float targetVolume = GetVolume(track);
        for (int i = 0; i < _sfxPool.Count; i++)
        {
            AudioSource source = _sfxPool[i];
            if (source == null || !source.isPlaying)
            {
                continue;
            }

            source.volume = targetVolume;
        }
    }
}
