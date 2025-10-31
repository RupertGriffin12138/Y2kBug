using UnityEngine;

namespace UI
{
    /// <summary>
    /// 全局全屏控制器：
    /// 按 F11 切换全屏/窗口模式，
    /// 全屏固定为 1600x1200，
    /// 所有场景有效且无重复实例。
    /// </summary>
    public class GlobalFullscreenToggle : MonoBehaviour
    {
        private static GlobalFullscreenToggle _instance;
        private bool _isFullscreen;

        // 固定分辨率
        private const int TargetWidth = 1600;
        private const int TargetHeight = 1200;

        private void Awake()
        {
            // 防止重复
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // 初始化
            _isFullscreen = PlayerPrefs.GetInt("fullscreen", 1) == 1;

            ApplyResolution(_isFullscreen);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                _isFullscreen = !_isFullscreen;
                ApplyResolution(_isFullscreen);

                PlayerPrefs.SetInt("fullscreen", _isFullscreen ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        private void ApplyResolution(bool fullscreen)
        {
            if (fullscreen)
            {
                // 全屏：强制 1600×1200，全屏窗口模式（兼容性最好）
                Screen.SetResolution(TargetWidth, TargetHeight, FullScreenMode.FullScreenWindow);
                Screen.fullScreen = true;
            }
            else
            {
                // 窗口：保持相同分辨率，禁用全屏
                Screen.SetResolution(TargetWidth, TargetHeight, false);
                Screen.fullScreen = false;
            }

            Debug.Log($"[GlobalFullscreenToggle] {(fullscreen ? "全屏" : "窗口")}模式  分辨率：{TargetWidth}x{TargetHeight}");
        }
    }
}