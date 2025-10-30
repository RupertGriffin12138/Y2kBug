using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scene.C1S0
{
    [System.Serializable]
    public class CreditLine
    {
        public string text;
        public float duration = 2f; // 默认停留时间为2秒
    }

    public class CreditsScroll : MonoBehaviour
    {
        public TMP_FontAsset specifiedFontAsset; // 在Inspector中指定字体资产
        public float fadeDuration = 1f; // 渐显/渐隐的时间
        public CreditLine[] creditsTexts; // 公共数组，可以在Inspector中设置

        private int currentLineIndex = 0;
        private TMP_Text creditsTextComponent;

        private void Start()
        {
            // 获取TMP_Text组件
            creditsTextComponent = GetComponent<TMP_Text>();

            // 检查TMP_Text组件是否存在
            if (creditsTextComponent == null)
            {
                Debug.LogError("No TMP_Text component found on this GameObject.");
                return;
            }

            // 设置指定字体资产
            if (specifiedFontAsset != null)
                creditsTextComponent.font = specifiedFontAsset;

            StartCoroutine(ShowCredits());
        }

        private IEnumerator ShowCredits()
        {
            while (currentLineIndex < creditsTexts.Length)
            {
                var creditLine = creditsTexts[currentLineIndex];

                // 显示当前行
                creditsTextComponent.text = creditLine.text;
                creditsTextComponent.alpha = 0f;
                yield return StartCoroutine(FadeIn());

                // 延迟一段时间
                yield return new WaitForSeconds(creditLine.duration);

                // 渐隐当前行
                yield return StartCoroutine(FadeOut());

                // 移动到下一行
                currentLineIndex++;
            }

            // 所有文字显示完毕后加载下一个场景
            SceneManager.LoadScene("C1S0 firework");
        }

        #region Fade Coroutines
        IEnumerator FadeIn()
        {
            float elapsedTime = 0f;
            float startAlpha = creditsTextComponent.alpha;
            float endAlpha = 1f;

            while (elapsedTime < fadeDuration)
            {
                creditsTextComponent.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            creditsTextComponent.alpha = endAlpha;
        }

        IEnumerator FadeOut()
        {
            float elapsedTime = 0f;
            float startAlpha = creditsTextComponent.alpha;
            float endAlpha = 0f;

            while (elapsedTime < fadeDuration)
            {
                creditsTextComponent.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            creditsTextComponent.alpha = endAlpha;
        }

        #endregion
    }
}


