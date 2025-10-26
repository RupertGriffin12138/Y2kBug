using UnityEngine;
using TMPro;
using UnityEngine.UI;              // ScrollRect
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class PickupDoc2D : MonoBehaviour
{
    [Header("文档配置")]
    public string docId = "note1";              // 必须与 DocDB 中 id 一致
    public bool openReaderOnPickup = true;      // 拾取后立即打开阅读面板
    public bool destroyAfterPickup = false;     // 收录后是否移除场景中的纸条

    [Header("输入")]
    public KeyCode pickupKey = KeyCode.E;

    [Header("提示UI（世界空间）")]
    [TextArea] public string promptString = "按 <b>E</b> 阅读/收录";

    [Header("引用（可留空自动找）")]
    public DocInventoryLite docInventory;      // 场景里的 DocInventoryLite
    public DocDB docDB;                        // 若不设，优先从 docInventory.docDB 获取
    public DocReaderPanel readerPanel;         // 可选：阅读面板

    [Header("可选音效")]
    public AudioSource sfxSource;
    public AudioClip pickupSfx;

    [Header("Save")]
    public SaveTag tag;                        // 一次性实体的稳定 id
    public bool autoSaveOnPickup = true;       // 拾取后是否立刻存档（推荐开启）

    bool _inRange;
    bool _consumed;                            // 防重复触发

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
        tag = GetComponent<SaveTag>();
    }

    void Start()
    {
        if (!docInventory) docInventory = FindObjectOfType<DocInventoryLite>();
        if (!docDB && docInventory) docDB = docInventory.docDB;
        if (!readerPanel) readerPanel = FindObjectOfType<DocReaderPanel>(true);

        // 确保存档已初始化（防止从特殊入口进入导致 GameState 为 null）
        if (GameState.Current == null)
            GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

        // 读档应用：若该实体已被禁用，则直接隐藏自己并失效
        if (tag && !string.IsNullOrEmpty(tag.id) && GameState.IsObjectDisabled(tag.id))
        {
            gameObject.SetActive(false);
            _consumed = true;
            return;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_consumed) return;
        if (other.CompareTag("Player"))
        {
            _inRange = true;
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.ShowMessage(promptString);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (_consumed) return;
        if (other.CompareTag("Player"))
        {
            _inRange = false;
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.Clear();
        }
    }

    void Update()
    {
        if (_consumed) return;
        if (_inRange && Input.GetKeyDown(pickupKey))
            TryPickupDoc();
    }

    void TryPickupDoc()
    {
        if (_consumed) return;

        if (!docInventory)
        {
            Debug.LogWarning("[PickupDoc2D] 未找到 DocInventoryLite。", this);
            return;
        }

        var def = docDB ? docDB.Get(docId) : null;
        string display = def != null && !string.IsNullOrWhiteSpace(def.displayName)
                         ? def.displayName : docId;

        // 1) 先写运行态数据库（避免 UI 延迟）
        bool isNew = docInventory.AddOnce(docId);   // 已有则不重复添加

        // 2) —— 写回 GameState（权威存档）——
        GameState.CollectDoc(docId);                // 文档加入“已收录”
        if (tag && !string.IsNullOrEmpty(tag.id))   // 一次性实体：加入禁用列表
            GameState.AddDisabledObject(tag.id);
        GameState.Current.lastScene = SceneManager.GetActiveScene().name;

        // 3) UI 提示与音效
        if (InfoDialogUI.Instance)
        {
            string msg = isNew ? $"获得《{display}》" : $"已收录《{display}》";
            InfoDialogUI.Instance.ShowMessage(msg);
            InfoDialogUI.Instance.CancelInvoke(nameof(InfoDialogUI.Clear));
            InfoDialogUI.Instance.Invoke(nameof(InfoDialogUI.Clear), 2f);
        }
        if (sfxSource && pickupSfx) sfxSource.PlayOneShot(pickupSfx);
        if (SlotUIController.Instance) SlotUIController.Instance.ShowFileSlotFromPickup();

        // 4) 阅读面板（如果需要）
        if (openReaderOnPickup && readerPanel && def != null)
        {
            if (SlotUIController.Instance)
                SlotUIController.Instance.StartCoroutine(OpenReaderStable(def));
            else
                StartCoroutine(OpenReaderStable(def));
        }

        // 5) 立即保存（可选）
        if (autoSaveOnPickup)
            GameState.SaveNow();

        // 6) 处理实体并防重入
        _consumed = true;
        if (destroyAfterPickup) Destroy(gameObject);
        // 不销毁时保持激活；下次读档会因禁用列表而不再出现
    }

    IEnumerator OpenReaderStable(DocDB.DocDef def)
    {
        if (readerPanel.rootPanel && !readerPanel.rootPanel.activeSelf)
            readerPanel.rootPanel.SetActive(true);

        Canvas.ForceUpdateCanvases();
        yield return null;

        readerPanel.Open(def);

        Canvas.ForceUpdateCanvases();
        yield return null;

        var scrollRect = readerPanel.contentText ?
            readerPanel.contentText.GetComponentInParent<ScrollRect>() : null;
        if (scrollRect) scrollRect.normalizedPosition = new Vector2(0, 1);
    }

    // 如果在阅读面板里点击“标记已读”，可直接调用这个（只更新 GameState）
    public void MarkReadNow()
    {
        if (!string.IsNullOrEmpty(docId))
        {
            GameState.MarkDocRead(docId);
            GameState.SaveNow();
        }
    }
}
