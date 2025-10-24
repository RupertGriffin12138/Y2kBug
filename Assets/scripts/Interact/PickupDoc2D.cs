using UnityEngine;
using TMPro;
using UnityEngine.UI;              // ScrollRect
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class PickupDoc2D : MonoBehaviour
{
    [Header("文档配置")]
    public string docId = "note1";          // 必须与 DocDB 中 id 一致
    public bool openReaderOnPickup = true;     // 拾取后立即打开阅读面板
    public bool destroyAfterPickup = false;    // 收录后是否移除场景中的纸条

    [Header("输入")]
    public KeyCode pickupKey = KeyCode.E;

    [Header("提示UI（世界空间）")]
    public GameObject promptRoot;              // 头顶小Canvas
    public TMP_Text promptText;                // “按E阅读/收录”
    [TextArea] public string promptString = "按 <b>E</b> 阅读/收录";

    [Header("引用（可留空自动找）")]
    public DocInventoryLite docInventory;      // 场景里的 DocInventoryLite
    public DocDB docDB;                        // 若不设，优先从 docInventory.docDB 获取
    public DocReaderPanel readerPanel;         // 可选：阅读面板（下面提供示例）

    [Header("可选音效")]
    public AudioSource sfxSource;
    public AudioClip pickupSfx;

    bool _inRange;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void Awake()
    {
        if (promptRoot) promptRoot.SetActive(false);
    }

    void Start()
    {
        if (!docInventory) docInventory = FindObjectOfType<DocInventoryLite>();
        if (!docDB && docInventory) docDB = docInventory.docDB;
        if (!readerPanel) readerPanel = FindObjectOfType<DocReaderPanel>(true);

        if (promptText && !string.IsNullOrWhiteSpace(promptString))
        {
            promptText.text = promptString;
            // 外扩描边，增强可读性（可按需微调）
            var mat = promptText.fontMaterial;
            if (mat)
            {
                mat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.2f);
                mat.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
                mat.SetFloat(ShaderUtilities.ID_FaceDilate, 0.12f);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _inRange = true;
            if (promptRoot) promptRoot.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _inRange = false;
            if (promptRoot) promptRoot.SetActive(false);
        }
    }

    void Update()
    {
        if (_inRange && Input.GetKeyDown(pickupKey))
        {
            TryPickupDoc();
        }
    }

    void TryPickupDoc()
    {
        if (!docInventory)
        {
            Debug.LogWarning("[PickupDoc2D] 未找到 DocInventoryLite。", this);
            return;
        }

        var def = docDB ? docDB.Get(docId) : null;
        string display = def != null && !string.IsNullOrWhiteSpace(def.displayName)
                         ? def.displayName : docId;

        bool isNew = docInventory.AddOnce(docId);   // 已有则不重复添加

        // 提示：“获得/已收录《xxx》”
        if (InfoDialogUI.Instance)
        {
            string msg = isNew ? $"获得《{display}》" : $"已收录《{display}》";
            InfoDialogUI.Instance.ShowMessage(msg);
            InfoDialogUI.Instance.CancelInvoke(nameof(InfoDialogUI.Clear));
            InfoDialogUI.Instance.Invoke(nameof(InfoDialogUI.Clear), 2f);
        }

        // 音效
        if (sfxSource && pickupSfx) sfxSource.PlayOneShot(pickupSfx);

        if (SlotUIController.Instance)
            SlotUIController.Instance.ShowFileSlotFromPickup();

        // 打开阅读面板
        if (openReaderOnPickup && readerPanel && def != null)
        {
            // 如果有 UI 控制器，让它来启动协程（不会因销毁中断）
            if (SlotUIController.Instance)
                SlotUIController.Instance.StartCoroutine(OpenReaderStable(def));
            else
                StartCoroutine(OpenReaderStable(def));
        }

        // 处理场景中的纸条
        if (destroyAfterPickup) Destroy(gameObject);
        else if (promptRoot) promptRoot.SetActive(false); // 不销毁就隐藏提示
    }

    IEnumerator OpenReaderStable(DocDB.DocDef def)
    {
        // 1) 先确保面板处于激活状态（有些布局在未激活时不会建立）
        if (readerPanel.rootPanel && !readerPanel.rootPanel.activeSelf)
            readerPanel.rootPanel.SetActive(true);

        // 2) 立即强刷一次 Canvas，建立首帧布局
        Canvas.ForceUpdateCanvases();

        // 3) 等待到下一帧（让激活/布局真正生效）
        yield return null;

        // 4) 真正打开并填充内容（此时 UI 已经 ready）
        readerPanel.Open(def);

        // 5) 再强刷一次，避免首次填充大文本的延迟
        Canvas.ForceUpdateCanvases();

        // 6) 再等一帧，确保 ScrollRect 可正确定位到顶部（一些版本需要）
        yield return null;

        // 7) 兜底：把滚动条推回顶部（若 Open 里已做，这里也无妨）
        var scrollRect = readerPanel.contentText ?
            readerPanel.contentText.GetComponentInParent<ScrollRect>() : null;
        if (scrollRect) scrollRect.normalizedPosition = new Vector2(0, 1);

        // （可选）把焦点给关闭按钮或滚动区域，避免首帧键盘事件被别的 UI 截走
        // EventSystem.current?.SetSelectedGameObject(scrollRect?.gameObject);
    }
}
