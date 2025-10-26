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
    public string docId = "note1";          // ������ DocDB �� id һ��
    public bool openReaderOnPickup = true;     // ʰȡ���������Ķ����
    public bool destroyAfterPickup = false;    // ��¼���Ƿ��Ƴ������е�ֽ��

    [Header("����")]
    public KeyCode pickupKey = KeyCode.E;

    [Header("��ʾUI������ռ䣩")]
    //public GameObject promptRoot;              // ͷ��СCanvas
    //public TMP_Text promptText;                // ����E�Ķ�/��¼��
    [TextArea] public string promptString = "�� <b>E</b> �Ķ�/��¼";

    [Header("���ã��������Զ��ң�")]
    public DocInventoryLite docInventory;      // ������� DocInventoryLite
    public DocDB docDB;                        // �����裬���ȴ� docInventory.docDB ��ȡ
    public DocReaderPanel readerPanel;         // ��ѡ���Ķ���壨�����ṩʾ����

    [Header("��ѡ��Ч")]
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
        if (other.CompareTag("Player"))
        {
            _inRange = true;

            // ��ʾ��ʾ���·� InfoDialogUI
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.ShowMessage(promptString);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _inRange = false;

            // �뿪��ָ�Ĭ����ʾ
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
            Debug.LogWarning("[PickupDoc2D] δ�ҵ� DocInventoryLite��", this);
            return;
        }

        var def = docDB ? docDB.Get(docId) : null;
        string display = def != null && !string.IsNullOrWhiteSpace(def.displayName)
                         ? def.displayName : docId;

        bool isNew = docInventory.AddOnce(docId);   // �������ظ����

        // ��ʾ
        if (InfoDialogUI.Instance)
        {
            string msg = isNew ? $"��á�{display}��" : $"����¼��{display}��";
            InfoDialogUI.Instance.ShowMessage(msg);
            InfoDialogUI.Instance.CancelInvoke(nameof(InfoDialogUI.Clear));
            InfoDialogUI.Instance.Invoke(nameof(InfoDialogUI.Clear), 2f);
        }

        // ��Ч
        if (sfxSource && pickupSfx) sfxSource.PlayOneShot(pickupSfx);

        if (SlotUIController.Instance)
            SlotUIController.Instance.ShowFileSlotFromPickup();

        // === �浵��д�� GameState����¼ + ����ѡ������ʵ�壩 ===
        GameState.CollectDoc(docId); // �ĵ����롰����¼��
        // ������ĵ�ʵ��ֻ����һ�Σ�����ֱ�ӽ�������
        if (tag && !string.IsNullOrEmpty(tag.id))
            GameState.AddDisabledObject(tag.id);

        // ��¼��ǰ�����������ڼ�����Ϸ�ص�����
        if (GameState.Current != null)
            GameState.Current.lastScene = SceneManager.GetActiveScene().name;

        // ���Ķ���壨������ԭ�������̣�
        if (openReaderOnPickup && readerPanel && def != null)
        {
            if (SlotUIController.Instance)
                SlotUIController.Instance.StartCoroutine(OpenReaderStable(def));
            else
                StartCoroutine(OpenReaderStable(def));
        }

        // ���̱���
        GameState.SaveNow();

        // �������е�ֽ��
        if (destroyAfterPickup) Destroy(gameObject);
        // ��������٣����Ա�������״̬���´ζ���ʱ��������б�����ٳ���
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

    // ������Ķ��������������Ѷ�������ֱ�ӵ������
    public void MarkReadNow()
    {
        if (!string.IsNullOrEmpty(docId))
        {
            GameState.MarkDocRead(docId);
            GameState.SaveNow();
        }
    }
}
