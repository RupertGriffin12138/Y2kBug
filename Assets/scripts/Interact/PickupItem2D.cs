using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class PickupItem2D : MonoBehaviour
{
    [Header("拾取配置")]
    [Tooltip("ItemDB 里的物品 id")]
    public string itemId = "sparkler";
    public int amount = 1;
    public KeyCode pickupKey = KeyCode.E;
    [Tooltip("拾取后销毁该物体；否则仅 SetActive(false)")]
    public bool destroyOnPickup = true;

    [Header("提示UI（可选）")]
    [TextArea] public string promptString = "按 <b>E</b> 拾取";

    [Header("引用（可留空自动找）")]
    public InventoryLite inventory;   // 若不手动拖拽，会在场景中自动 Find
    public ItemDB itemDB;             // 若不设，则尝试用 inventory.itemDB

    [Header("Save")]
    [Tooltip("必须有稳定 id 才能跨读档保持隐藏")]
    public SaveTag tag;
    [Tooltip("拾取后是否自动保存（推荐：勾选）")]
    public bool autoSaveOnPickup = true;

    bool _playerInRange = false;
    bool _consumed = false;           // 防止重复执行

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
        tag = GetComponent<SaveTag>(); // 方便自动挂上
    }

    void Start()
    {
        // 自动补引用
        if (!inventory) inventory = FindObjectOfType<InventoryLite>();
        if (!itemDB && inventory) itemDB = inventory.itemDB;

        // 确保 GameState 可用（防止从主菜单没走过加载路径时为 null）
        if (GameState.Current == null)
            GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

        // 读档应用：若该对象已被禁用，直接隐藏并不再工作
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
            _playerInRange = true;
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.ShowMessage(promptString);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (_consumed) return;
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.Clear();
        }
    }

    void Update()
    {
        if (_consumed) return;
        if (_playerInRange && Input.GetKeyDown(pickupKey))
            TryPickup();
    }

    void TryPickup()
    {
        if (_consumed) return;

        // 基础校验
        if (string.IsNullOrWhiteSpace(itemId) || amount == 0)
        {
            Debug.LogWarning($"[PickupItem2D] 无效的 itemId 或数量：{itemId}, {amount}", this);
            return;
        }
        if (!inventory)
        {
            Debug.LogWarning("[PickupItem2D] 未找到 InventoryLite，无法拾取。", this);
            return;
        }

        // 1) 先更新背包（你现有逻辑）
        int newCount = inventory.Add(itemId, amount);

        // 2) 计算展示名称
        string displayName = itemId;
        if (itemDB)
        {
            var def = itemDB.Get(itemId);
            if (def != null && !string.IsNullOrWhiteSpace(def.displayName))
                displayName = def.displayName;
        }

        if (InfoDialogUI.Instance)
            InfoDialogUI.Instance.ShowMessage($"获得 {displayName} x{amount}");
        if (SlotUIController.Instance)
            SlotUIController.Instance.ShowPackageSlotFromPickup();

        // 3) —— 写回 GameState（这是“存档里真正的数据”）——
        // 背包
        GameState.AddItem(itemId, amount);
        // 一次性对象：加入禁用列表（下次读档不再出现）
        if (tag && !string.IsNullOrEmpty(tag.id))
            GameState.AddDisabledObject(tag.id);
        // 记录当前位置的场景名（方便继续游戏回到此关）
        GameState.Current.lastScene = SceneManager.GetActiveScene().name;

        // 4) （可选）立即落盘
        if (autoSaveOnPickup)
            GameState.SaveNow();

        // 5) 处理自身并防重入
        _consumed = true;
        if (destroyOnPickup) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}
