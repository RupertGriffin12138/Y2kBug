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
    public string docId = "note1";          // 必须与 DocDB 中 id 一致
    public bool openReaderOnPickup = true;     // 拾取后立即打开阅读面板
    public bool destroyAfterPickup = false;    // 收录后是否移除场景中的纸条

    [Header("输入")]
    public KeyCode pickupKey = KeyCode.E;

    [Header("提示UI（世界空间）")]
    //public GameObject promptRoot;              // 头顶小Canvas
    //public TMP_Text promptText;                // “按E阅读/收录”
    [TextArea] public string promptString = "按 <b>E</b> 阅读/收录";

    [Header("引用（可留空自动找）")]
    public DocInventoryLite docInventory;      // 场景里的 DocInventoryLite
    public DocDB docDB;                        // 若不设，优先从 docInventory.docDB 获取
    public DocReaderPanel readerPanel;         // 可选：阅读面板（下面提供示例）

    [Header("可选音效")]
    public AudioSource sfxSource;
    public AudioClip pickupSfx;

    [Header("Save")]
    public SaveTag tag;

    bool _inRange;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
        tag = GetComponent<SaveTag>();
    }

    void Awake()
    {
        //if (promptRoot) promptRoot.SetActive(false);
    }

    void Start()
    {
        if (!docInventory) docInventory = FindObjectOfType<DocInventoryLite>();
        if (!docDB && docInventory) docDB = docInventory.docDB;
        if (!readerPanel) readerPanel = FindObjectOfType<DocReaderPanel>(true);

        // === 读档应用：若已被禁用，则直接隐藏自己 ===
        if (tag && !string.IsNullOrEmpty(tag.id) && GameState.Current != null)
        {
            if (GameState.IsObjectDisabled(tag.id))
            {
                gameObject.SetActive(false);
                return;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _inRange = true;

            // 显示提示到下方 InfoDialogUI
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.ShowMessage(promptString);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _inRange = false;

            // 离开后恢复默认提示
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.Clear();
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

        // 提示
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

        // === 存档：写回 GameState（收录 + （可选）禁用实体） ===
        GameState.CollectDoc(docId); // 文档加入“已收录”
        // 若你的文档实体只出现一次，建议直接禁用它：
        if (tag && !string.IsNullOrEmpty(tag.id))
            GameState.AddDisabledObject(tag.id);

        // 记录当前场景名，便于继续游戏回到本关
        if (GameState.Current != null)
            GameState.Current.lastScene = SceneManager.GetActiveScene().name;

        // 打开阅读面板（保留你原本的流程）
        if (openReaderOnPickup && readerPanel && def != null)
        {
            if (SlotUIController.Instance)
                SlotUIController.Instance.StartCoroutine(OpenReaderStable(def));
            else
                StartCoroutine(OpenReaderStable(def));
        }

        // 立刻保存
        GameState.SaveNow();

        // 处理场景中的纸条
        if (destroyAfterPickup) Destroy(gameObject);
        // 如果不销毁，可以保留激活状态；下次读档时会因禁用列表而不再出现
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

    // 如果在阅读面板里点击“标记已读”，可直接调用这个
    public void MarkReadNow()
    {
        if (!string.IsNullOrEmpty(docId))
        {
            GameState.MarkDocRead(docId);
            GameState.SaveNow();
        }
    }
}
