using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsScroll : MonoBehaviour
{
    public TMP_FontAsset specifiedFontAsset; // ��Inspector��ָ�������ʲ�
    public float delayBetweenLines = 2f; // ÿ��֮����ӳ�ʱ��
    public float fadeDuration = 1f; // ����/������ʱ��

    private string[] creditsTexts = new string[]
    {
        "ǧ��棬һ����ʵ�����ڼ������ʷ�ϵĳ������ȷ�е�˵��һ������ϵͳ����ߵ�©����Ҳ������������֪�ġ�BUG����",
        "����������������������ͳһ���洢ϵͳ����ͬһʱ�䵮���ġ�С���ӡ�����δ��������ʮ���ʱ���ʼ���ݷ��ڰ�����",
        "һ�������Լ����������ȴ����������ͽ������Ҫʱ�̣�׼�������ǵ�����ں�ʱ���ϵ�С��Ϸ����һ����",
        "�źߣ�Ҳ������Ļ�ݻ���������Ϣ�Ƽ�Ϊ��������ᣬ�԰ɡ�",
        "������ʼ�յȴ��ţ��ȴ�����һ�̵ĵ�����",
        "�����ҡ����ǣ����ÿǧ��Ż����һ�εĻ��ᣬ���Ǳ������ɱ����ҡ���",
        "����Ա�Ƿ�����������ܻᵼ����������������©�����Ծ�ʮ�����ʼ��Ϊ���ĳ���������׼����",
        "С���ӿ���Ҳû���뵽������Ϊӭ�ӵĳ���������������һ������ǽ��",
        "���գ��Ǹ�ʱ���ڶ����˵�����֮�е����ˡ�",
        "��ǧ��潵�ٵĿ�纣Х�ƺ���û�жԵ̷��걸����ʵ������������еľ޴�Ӱ�졣",
        "������������ʱ�����ڶ̴ٵ��˾Ⱥ�Ѹ���������Ļ��",
        "ǧ�����Ҳû���κλ��������Ǹ���������ռ�����Ļ��᣿",
        "Ҳ�����еģ���һ����ȫû����ע�⵽�������磬��һ���㹻����Ľ������˳�ֵ����������������硣",
        "ǧ�����Լ�����ˣ���������ʹ�����ϸ����ǧ��֮Լ��",
        "����Ҫ�߽��ľ�������һ�����磬�����������������ĵģ�����ո�µ�21���ͣ�����......"
    };

    private int currentLineIndex = 0;
    private TMP_Text creditsTextComponent;

    void Start()
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

    private void Update()
    {
       
        
    }

    IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator ShowCredits()
    {
        while (currentLineIndex < creditsTexts.Length)
        {
            // ��ʾ��ǰ��
            creditsTextComponent.text = creditsTexts[currentLineIndex];
            creditsTextComponent.alpha = 0f;
            yield return StartCoroutine(FadeIn());

            // �ӳ�һ��ʱ��
            yield return new WaitForSeconds(delayBetweenLines);

            // ������ǰ��
            yield return StartCoroutine(FadeOut());

            // �ƶ�����һ��
            currentLineIndex++;
        }

        // ����������ʾ��Ϻ������һ������
        LoadSceneAfterDelay("C1S1 firework", 2f); 
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


