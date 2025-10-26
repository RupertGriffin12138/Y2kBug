using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class PickupItem2D : MonoBehaviour
{
    [Header("ʰȡ����")]
    [Tooltip("ItemDB �����Ʒ id")]
    public string itemId = "sparkler";
    public int amount = 1;
    public KeyCode pickupKey = KeyCode.E;
    [Tooltip("ʰȡ�����ٸ����壻����� SetActive(false)")]
    public bool destroyOnPickup = true;

    [Header("��ʾUI����ѡ��")]
    [TextArea] public string promptString = "�� <b>E</b> ʰȡ";

    [Header("���ã��������Զ��ң�")]
    public InventoryLite inventory;   // �����ֶ���ק�����ڳ������Զ� Find
    public ItemDB itemDB;             // �����裬������ inventory.itemDB

    [Header("Save")]
    [Tooltip("�������ȶ� id ���ܿ������������")]
    public SaveTag tag;
    [Tooltip("ʰȡ���Ƿ��Զ����棨�Ƽ�����ѡ��")]
    public bool autoSaveOnPickup = true;

    bool _playerInRange = false;
    bool _consumed = false;           // ��ֹ�ظ�ִ��

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
        tag = GetComponent<SaveTag>(); // �����Զ�����
    }

    void Start()
    {
        // �Զ�������
        if (!inventory) inventory = FindObjectOfType<InventoryLite>();
        if (!itemDB && inventory) itemDB = inventory.itemDB;

        // ȷ�� GameState ���ã���ֹ�����˵�û�߹�����·��ʱΪ null��
        if (GameState.Current == null)
            GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

        // ����Ӧ�ã����ö����ѱ����ã�ֱ�����ز����ٹ���
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

        // ����У��
        if (string.IsNullOrWhiteSpace(itemId) || amount == 0)
        {
            Debug.LogWarning($"[PickupItem2D] ��Ч�� itemId ��������{itemId}, {amount}", this);
            return;
        }
        if (!inventory)
        {
            Debug.LogWarning("[PickupItem2D] δ�ҵ� InventoryLite���޷�ʰȡ��", this);
            return;
        }

        // 1) �ȸ��±������������߼���
        int newCount = inventory.Add(itemId, amount);

        // 2) ����չʾ����
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

        // 3) ���� д�� GameState�����ǡ��浵�����������ݡ�������
        // ����
        GameState.AddItem(itemId, amount);
        // һ���Զ��󣺼�������б��´ζ������ٳ��֣�
        if (tag && !string.IsNullOrEmpty(tag.id))
            GameState.AddDisabledObject(tag.id);
        // ��¼��ǰλ�õĳ����������������Ϸ�ص��˹أ�
        GameState.Current.lastScene = SceneManager.GetActiveScene().name;

        // 4) ����ѡ����������
        if (autoSaveOnPickup)
            GameState.SaveNow();

        // 5) ��������������
        _consumed = true;
        if (destroyOnPickup) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}
