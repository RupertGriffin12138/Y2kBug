using TMPro;
using UnityEngine;

namespace Items
{
    public class DocReaderPanel : MonoBehaviour
    {
        [Header("数据")]
        public DocDB docDB;

        [Header("UI 引用")]
        public GameObject rootPanel;     // 整个 TextPage 面板（含 ScrollView）
        public TMP_Text contentText;     // ScrollView/Viewport/Content 上的 TMP_Text

        void Awake()
        {
            if (rootPanel) rootPanel.SetActive(false);
        }

        /// <summary>通过 id 打开（供 DocUI、拾取脚本等调用）</summary>
        public void OpenById(string docId)
        {
            if (docDB == null || string.IsNullOrEmpty(docId))
            {
                Debug.LogWarning("[DocReaderPanel] 无效 docId 或未设置 docDB。", this);
                return;
            }

            var def = docDB.Get(docId);
            if (def == null)
            {
                Debug.LogWarning($"[DocReaderPanel] docDB 中未找到 id: {docId}", this);
                return;
            }

            Open(def);
        }

        /// <summary>通过文档定义直接打开（供拾取脚本调用）</summary>
        public void Open(DocDB.DocDef def)
        {
            if (def == null)
            {
                Debug.LogWarning("[DocReaderPanel] 传入的 DocDef 为空。", this);
                return;
            }

            // --- 修复重点：确保 UI 已激活再刷新 ---
            if (rootPanel && !rootPanel.activeSelf)
                rootPanel.SetActive(true);

            // 立即刷新 Canvas 布局，防止“首次激活不显示”
            Canvas.ForceUpdateCanvases();

            // 填充文本内容
            if (contentText)
                contentText.text = def.content ?? "(暂无内容)";

            // 再刷新一次，确保文字布局更新
            Canvas.ForceUpdateCanvases();

            // 滚动条回到顶部
            var scrollRect = contentText?.GetComponentInParent<UnityEngine.UI.ScrollRect>();
            if (scrollRect)
                scrollRect.normalizedPosition = new Vector2(0, 1);
        }

        /// <summary>关闭阅读面板</summary>
        public void Close()
        {
            if (rootPanel) rootPanel.SetActive(false);
        }
    }
}
