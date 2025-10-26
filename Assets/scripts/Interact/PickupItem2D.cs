using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

    [Header("Save")]
    public SaveTag tag;               // ��ѡ��û�����Կ�ʰȡ�����޷����������

    bool _playerInRange = false;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
        tag = GetComponent<SaveTag>(); // �����Զ�����
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

        // === ����Ӧ�ã����ѱ����ã���ֱ�������Լ� ===
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
        if (!inventory) return;

        // 1) ���� + �����е��߼�
        int newCount = inventory.Add(itemId, amount);

        string displayName = itemId;
        if (itemDB)
        {
            var def = itemDB.Get(itemId);
            if (def != null && !string.IsNullOrWhiteSpace(def.displayName))
                displayName = def.displayName;
        }
        if (InfoDialogUI.Instance)
            InfoDialogUI.Instance.ShowMessage($"��� {displayName} x{amount}");

        if (SlotUIController.Instance)
            SlotUIController.Instance.ShowPackageSlotFromPickup();

        // 2) === �浵��д�� GameState�����øö��� + ��¼��Ʒ�� ===
        if (tag && !string.IsNullOrEmpty(tag.id))
            GameState.AddDisabledObject(tag.id);    // �´ζ���ʱ���ٳ���
        GameState.AddItem(itemId, amount);

        // ���¡���ǰ�������������������Ϸ�ص��˹أ�
        if (GameState.Current != null)
            GameState.Current.lastScene = SceneManager.GetActiveScene().name;

        GameState.SaveNow(); // ���̴�һ�֣�Demo �򵥿ɿ���

        // 3) ��������
        if (destroyOnPickup) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}
