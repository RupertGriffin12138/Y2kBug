using UI;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Core
{
    public class SettingsAudioUI : MonoBehaviour
    {
        [Header("Mixer")]
        public AudioMixer mixer;                   // ָ�� MainMixer
        [Tooltip("�� AudioMixer ��¶�Ĳ�����һ��")]
        public string musicParam = "MusicVolume";
        public string sfxParam = "SFXVolume";

        [Header("Music UI")]
        public Button btnMusicYes;                 // ��
        public Button btnMusicNo;                  // ��
        public Slider musicSlider;                 // 0~1�����ԣ����ڲ������ dB

        [Header("SFX UI")]
        public Button btnSfxYes;
        public Button btnSfxNo;
        public Slider sfxSlider;

        [Header("Prefs Keys")]
        public string pfMusicOn = "pf_music_on";     // 1/0
        public string pfMusicVol = "pf_music_vol";    // 0~1
        public string pfSfxOn = "pf_sfx_on";
        public string pfSfxVol = "pf_sfx_vol";

        // ����������ʱ�� -80dB���ӽ�������
        const float MUTE_DB = -80f;

        private void Awake()
        {
            // �󶨰�ť
            if (btnMusicYes) btnMusicYes.onClick.AddListener(() => SetMusicOn(true));
            if (btnMusicNo) btnMusicNo.onClick.AddListener(() => SetMusicOn(false));
            if (btnSfxYes) btnSfxYes.onClick.AddListener(() => SetSfxOn(true));
            if (btnSfxNo) btnSfxNo.onClick.AddListener(() => SetSfxOn(false));

            // �󶨻���
            if (musicSlider) musicSlider.onValueChanged.AddListener(OnMusicSlider);
            if (sfxSlider) sfxSlider.onValueChanged.AddListener(OnSfxSlider);
        }

        private void OnEnable()
        {
            // �����ã�Ĭ�ϣ��������� 0.8��
            bool musicOn = PlayerPrefs.GetInt(pfMusicOn, 1) == 1;
            float musicVol = PlayerPrefs.GetFloat(pfMusicVol, 0.8f);
            bool sfxOn = PlayerPrefs.GetInt(pfSfxOn, 1) == 1;
            float sfxVol = PlayerPrefs.GetFloat(pfSfxVol, 0.8f);

            // ˢ UI & Mixer
            if (musicSlider) musicSlider.SetValueWithoutNotify(Mathf.Clamp01(musicVol));
            if (sfxSlider) sfxSlider.SetValueWithoutNotify(Mathf.Clamp01(sfxVol));

            ApplyMusicVolume(musicOn ? musicVol : 0f);
            ApplySfxVolume(sfxOn ? sfxVol : 0f);

            SetButtonsVisual(btnMusicYes, btnMusicNo, musicOn);
            SetButtonsVisual(btnSfxYes, btnSfxNo, sfxOn);

            if (musicSlider) musicSlider.interactable = musicOn;
            if (sfxSlider) sfxSlider.interactable = sfxOn;
        }

        // ====== Music ======
        void SetMusicOn(bool on)
        {
            PlayerPrefs.SetInt(pfMusicOn, on ? 1 : 0);
            if (musicSlider)
            {
                musicSlider.interactable = on;
                float vol = on ? (PlayerPrefs.GetFloat(pfMusicVol, 0.8f)) : 0f;
                ApplyMusicVolume(vol);
                if (on && vol <= 0.0001f) { vol = 0.8f; musicSlider.SetValueWithoutNotify(vol); ApplyMusicVolume(vol); }
            }
            SetButtonsVisual(btnMusicYes, btnMusicNo, on);

            AudioSettingsBus.Broadcast();
        }

        void OnMusicSlider(float v)
        {
            v = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat(pfMusicVol, v);
            // �����ڡ��رա�������Ҳ��Ϊ����
            if (PlayerPrefs.GetInt(pfMusicOn, 1) == 0 && v > 0f)
                PlayerPrefs.SetInt(pfMusicOn, 1);
            SetButtonsVisual(btnMusicYes, btnMusicNo, v > 0f);
            if (musicSlider) musicSlider.interactable = v > 0f;
            ApplyMusicVolume(v);

            AudioSettingsBus.Broadcast();
        }

        void ApplyMusicVolume(float linear01)
        {
            if (!mixer) return;
            if (linear01 <= 0.0001f) mixer.SetFloat(musicParam, MUTE_DB);
            else mixer.SetFloat(musicParam, Lin01ToDb(linear01));
        }

        // ====== SFX ======
        void SetSfxOn(bool on)
        {
            PlayerPrefs.SetInt(pfSfxOn, on ? 1 : 0);
            if (sfxSlider)
            {
                sfxSlider.interactable = on;
                float vol = on ? (PlayerPrefs.GetFloat(pfSfxVol, 0.8f)) : 0f;
                ApplySfxVolume(vol);
                if (on && vol <= 0.0001f) { vol = 0.8f; sfxSlider.SetValueWithoutNotify(vol); ApplySfxVolume(vol); }
            }
            SetButtonsVisual(btnSfxYes, btnSfxNo, on);
        }

        void OnSfxSlider(float v)
        {
            v = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat(pfSfxVol, v);
            if (PlayerPrefs.GetInt(pfSfxOn, 1) == 0 && v > 0f)
                PlayerPrefs.SetInt(pfSfxOn, 1);
            SetButtonsVisual(btnSfxYes, btnSfxNo, v > 0f);
            if (sfxSlider) sfxSlider.interactable = v > 0f;
            ApplySfxVolume(v);
        }

        void ApplySfxVolume(float linear01)
        {
            if (!mixer) return;
            if (linear01 <= 0.0001f) mixer.SetFloat(sfxParam, MUTE_DB);
            else mixer.SetFloat(sfxParam, Lin01ToDb(linear01));
        }

        // ====== Helpers ======
        static float Lin01ToDb(float v) => Mathf.Log10(Mathf.Clamp(v, 0.0001f, 1f)) * 20f;

        void SetButtonsVisual(Button yes, Button no, bool on)
        {
            if (yes) yes.interactable = !on;  // ����İ�ť�ɸ�Ϊ�ǽ������γɡ�ѡ�С��Ӿ�
            if (no) no.interactable = on;
            // ������� SpriteSwap ����ɫ����������Ҳ����˳����ɫ��
        }

        void OnDisable()
        {
            PlayerPrefs.Save();
        }
    }
}
