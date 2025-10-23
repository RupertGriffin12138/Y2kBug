using UnityEngine;
using TMPro;

public class DocReaderPanel : MonoBehaviour
{
    [Header("数据")]
    public DocDB docDB;

    [Header("UI 引用")]
    public GameObject rootPanel;     // 整个 TextPage 面板（含 ScrollView）
    //public TMP_Text titleText;       // 顶部标题（显示 displayName）
    public TMP_Text contentText;     // ScrollView/Viewport/Content 上的 TMP_Text

    void Awake()
    {
        if (rootPanel) rootPanel.SetActive(false);
    }

    public void OpenById(string docId)
    {
        if (docDB == null || string.IsNullOrEmpty(docId)) return;
        var def = docDB.Get(docId);
        if (def == null) return;

        if (contentText)
            contentText.text = def.content ?? "";

        if (rootPanel)
            rootPanel.SetActive(true);

        // 可选：让滚动条回到顶部
        //var scrollRect = contentText?.GetComponentInParent<UnityEngine.UI.ScrollRect>();
        //if (scrollRect)
        //    scrollRect.normalizedPosition = new Vector2(0, 1);
    }

    public void Close()
    {
        if (rootPanel) rootPanel.SetActive(false);
    }
}
