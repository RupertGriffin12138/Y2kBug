using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoSingleton<AudioManager>
{

    public AudioManagerModel Model { get; private set; }

    public List<GameObject> soundEffect;
    private Dictionary<string, GameObject> audioSources;

    protected override void OnStart()
    {
        base.OnStart();
        soundEffect.Clear();
        audioSources = new Dictionary<string, GameObject>();

    }
    private void Update()
    {

        if (AudioClipHelper.Instance != null)
        {
            if (Input.GetKeyDown(KeyCode.H))
                AudioClipHelper.Instance.Play_UIHover();

            if (Input.GetKeyDown(KeyCode.J))
                AudioClipHelper.Instance.Play_UIClick();

            if (Input.GetKeyDown(KeyCode.K))
                AudioClipHelper.Instance.Play_SuanPan();

            if (Input.GetKeyDown(KeyCode.L))
                AudioClipHelper.Instance.Play_IronCabinet();
        }

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
            if (!audioSources.ContainsKey(clip.name))
            {
                CreatAudioObj(clip);
            }

            audioSources[clip.name].GetComponent<AudioSource>().Play();

        }
        else
        {
            Debug.LogWarning("clip  is not available.");
        }
    }

    void CreatAudioObj(AudioClip clip)
    {
        GameObject sourceObj = new GameObject(clip.name);
        sourceObj.transform.SetParent(this.transform);
        AudioSource audioSource = sourceObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = 1f;
        audioSource.pitch = 1f;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        soundEffect.Add(sourceObj);
        audioSources[clip.name] = sourceObj;
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


    private void OnBGMSwtich(bool isBGMOpen)
    {
        _bgmSource.enabled = isBGMOpen;
    }

    [SerializeField] private AudioSource _bgmSource = null;
    [SerializeField] private AudioSource _loopSoundSource = null;
}
