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
        public float duration = 2f; // Ĭ��ͣ��ʱ��Ϊ2��
    }

    public class CreditsScroll : MonoBehaviour
    {
        public TMP_FontAsset specifiedFontAsset; // ��Inspector��ָ�������ʲ�
        public float fadeDuration = 1f; // ����/������ʱ��
        public CreditLine[] creditsTexts; // �������飬������Inspector������

        private int currentLineIndex = 0;
        private TMP_Text creditsTextComponent;

        private void Start()
        {
            // ��ȡTMP_Text���
            creditsTextComponent = GetComponent<TMP_Text>();

            // ���TMP_Text����Ƿ����
            if (creditsTextComponent == null)
            {
                Debug.LogError("No TMP_Text component found on this GameObject.");
                return;
            }

            // ����ָ�������ʲ�
            if (specifiedFontAsset != null)
                creditsTextComponent.font = specifiedFontAsset;

            StartCoroutine(ShowCredits());
        }

        private IEnumerator ShowCredits()
        {
            while (currentLineIndex < creditsTexts.Length)
            {
                var creditLine = creditsTexts[currentLineIndex];

                // ��ʾ��ǰ��
                creditsTextComponent.text = creditLine.text;
                creditsTextComponent.alpha = 0f;
                yield return StartCoroutine(FadeIn());

                // �ӳ�һ��ʱ��
                yield return new WaitForSeconds(creditLine.duration);

                // ������ǰ��
                yield return StartCoroutine(FadeOut());

                // �ƶ�����һ��
                currentLineIndex++;
            }

            // ����������ʾ��Ϻ������һ������
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


