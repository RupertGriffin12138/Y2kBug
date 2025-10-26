using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class InfoDialogUI : MonoBehaviour
{
    public static InfoDialogUI Instance;  // 方便槽位直接调用（也可走 Inspector 引用）

    [Header("对话框文本（TMP）")]
    public TMP_Text textBoxText; // TextBoxText UI Text component

    [Header("名字框文本（TMP）")]
    public TMP_Text nameBoxText; // NameBoxText UI Text component

    [Header("箭头动画（UGUI）")]
    public GameObject arrowGO;            // 有 Image + Animator 的那个对象
    public Animator arrowAnimator;        // 指向箭头的 Animator（可选：自动 Get）

    [Header("无悬停时的默认提示")]
    [TextArea]
    public string idleHint = "将鼠标移到物品上查看信息";

    [Header("卡通对象")]
    public GameObject[] cartoonObjects; // Array of cartoon objects (T_cartoon_1, T_cartoon_2, etc.)

    private bool isShowingDialogue = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    void Start()
    {
        // 防御：若忘了拖引用，自动取一次
        if (!arrowAnimator && arrowGO) arrowAnimator = arrowGO.GetComponent<Animator>();

        Clear(); // 初始显示默认提示
    }

    /// <summary>显示物品名称 + 第二行提示。</summary>
    public void ShowItem(string displayName, bool showUseTip = true)
    {
        if (!textBoxText || !nameBoxText) return;
        if (showUseTip)
            textBoxText.text = $"{displayName}\n<size=90%>－点击调查/使用－</size>";
        else
            textBoxText.text = displayName;
    }

    /// <summary>显示任意文本（可用于系统消息）。</summary>
    public void ShowMessage(string message)
    {
        if (!textBoxText) return;
        textBoxText.text = message;
    }

    /// <summary>恢复默认提示。</summary>
    public void Clear()
    {
        if (isShowingDialogue) return;
        textBoxText.text = idleHint;
        nameBoxText.text = "";
        HideArrow();
        DisableCartoons();
        isShowingDialogue = false;
    }

    /// <summary>设置名字文本。</summary>
    public void SetNameText(string name)
    {
        if (!nameBoxText) return;
        nameBoxText.text = name;
    }

    /// <summary>开始显示对话。</summary>
    public void StartDialogue()
    {
        isShowingDialogue = true;
        textBoxText.text = ""; // 清除默认提示
        nameBoxText.text = ""; // 确保名字框为空
        HideArrow();
        DisableCartoons();
    }

    /// <summary>结束显示对话。</summary>
    public void EndDialogue()
    {
        isShowingDialogue = false;
        Clear();
    }

    /// <summary>显示箭头（开始循环动画）。</summary>
    public void ShowArrow()
    {
        if (!arrowGO) return;

        arrowGO.SetActive(true);                 // 显示对象
        if (arrowAnimator)
        {
            arrowAnimator.speed = 1f;            // 确保动画在播放
            // 可选：强制从头播放
            // arrowAnimator.Play("Arrow_Loop", 0, 0f);
        }
    }

    /// <summary>隐藏箭头（停止动画）。</summary>
    public void HideArrow()
    {
        if (!arrowGO) return;

        // 方式1：直接隐藏（简单稳妥）
        arrowGO.SetActive(false);

        // 方式2：不隐藏对象，仅暂停（如需保持布局）
        // if (arrowAnimator) arrowAnimator.speed = 0f;
    }

    /// <summary>禁用所有卡通对象。</summary>
    public void DisableCartoons()
    {
        foreach (GameObject obj in cartoonObjects)
        {
            if (obj != null)
            {
                SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0f);
                }
            }
        }
    }

    /// <summary>启用指定的卡通对象并使其逐渐变得不透明。</summary>
    public void EnableCartoon(int index)
    {
        if (index >= 0 && index < cartoonObjects.Length)
        {
            GameObject obj = cartoonObjects[index];
            if (obj != null)
            {
                StartCoroutine(FadeIn(obj));
            }
         
        }
    }

    /// <summary>使物体逐渐变得不透明。</summary>
    IEnumerator FadeIn(GameObject obj)
    {
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            float duration = 1f; // 淡入持续时间
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                float alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 1f);
        }
    }

    IEnumerator FadeOut(GameObject obj)
    {
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            float duration = 1f; // 淡出持续时间
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0f);
        }
    }

    /// <summary>使所有卡通对象逐渐变得透明。</summary>
    public void DisableAllCartoonsWithFadeOut()
    {
        foreach (GameObject obj in cartoonObjects)
        {
            if (obj != null)
            {
                StartCoroutine(FadeOut(obj));
            }
        }
    }
}


