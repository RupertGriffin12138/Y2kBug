using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InfoDialogUI : MonoBehaviour
{
    public static InfoDialogUI Instance;  // 方便槽位直接调用（也可走 Inspector 引用）

    [Header("对话框文本（TMP）")]
    public TMP_Text textBoxText; // TextBoxText UI Text component

    [Header("名字框文本（TMP）")]
    public TMP_Text nameBoxText; // NameBoxText UI Text component

    [Header("箭头图像")]
    public Image arrowImage; // Arrow image to indicate pressing E key

    [Header("无悬停时的默认提示")]
    [TextArea]
    public string idleHint = "将鼠标移到物品上查看信息";

    private bool isShowingDialogue = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    void Start()
    {
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
    }

    /// <summary>结束显示对话。</summary>
    public void EndDialogue()
    {
        isShowingDialogue = false;
        Clear();
    }

    /// <summary>显示箭头。</summary>
    public void ShowArrow()
    {
        if (arrowImage != null)
        {
            arrowImage.enabled = true;
        }
    }

    /// <summary>隐藏箭头。</summary>
    public void HideArrow()
    {
        if (arrowImage != null)
        {
            arrowImage.enabled = false;
        }
    }
}


