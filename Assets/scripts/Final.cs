using System.Collections;
using Scene;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Final : MonoBehaviour
{
    [Header("目标场景名（例如 MainMenu）")]
    public string targetScene = "MainMenu";

    [Header("文字显示后等待时间（秒）")]
    public float waitAfterText = 3f;

    [Header("淡入淡出时长（秒）")]
    public float fadeDuration = 1f;

    [Header("图片面板（进入场景默认显示）")]
    public GameObject imagePanel;

    [Header("文字面板（等待E后显示）")]
    public GameObject textPanel;

    private CanvasGroup imageCanvas;
    private CanvasGroup textCanvas;

    private bool pressed;

    private void Start()
    {
        // 初始化 CanvasGroup（确保两者存在）
        if (imagePanel)
        {
            imageCanvas = imagePanel.GetComponent<CanvasGroup>();
            if (!imageCanvas)
                imageCanvas = imagePanel.AddComponent<CanvasGroup>();
        }

        if (textPanel)
        {
            textCanvas = textPanel.GetComponent<CanvasGroup>();
            if (!textCanvas)
                textCanvas = textPanel.AddComponent<CanvasGroup>();
        }

        // 初始状态：imagePanel 可见，textPanel 隐藏
        if (imagePanel)
        {
            imagePanel.SetActive(true);
            imageCanvas.alpha = 0f;
        }

        if (textPanel)
        {
            textPanel.SetActive(true);
            textCanvas.alpha = 0f;
        }

        // 开始淡入显示 imagePanel
        StartCoroutine(FadeIn(imageCanvas));
    }

    private void Update()
    {
        if (!pressed && Input.GetKeyDown(KeyCode.E))
        {
            pressed = true;
            StartCoroutine(SwitchFlow());
        }
    }

    private IEnumerator SwitchFlow()
    {
        // Step 1: 淡出 imagePanel
        if (imageCanvas)
            yield return FadeOut(imageCanvas);

        // Step 2: 淡入 textPanel
        if (textCanvas)
            yield return FadeIn(textCanvas);

        // Step 3: 停留几秒
        yield return new WaitForSeconds(waitAfterText);
        
        SceneManager.LoadScene(targetScene);
        
    }

    // ――― 工具函数 ―――
    private IEnumerator FadeIn(CanvasGroup cg)
    {
        if (!cg) yield break;
        cg.gameObject.SetActive(true);
        float t = 0f;
        while (t < fadeDuration)
        {
            cg.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        cg.alpha = 1f;
    }

    private IEnumerator FadeOut(CanvasGroup cg)
    {
        if (!cg) yield break;
        float t = 0f;
        while (t < fadeDuration)
        {
            cg.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        cg.alpha = 0f;
        cg.gameObject.SetActive(false);
    }
}
