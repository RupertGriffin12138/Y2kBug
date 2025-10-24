using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsScroll : MonoBehaviour
{
    public TMP_FontAsset specifiedFontAsset; // 在Inspector中指定字体资产
    public float delayBetweenLines = 2f; // 每行之间的延迟时间
    public float fadeDuration = 1f; // 渐显/渐隐的时间

    private string[] creditsTexts = new string[]
    {
        "千年虫，一个真实存在于计算机历史上的程序错误，确切地说，一个电脑系统设计者的漏洞，也就是我们所熟知的“BUG”。",
        "这个几乎跟随计算机编程语言统一、存储系统调整同一时间诞生的“小虫子”，在未来将近四十年的时间里，始终蛰伏在暗处。",
        "一点点积蓄自己的力量，等待着两个世纪交汇的重要时刻，准备用它那点儿日期和时间上的小把戏大闹一番。",
        "嗯哼，也许那真的会摧毁整个以信息科技为基础的社会，对吧。",
        "我想它始终等待着，等待着那一刻的到来。",
        "“不幸”的是，这个每千年才会出现一次的机会，还是被人类扼杀在了摇篮里。",
        "程序员们发现了这个可能会导致世界运作崩溃的漏洞，自九十年代开始就为它的出现做足了准备。",
        "小虫子可能也没有想到，人类为迎接的出生，早早筑起了一道道高墙。",
        "最终，那个时刻在多数人的意料之中到来了。",
        "随千年虫降临的狂风海啸似乎并没有对堤防完备的现实世界带来想象中的巨大影响。",
        "本该属于它的时代，在短促的扑救后，迅速落下了帷幕。",
        "千年虫再也没有任何机会了吗，那个完成自身终极任务的机会？",
        "也许是有的，在一个完全没有人注意到它的世界，在一个足够阴翳的角落获得了充分的生长和滋润后的世界。",
        "千年虫如约而至了，带着它的使命，赶赴这个千年之约。",
        "我们要走进的就是这样一个世界，伴随跨年的钟声到来的的，不是崭新的21世纪，而是......"
    };

    private int currentLineIndex = 0;
    private TMP_Text creditsTextComponent;

    void Start()
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
            // 显示当前行
            creditsTextComponent.text = creditsTexts[currentLineIndex];
            creditsTextComponent.alpha = 0f;
            yield return StartCoroutine(FadeIn());

            // 延迟一段时间
            yield return new WaitForSeconds(delayBetweenLines);

            // 渐隐当前行
            yield return StartCoroutine(FadeOut());

            // 移动到下一行
            currentLineIndex++;
        }

        // 所有文字显示完毕后加载下一个场景
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


