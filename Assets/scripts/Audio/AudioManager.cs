using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoSingleton<AudioManager>
{

    public AudioManagerModel Model { get; private set; }

    private void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //    AudioClipHelper.Instance.PlayMouseClick();

        if (AudioClipHelper.Instance != null)
        {
            if (Input.GetKeyDown(KeyCode.H))
                AudioClipHelper.Instance.Play_UIHover();

            if (Input.GetKeyDown(KeyCode.J))
                AudioClipHelper.Instance.Play_UIClick();
        }
        // 如果为 null，可以输出一个警告日志以便调试
        else
        {
            Debug.LogWarning("AudioClipHelper Instance is not available.");
        }

    }

    public void StopBGM()
    {
        _bgmSource.Stop();
    }

    public void ContinueBGM()
    {
        _bgmSource.Play();
    }

    public void CloseBGM()
    {
        _bgmSource.Stop();
        _bgmSource.clip = null;
    }

    public void PlayBGM(AudioClip clip, float volume = 1.0f)
    {
        if (!_bgmSource.isPlaying && Model.IsBgmOpen)
        {
            _bgmSource.Play();
        }

        if (_bgmSource.clip == clip)
            return;

        if (clip != null)
        {
            _bgmSource.clip = clip;
            _bgmSource.loop = true;
            _bgmSource.volume = volume;
            _bgmSource.enabled = Model.IsBgmOpen;
            if (Model.IsBgmOpen)
                _bgmSource.Play();
        }
    }

    public void PlaySoundEffect(AudioClip clip, float duration = 2.5f)
    {
        if (clip != null /**&& Model.IsSoundOpen**/)
        {
            GameObject sourceObj = new GameObject(clip.name);
            AudioSource audioSource = sourceObj.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.volume = 1f;
            audioSource.pitch = 1f;
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("clip  is not available.");
        }
    }

    public void PlayLoopSound(AudioClip clip)
    {
        if (clip != null && Model.IsSoundOpen)
        {
            _loopSoundSource.clip = clip;
            _loopSoundSource.loop = true;
            _loopSoundSource.Play();
        }
    }

    public void StopLoopSound()
    {
        _loopSoundSource.Stop();
    }

    //protected override void HandleAwake()
    //{
    //    base.HandleAwake();
    //}

    private void OnEnable()
    {
        //Model = new AudioManagerModel();
        //_bgmSource.enabled = Model.IsBgmOpen;
        //Model.OnBGMSwtich += OnBGMSwtich;
    }

    private void OnDisable()
    {
        //Model.OnBGMSwtich -= OnBGMSwtich;
    }

    private void OnBGMSwtich(bool isBGMOpen)
    {
        _bgmSource.enabled = isBGMOpen;
    }

    [SerializeField] private AudioSource _bgmSource = null;
    [SerializeField] private AudioSource _loopSoundSource = null;
}
