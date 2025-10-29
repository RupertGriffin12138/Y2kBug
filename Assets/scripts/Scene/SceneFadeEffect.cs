using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scene
{
    /// <summary>
    /// 通用场景渐入渐出效果类（黑屏淡出/淡入）
    /// 用法：
    ///     SceneFadeEffect.Instance.FadeOutAndLoad("NextScene", 1.5f, 2f);
    ///     SceneFadeEffect.Instance.FadeIn(1.5f);
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class SceneFadeEffect : MonoBehaviour
    {

        private Image fadeImage;
        private Canvas canvas;

        private void Awake()
        {
            // 初始化 Canvas + Image
            canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // 确保在最上层

            fadeImage = new GameObject("FadeImage").AddComponent<Image>();
            fadeImage.transform.SetParent(canvas.transform, false);
            // 让它在 Canvas 的第 2 个位置（下标从 0 开始）
            fadeImage.transform.SetSiblingIndex(1);
            RectTransform rect = fadeImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            fadeImage.color = new Color(0, 0, 0, 0); // 初始透明
            fadeImage.raycastTarget = false;
        }

        /// <summary>
        /// 渐出并切换场景
        /// </summary>
        /// <param name="nextScene">目标场景名</param>
        /// <param name="fadeDuration">淡出时间（秒）</param>
        /// <param name="holdDuration">黑屏停留时间（秒）</param>
        public void FadeOutAndLoad(string nextScene, float fadeDuration = 1.5f, float holdDuration = 0f)
        {
            StartCoroutine(FadeOutAndLoadCoroutine(nextScene, fadeDuration, holdDuration));
        }

        /// <summary>
        /// 单纯执行黑幕渐出
        /// </summary>
        public void FadeOut(float fadeDuration = 1.5f)
        {
            StartCoroutine(FadeRoutine(0f, 1f, fadeDuration));
        }

        /// <summary>
        /// 单纯执行黑幕渐入
        /// </summary>
        public void FadeIn(float fadeDuration = 1.5f)
        {
            StartCoroutine(FadeRoutine(1f, 0f, fadeDuration));
        }

        private IEnumerator FadeOutAndLoadCoroutine(string nextScene, float fadeDuration, float holdDuration)
        {
            if (nextScene!="C1S1 campus")
            {
                AudioClipHelper.Instance.Play_Footsteps();
            }

            // 渐出到全黑
            yield return StartCoroutine(FadeRoutine(0f, 1f, fadeDuration));

            // 黑屏停留（可选）
            if (holdDuration > 0f)
                yield return new WaitForSeconds(holdDuration);
            
            if (nextScene != "C1S1 campus")
                AudioClipHelper.Instance.Stop_Footsteps();

            // 加载新场景
            AsyncOperation async = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Single);

            // 等待场景加载完毕（确保不会提前淡入）
            while (!async.isDone)
                yield return null;

        

            // 场景加载完成后从黑幕淡入
            yield return StartCoroutine(FadeRoutine(1f, 0f, fadeDuration));
        }

        private IEnumerator FadeRoutine(float from, float to, float duration)
        {
            float time = 0f;
            Color color = fadeImage.color;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                color.a = Mathf.Lerp(from, to, t);
                fadeImage.color = color;
                yield return null;
            }

            color.a = to;
            fadeImage.color = color;
        }
    }
}
