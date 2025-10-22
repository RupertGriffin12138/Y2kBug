using UnityEngine;
using TMPro;

public class InfoDialogUI : MonoBehaviour
{
    public static InfoDialogUI Instance;  // 方便槽位直接调用（也可走 Inspector 引用）

    [Header("对话框文本（TMP）")]
    public TMP_Text text;

    [Header("无悬停时的默认提示")]
    [TextArea]
    public string idleHint = "将鼠标移到物品上查看信息";

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
        if (!text) return;
        if (showUseTip)
            text.text = $"{displayName}\n<size=90%>－点击调查/使用－</size>";
        else
            text.text = displayName;
    }

    /// <summary>显示任意文本（可用于系统消息）。</summary>
    public void ShowMessage(string message)
    {
        if (!text) return;
        text.text = message;
    }

    /// <summary>恢复默认提示。</summary>
    public void Clear()
    {
        if (!text) return;
        text.text = idleHint;
    }
}
