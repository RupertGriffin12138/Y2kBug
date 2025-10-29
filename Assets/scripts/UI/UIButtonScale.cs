using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>
    /// 鼠标悬浮放大、按下缩小的按钮动效（uGUI）
    /// 将该脚本挂在需要缩放的 RectTransform 上（通常是 Button 本体）
    /// </summary>
    [DisallowMultipleComponent]
    public class UIButtonScale : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        [Header("Scale Settings")]
        [Tooltip("悬浮时相对于初始缩放的倍数")]
        public float hoverScale = 1.06f;

        [Tooltip("按下时相对于初始缩放的倍数")]
        public float pressedScale = 0.94f;

        [Tooltip("缩放动画时长（秒）")]
        public float tweenDuration = 0.08f;

        [Tooltip("缓动曲线（建议Ease Out）")]
        public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

        RectTransform rt;
        Vector3 baseScale;
        Coroutine tweenCo;
        bool isPointerOver;
        bool isPointerDown;

        void Awake()
        {
            rt = transform as RectTransform;
            baseScale = rt.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerOver = true;
            // 清除当前选中对象，避免Selected状态遮挡Hover
            if (EventSystem.current && EventSystem.current.currentSelectedGameObject != null)
                EventSystem.current.SetSelectedGameObject(null);
            UpdateTargetScale();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerOver = false;
            isPointerDown = false;
            UpdateTargetScale(); // 回到1或hover
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPointerDown = true;
            UpdateTargetScale();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPointerDown = false;
            UpdateTargetScale();
        }

        void UpdateTargetScale()
        {
            float targetMul = 1f;

            if (isPointerDown)
            {
                targetMul = pressedScale;
                AudioClipHelper.Instance.Play_UIClick();
            }
            else if (isPointerOver)
            {
                targetMul = hoverScale;
                AudioClipHelper.Instance.Play_UIHover(); 
            }

            Vector3 target = baseScale * targetMul;
            StartTweenTo(target);
        }

        void StartTweenTo(Vector3 target)
        {
            if (tweenCo != null) StopCoroutine(tweenCo);
            tweenCo = StartCoroutine(TweenScale(rt.localScale, target, tweenDuration));
        }

        IEnumerator TweenScale(Vector3 from, Vector3 to, float duration)
        {
            if (duration <= 0f)
            {
                rt.localScale = to;
                yield break;
            }

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / duration; // 菜单可用 unscaled
                float k = ease.Evaluate(Mathf.Clamp01(t));
                rt.localScale = Vector3.LerpUnclamped(from, to, k);
                yield return null;
            }
            rt.localScale = to;
            tweenCo = null;
        }

        // 自动复位逻辑
        void OnDisable()
        {
            // 当按钮所在面板被隐藏时自动重置状态
            if (tweenCo != null) StopCoroutine(tweenCo);
            tweenCo = null;

            rt.localScale = baseScale;
            isPointerOver = false;
            isPointerDown = false;
        }

        // 方便在运行时重置（比如换分辨率或需要恢复BaseScale）
        public void ResetBaseScaleToCurrent()
        {
            baseScale = rt.localScale;
            isPointerOver = false;
            isPointerDown = false;
        }
    }
}
