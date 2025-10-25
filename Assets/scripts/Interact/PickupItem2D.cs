using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class PickupItem2D : MonoBehaviour
{
    [Header("ʰȡ����")]
    public string itemId = "sparkler";    // ItemDB�����ù���id
    public int amount = 1;
    public KeyCode pickupKey = KeyCode.E;
    public bool destroyOnPickup = true;       // ʰȡ�����ٸ����壻����ɸ�ΪSetActive(false)

    [Header("��ʾUI����ѡ��")]
    //public GameObject promptRoot;             // ��һ������ռ�Canvas��Ϊ�����壬��һ�Ρ���Eʰȡ������
    //public TMP_Text promptText;               // ������վͲ���д����
    [TextArea] public string promptString = "�� <b>E</b> ʰȡ";

    [Header("���ã��������Զ��ң�")]
    public InventoryLite inventory;           // �����ֶ���ק�����ڳ������Զ�Find
    public ItemDB itemDB;                     // �����裬������ inventory.itemDB

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

            // [MOD] ���뷶Χʱ������ʾ��ʾ���·��� InfoDialogUI
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

            // [MOD] �뿪��Χʱ���ָ�Ĭ����ʾ
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.Clear();

            // [REMOVED] ������������ռ���ʾ
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
            //Debug.LogWarning("[PickupItem2D] δ�ҵ� InventoryLite���޷�ʰȡ��");
            return;
        }

        // ���뱳��
        int newCount = inventory.Add(itemId, amount);

        // ���������xxx��
        string displayName = itemId;
        if (itemDB)
        {
            var def = itemDB.Get(itemId);
            if (def != null && !string.IsNullOrWhiteSpace(def.displayName))
                displayName = def.displayName;
        }
        if (InfoDialogUI.Instance)
        {
            InfoDialogUI.Instance.ShowMessage($"��� {displayName} x{amount}");
            // ��ѡ�����뼸���ָ�Ĭ����ʾ��������һ��Э����n��� InfoDialogUI.Instance.Clear();
        }

        // ��ѡ�����汳��
        // inventory.Save();

        if (SlotUIController.Instance)
            SlotUIController.Instance.ShowPackageSlotFromPickup();


        // ��������
        if (destroyOnPickup)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}
