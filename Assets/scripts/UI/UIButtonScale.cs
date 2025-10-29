using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>
    /// ��������Ŵ󡢰�����С�İ�ť��Ч��uGUI��
    /// ���ýű�������Ҫ���ŵ� RectTransform �ϣ�ͨ���� Button ���壩
    /// </summary>
    [DisallowMultipleComponent]
    public class UIButtonScale : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        [Header("Scale Settings")]
        [Tooltip("����ʱ����ڳ�ʼ���ŵı���")]
        public float hoverScale = 1.06f;

        [Tooltip("����ʱ����ڳ�ʼ���ŵı���")]
        public float pressedScale = 0.94f;

        [Tooltip("���Ŷ���ʱ�����룩")]
        public float tweenDuration = 0.08f;

        [Tooltip("�������ߣ�����Ease Out��")]
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
            // �����ǰѡ�ж��󣬱���Selected״̬�ڵ�Hover
            if (EventSystem.current && EventSystem.current.currentSelectedGameObject != null)
                EventSystem.current.SetSelectedGameObject(null);
            UpdateTargetScale();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerOver = false;
            isPointerDown = false;
            UpdateTargetScale(); // �ص�1��hover
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
                t += Time.unscaledDeltaTime / duration; // �˵����� unscaled
                float k = ease.Evaluate(Mathf.Clamp01(t));
                rt.localScale = Vector3.LerpUnclamped(from, to, k);
                yield return null;
            }
            rt.localScale = to;
            tweenCo = null;
        }

        // �Զ���λ�߼�
        void OnDisable()
        {
            // ����ť������屻����ʱ�Զ�����״̬
            if (tweenCo != null) StopCoroutine(tweenCo);
            tweenCo = null;

            rt.localScale = baseScale;
            isPointerOver = false;
            isPointerDown = false;
        }

        // ����������ʱ���ã����绻�ֱ��ʻ���Ҫ�ָ�BaseScale��
        public void ResetBaseScaleToCurrent()
        {
            baseScale = rt.localScale;
            isPointerOver = false;
            isPointerDown = false;
        }
    }
}
