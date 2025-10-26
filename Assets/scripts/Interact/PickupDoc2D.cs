using UnityEngine;
using TMPro;
using UnityEngine.UI;              // ScrollRect
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class PickupDoc2D : MonoBehaviour
{
    [Header("�ĵ�����")]
    public string docId = "note1";              // ������ DocDB �� id һ��
    public bool openReaderOnPickup = true;      // ʰȡ���������Ķ����
    public bool destroyAfterPickup = false;     // ��¼���Ƿ��Ƴ������е�ֽ��

    [Header("����")]
    public KeyCode pickupKey = KeyCode.E;

    [Header("��ʾUI������ռ䣩")]
    [TextArea] public string promptString = "�� <b>E</b> �Ķ�/��¼";

    [Header("���ã��������Զ��ң�")]
    public DocInventoryLite docInventory;      // ������� DocInventoryLite
    public DocDB docDB;                        // �����裬���ȴ� docInventory.docDB ��ȡ
    public DocReaderPanel readerPanel;         // ��ѡ���Ķ����

    [Header("��ѡ��Ч")]
    public AudioSource sfxSource;
    public AudioClip pickupSfx;

    [Header("Save")]
    public SaveTag tag;                        // һ����ʵ����ȶ� id
    public bool autoSaveOnPickup = true;       // ʰȡ���Ƿ����̴浵���Ƽ�������

    bool _inRange;
    bool _consumed;                            // ���ظ�����

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

        // ȷ���浵�ѳ�ʼ������ֹ��������ڽ��뵼�� GameState Ϊ null��
        if (GameState.Current == null)
            GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

        // ����Ӧ�ã�����ʵ���ѱ����ã���ֱ�������Լ���ʧЧ
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
            Debug.LogWarning("[PickupDoc2D] δ�ҵ� DocInventoryLite��", this);
            return;
        }

        var def = docDB ? docDB.Get(docId) : null;
        string display = def != null && !string.IsNullOrWhiteSpace(def.displayName)
                         ? def.displayName : docId;

        // 1) ��д����̬���ݿ⣨���� UI �ӳ٣�
        bool isNew = docInventory.AddOnce(docId);   // �������ظ����

        // 2) ���� д�� GameState��Ȩ���浵������
        GameState.CollectDoc(docId);                // �ĵ����롰����¼��
        if (tag && !string.IsNullOrEmpty(tag.id))   // һ����ʵ�壺��������б�
            GameState.AddDisabledObject(tag.id);
        GameState.Current.lastScene = SceneManager.GetActiveScene().name;

        // 3) UI ��ʾ����Ч
        if (InfoDialogUI.Instance)
        {
            string msg = isNew ? $"��á�{display}��" : $"����¼��{display}��";
            InfoDialogUI.Instance.ShowMessage(msg);
            InfoDialogUI.Instance.CancelInvoke(nameof(InfoDialogUI.Clear));
            InfoDialogUI.Instance.Invoke(nameof(InfoDialogUI.Clear), 2f);
        }
        if (sfxSource && pickupSfx) sfxSource.PlayOneShot(pickupSfx);
        if (SlotUIController.Instance) SlotUIController.Instance.ShowFileSlotFromPickup();

        // 4) �Ķ���壨�����Ҫ��
        if (openReaderOnPickup && readerPanel && def != null)
        {
            if (SlotUIController.Instance)
                SlotUIController.Instance.StartCoroutine(OpenReaderStable(def));
            else
                StartCoroutine(OpenReaderStable(def));
        }

        // 5) �������棨��ѡ��
        if (autoSaveOnPickup)
            GameState.SaveNow();

        // 6) ����ʵ�岢������
        _consumed = true;
        if (destroyAfterPickup) Destroy(gameObject);
        // ������ʱ���ּ���´ζ�����������б�����ٳ���
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

    // ������Ķ��������������Ѷ�������ֱ�ӵ��������ֻ���� GameState��
    public void MarkReadNow()
    {
        if (!string.IsNullOrEmpty(docId))
        {
            GameState.MarkDocRead(docId);
            GameState.SaveNow();
        }
    }
}
