using UnityEngine;
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

    bool _playerInRange = false;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
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

        //if (promptRoot && promptText && !string.IsNullOrWhiteSpace(promptString))
        //    promptText.text = promptString;
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
        if (!inventory)
        {
            //Debug.LogWarning("[PickupItem2D] 未找到 InventoryLite，无法拾取。");
            return;
        }

        // 加入背包
        int newCount = inventory.Add(itemId, amount);

        // 弹出“获得xxx”
        string displayName = itemId;
        if (itemDB)
        {
            var def = itemDB.Get(itemId);
            if (def != null && !string.IsNullOrWhiteSpace(def.displayName))
                displayName = def.displayName;
        }
        if (InfoDialogUI.Instance)
        {
            InfoDialogUI.Instance.ShowMessage($"获得 {displayName} x{amount}");
            // 可选：若想几秒后恢复默认提示，可启动一个协程在n秒后 InfoDialogUI.Instance.Clear();
        }

        // 可选：保存背包
        // inventory.Save();

        if (SlotUIController.Instance)
            SlotUIController.Instance.ShowPackageSlotFromPickup();


        // 处理物体
        if (destroyOnPickup)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}
