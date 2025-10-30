using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    public class AudioManager : MonoSingleton<AudioManager>
    {

        public AudioManagerModel Model { get; private set; }

        public List<GameObject> soundEffect;
        private Dictionary<string, GameObject> audioSources;

        private int nowAudio = 1;

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

        public void PlaySoundEffect(AudioClip clip,bool loop)
        {
            if (clip != null /**&& Model.IsSoundOpen**/)
            {
                if (!audioSources.ContainsKey(clip.name))
                {
                    CreatAudioObj(clip);
                }

                bool canPlayAudio = PlayerPrefs.GetInt("pf_sfx_on", 1) == 1;
                if (canPlayAudio)
                    audioSources[clip.name].GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("pf_sfx_vol", 1f);
                else
                    audioSources[clip.name].GetComponent<AudioSource>().volume = 0;

                audioSources[clip.name].GetComponent<AudioSource>().loop = loop;
                audioSources[clip.name].GetComponent<AudioSource>().Play();

            }
            else
            {
                Debug.LogWarning("clip  is not available.");
            }
        }
        public void PlaySoundEffect_Board(AudioClip clip, bool loop,int id)
        {
            nowAudio = id;
            if (clip != null /**&& Model.IsSoundOpen**/)
            {
                if (!audioSources.ContainsKey(clip.name))
                {
                    CreatAudioObj(clip);
                }

                bool canPlayAudio = PlayerPrefs.GetInt("pf_sfx_on", 1) == 1;
                if (canPlayAudio)
                    audioSources[clip.name].GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("pf_sfx_vol", 1f);
                else
                    audioSources[clip.name].GetComponent<AudioSource>().volume = 0;

                audioSources[clip.name].GetComponent<AudioSource>().loop = loop;
                audioSources[clip.name].GetComponent<AudioSource>().Play();

            }
            else
            {
                Debug.LogWarning("clip  is not available.");
            }
        }
        public void StopLatelyAudio()
        {
            switch (nowAudio)
            {
                case 1:
                    AudioClipHelper.Instance.Stop_Mouse1();
                    break;
                case 2:
                    AudioClipHelper.Instance.Stop_Mouse2();
                    break;
                case 3:
                    AudioClipHelper.Instance.Stop_Eyes1();
                    break;
                case 4:
                    AudioClipHelper.Instance.Stop_hreat1();
                    break;
                case 5:
                    AudioClipHelper.Instance.Stop_MutiImage();
                    break;
            }
        }
        public void StopSoundEffect(AudioClip clip)
        {
            if (clip != null /**&& Model.IsSoundOpen**/)
            {
                if (!audioSources.ContainsKey(clip.name))
                {
                    //CreatAudioObj(clip);
                }

                audioSources[clip.name].GetComponent<AudioSource>().Stop();
            }
            else
            {
                Debug.LogWarning("clip  is not available.");
            }
        }
        public void StopAllAudio()
        {
            for (int i = 0; i < soundEffect.Count; i++)
            {
                if (soundEffect[i].GetComponent<AudioSource>().isPlaying)
                {
                    soundEffect[i].GetComponent<AudioSource>().Stop();
                }
            }
        }

        private void CreatAudioObj(AudioClip clip)
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
}
