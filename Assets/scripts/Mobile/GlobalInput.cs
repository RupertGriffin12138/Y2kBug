using UnityEngine;

namespace Mobile
{
    public static class GlobalInput
    {
        private static bool _virtualEPressed;
        private static int _pressedFrame;
        
        private static Joystick _cachedJoystick;
        public static Joystick joystick
        {
            get
            {
                // 如果已经缓存过，就直接返回
                if (_cachedJoystick != null)
                    return _cachedJoystick;

                // 没缓存就尝试在场景中查找
                _cachedJoystick = Object.FindAnyObjectByType<FloatingJoystick>();

                // 如果没找到，可以尝试查找其它类型的摇杆（兼容 JoystickPack 的不同模式）
                if (_cachedJoystick == null)
                    _cachedJoystick = Object.FindAnyObjectByType<Joystick>();

                if (_cachedJoystick == null)
                    Debug.LogWarning("[PlayerInput] 未在场景中找到 Joystick 或 FloatingJoystick 实例。");

                return _cachedJoystick;
            }
        }

        /// <summary>由手机按钮调用</summary>
        public static void SimulateEPress()
        {
            // 按下时立即标记为当前帧
            _virtualEPressed = true;
            _pressedFrame = Time.frameCount;
        }

        /// <summary>统一检测（键盘 + 虚拟）</summary>
        public static bool GetEKeyDown()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            // 移动端：只看虚拟按钮
            if (_virtualEPressed && Time.frameCount == _pressedFrame)
            {
                // 只在点击当帧返回 true，然后立刻清掉
                _virtualEPressed = false;
                return true;
            }
            return false;
#else
            // PC / 编辑器：键盘 E 或当帧虚拟按钮都算
            bool keyDown = Input.GetKeyDown(KeyCode.E);
            bool virtualDown = _virtualEPressed && Time.frameCount == _pressedFrame;

            if (virtualDown)
                _virtualEPressed = false;

            return keyDown || virtualDown;
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnLoad()
        {
            _virtualEPressed = false;
            _pressedFrame = -1;
        }
    }
}