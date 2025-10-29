using UI;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Core
{
    public class SettingsAudioUI : MonoBehaviour
    {
        [Header("Mixer")]
        public AudioMixer mixer;                   // 指向 MainMixer
        [Tooltip("与 AudioMixer 暴露的参数名一致")]
        public string musicParam = "MusicVolume";
        public string sfxParam = "SFXVolume";

        [Header("Music UI")]
        public Button btnMusicYes;                 // 开
        public Button btnMusicNo;                  // 关
        public Slider musicSlider;                 // 0~1（线性），内部换算成 dB

        [Header("SFX UI")]
        public Button btnSfxYes;
        public Button btnSfxNo;
        public Slider sfxSlider;

        [Header("Prefs Keys")]
        public string pfMusicOn = "pf_music_on";     // 1/0
        public string pfMusicVol = "pf_music_vol";    // 0~1
        public string pfSfxOn = "pf_sfx_on";
        public string pfSfxVol = "pf_sfx_vol";

        // 常量：静音时用 -80dB（接近静音）
        const float MUTE_DB = -80f;

        private void Awake()
        {
            // 绑定按钮
            if (btnMusicYes) btnMusicYes.onClick.AddListener(() => SetMusicOn(true));
            if (btnMusicNo) btnMusicNo.onClick.AddListener(() => SetMusicOn(false));
            if (btnSfxYes) btnSfxYes.onClick.AddListener(() => SetSfxOn(true));
            if (btnSfxNo) btnSfxNo.onClick.AddListener(() => SetSfxOn(false));

            // 绑定滑条
            if (musicSlider) musicSlider.onValueChanged.AddListener(OnMusicSlider);
            if (sfxSlider) sfxSlider.onValueChanged.AddListener(OnSfxSlider);
        }

        private void OnEnable()
        {
            // 读配置（默认：开；音量 0.8）
            bool musicOn = PlayerPrefs.GetInt(pfMusicOn, 1) == 1;
            float musicVol = PlayerPrefs.GetFloat(pfMusicVol, 0.8f);
            bool sfxOn = PlayerPrefs.GetInt(pfSfxOn, 1) == 1;
            float sfxVol = PlayerPrefs.GetFloat(pfSfxVol, 0.8f);

            // 刷 UI & Mixer
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
            // 若处于“关闭”，滑动也认为开启
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
            if (yes) yes.interactable = !on;  // 亮起的按钮可改为非交互，形成“选中”视觉
            if (no) no.interactable = on;
            // 如果你有 SpriteSwap 或颜色高亮，这里也可以顺带改色。
        }

        void OnDisable()
        {
            PlayerPrefs.Save();
        }
    }
}
