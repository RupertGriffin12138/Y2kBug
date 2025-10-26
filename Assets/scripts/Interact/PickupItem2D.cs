using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class PickupItem2D : MonoBehaviour
{
    [Header("拾取配置")]
    public string itemId = "sparkler";    // ItemDB里配置过的id
    public int amount = 1;
    public KeyCode pickupKey = KeyCode.E;
    public bool destroyOnPickup = true;       // 拾取后销毁该物体；否则可改为SetActive(false)

    [Header("提示UI（可选）")]
    //public GameObject promptRoot;             // 放一个世界空间Canvas作为子物体，挂一段“按E拾取”文字
    //public TMP_Text promptText;               // 如果留空就不改写文字
    [TextArea] public string promptString = "按 <b>E</b> 拾取";

    [Header("引用（可留空自动找）")]
    public InventoryLite inventory;           // 若不手动拖拽，会在场景中自动Find
    public ItemDB itemDB;                     // 若不设，则尝试用 inventory.itemDB

    [Header("Save")]
    public SaveTag tag;               // 可选；没有则仍可拾取，但无法跨读档隐藏

    bool _playerInRange = false;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
        tag = GetComponent<SaveTag>(); // 方便自动挂上
    }

    void Awake()
    {
        //if (promptRoot) promptRoot.SetActive(false);
    }

    void Start()
    {
        if (!inventory)
            inventory = FindObjectOfType<InventoryLite>();
        if (!itemDB && inventory)
            itemDB = inventory.itemDB;

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
        //Debug.Log($"[Pickup] Enter by {other.name}, tag={other.tag}", this);
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;

            // [MOD] 进入范围时，把提示显示到下方的 InfoDialogUI
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.ShowMessage(promptString);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //Debug.Log($"[Pickup] Exit by {other.name}, tag={other.tag}", this);
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;

            // [MOD] 离开范围时，恢复默认提示
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.Clear();

            // [REMOVED] 不再隐藏世界空间提示
            // if (promptRoot) promptRoot.SetActive(false);
        }
    }

    void Update()
    {
        if (_playerInRange && Input.GetKeyDown(pickupKey))
        {
            //Debug.Log("[Pickup] E pressed inside trigger", this);
            TryPickup();
        }
    }

    void TryPickup()
    {
        if (!inventory) return;

        // 1) 背包 + 你已有的逻辑
        int newCount = inventory.Add(itemId, amount);

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

        // 2) === 存档：写回 GameState（禁用该对象 + 记录物品） ===
        if (tag && !string.IsNullOrEmpty(tag.id))
            GameState.AddDisabledObject(tag.id);    // 下次读档时不再出现
        GameState.AddItem(itemId, amount);

        // 更新“当前场景名”（方便继续游戏回到此关）
        if (GameState.Current != null)
            GameState.Current.lastScene = SceneManager.GetActiveScene().name;

        GameState.SaveNow(); // 立刻存一手（Demo 简单可靠）

        // 3) 处理物体
        if (destroyOnPickup) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}
