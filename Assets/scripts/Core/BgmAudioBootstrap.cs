using UnityEngine;
using UnityEngine.Audio;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
[DefaultExecutionOrder(-100)]
public class BgmAudioBootstrap : MonoBehaviour
{
    [Header("Mixer Routing")]
    public AudioMixer mixer;
    public AudioMixerGroup musicGroup;
    public string musicParam = "MusicVolume";
    public string sfxParam = "SFXVolume";

    [Header("Prefs Keys")]
    public string pfMusicOn = "pf_music_on";
    public string pfMusicVol = "pf_music_vol";
    public string pfSfxOn = "pf_sfx_on";
    public string pfSfxVol = "pf_sfx_vol";

    [Header("BGM")]
    public AudioClip bgmClip;
    public bool playOnStart = true;
    public bool loop = true;
    public bool ignoreListenerPause = true;

    const float MUTE_DB = -80f;
    AudioSource _src;

    void Awake()
    {
        _src = GetComponent<AudioSource>();
        _src.playOnAwake = false;
        _src.loop = loop;
        _src.ignoreListenerPause = ignoreListenerPause;
        _src.volume = 1f; // 让最终响度只由 Mixer 决定

        if (musicGroup) _src.outputAudioMixerGroup = musicGroup;
        if (bgmClip && !_src.clip) _src.clip = bgmClip;

        ApplyAllFromPrefs();
        EnsurePlaybackAccordingToPrefs();
    }

    // ✅ 关键：在 Start 再应用一次，覆盖其它脚本在 Start 阶段的改动
    void Start()
    {
        ApplyAllFromPrefs();
        EnsurePlaybackAccordingToPrefs();
        // ✅ 关键：再等一帧（确保所有初始化完成后）再应用一次
        StartCoroutine(ReapplyEndOfFrame());
    }

    System.Collections.IEnumerator ReapplyEndOfFrame()
    {
        yield return null; // 等到首帧末尾
        ApplyAllFromPrefs();
        EnsurePlaybackAccordingToPrefs();
    }

    void OnEnable()
    {
        AudioSettingsBus.OnAudioSettingsChanged += OnAudioSettingsChanged;
    }

    void OnDisable()
    {
        AudioSettingsBus.OnAudioSettingsChanged -= OnAudioSettingsChanged;
    }

    void OnAudioSettingsChanged()
    {
        ApplyAllFromPrefs();
        EnsurePlaybackAccordingToPrefs();
    }

    // ===== Core =====
    void ApplyAllFromPrefs()
    {
        if (!mixer) return;

        bool musicOn = PlayerPrefs.GetInt(pfMusicOn, 1) == 1;
        float musicV = Mathf.Clamp01(PlayerPrefs.GetFloat(pfMusicVol, 0.8f));
        bool sfxOn = PlayerPrefs.GetInt(pfSfxOn, 1) == 1;
        float sfxV = Mathf.Clamp01(PlayerPrefs.GetFloat(pfSfxVol, 0.8f));

        SetMixerLinear01(mixer, musicParam, musicOn ? musicV : 0f);
        SetMixerLinear01(mixer, sfxParam, sfxOn ? sfxV : 0f);

        // 兜底：若还未路由，自动指到 Music 组
        if (_src.outputAudioMixerGroup == null && musicGroup)
            _src.outputAudioMixerGroup = musicGroup;
    }

    void EnsurePlaybackAccordingToPrefs()
    {
        bool musicOn = PlayerPrefs.GetInt(pfMusicOn, 1) == 1;
        float musicV = PlayerPrefs.GetFloat(pfMusicVol, 0.8f);
        bool shouldPlay = playOnStart && musicOn && musicV > 0.0001f;

        if (shouldPlay)
        {
            if (_src.clip && !_src.isPlaying) _src.Play();
        }
        else
        {
            if (_src.isPlaying) _src.Stop();
        }
    }

    static void SetMixerLinear01(AudioMixer m, string param, float linear01)
    {
        if (!m || string.IsNullOrEmpty(param)) return;
        float db = (linear01 <= 0.0001f) ? MUTE_DB
                 : Mathf.Log10(Mathf.Clamp(linear01, 0.0001f, 1f)) * 20f;
        m.SetFloat(param, db);
    }
}
