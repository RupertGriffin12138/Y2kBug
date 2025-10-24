using UnityEngine;
using TMPro;
using UnityEngine.UI;              // ScrollRect
using UnityEngine.EventSystems;
using System.Collections;

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
    public GameObject promptRoot;              // ͷ��СCanvas
    public TMP_Text promptText;                // ����E�Ķ�/��¼��
    [TextArea] public string promptString = "�� <b>E</b> �Ķ�/��¼";

    [Header("���ã��������Զ��ң�")]
    public DocInventoryLite docInventory;      // ������� DocInventoryLite
    public DocDB docDB;                        // �����裬���ȴ� docInventory.docDB ��ȡ
    public DocReaderPanel readerPanel;         // ��ѡ���Ķ���壨�����ṩʾ����

    [Header("��ѡ��Ч")]
    public AudioSource sfxSource;
    public AudioClip pickupSfx;

    bool _inRange;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void Awake()
    {
        if (promptRoot) promptRoot.SetActive(false);
    }

    void Start()
    {
        if (!docInventory) docInventory = FindObjectOfType<DocInventoryLite>();
        if (!docDB && docInventory) docDB = docInventory.docDB;
        if (!readerPanel) readerPanel = FindObjectOfType<DocReaderPanel>(true);

        if (promptText && !string.IsNullOrWhiteSpace(promptString))
        {
            promptText.text = promptString;
            // ������ߣ���ǿ�ɶ��ԣ��ɰ���΢����
            var mat = promptText.fontMaterial;
            if (mat)
            {
                mat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.2f);
                mat.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
                mat.SetFloat(ShaderUtilities.ID_FaceDilate, 0.12f);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _inRange = true;
            if (promptRoot) promptRoot.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _inRange = false;
            if (promptRoot) promptRoot.SetActive(false);
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

        // ��ʾ�������/����¼��xxx����
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

        // ���Ķ����
        if (openReaderOnPickup && readerPanel && def != null)
        {
            // ����� UI ������������������Э�̣������������жϣ�
            if (SlotUIController.Instance)
                SlotUIController.Instance.StartCoroutine(OpenReaderStable(def));
            else
                StartCoroutine(OpenReaderStable(def));
        }

        // �������е�ֽ��
        if (destroyAfterPickup) Destroy(gameObject);
        else if (promptRoot) promptRoot.SetActive(false); // �����پ�������ʾ
    }

    IEnumerator OpenReaderStable(DocDB.DocDef def)
    {
        // 1) ��ȷ����崦�ڼ���״̬����Щ������δ����ʱ���Ὠ����
        if (readerPanel.rootPanel && !readerPanel.rootPanel.activeSelf)
            readerPanel.rootPanel.SetActive(true);

        // 2) ����ǿˢһ�� Canvas��������֡����
        Canvas.ForceUpdateCanvases();

        // 3) �ȴ�����һ֡���ü���/����������Ч��
        yield return null;

        // 4) �����򿪲�������ݣ���ʱ UI �Ѿ� ready��
        readerPanel.Open(def);

        // 5) ��ǿˢһ�Σ������״������ı����ӳ�
        Canvas.ForceUpdateCanvases();

        // 6) �ٵ�һ֡��ȷ�� ScrollRect ����ȷ��λ��������һЩ�汾��Ҫ��
        yield return null;

        // 7) ���ף��ѹ������ƻض������� Open ������������Ҳ�޷���
        var scrollRect = readerPanel.contentText ?
            readerPanel.contentText.GetComponentInParent<ScrollRect>() : null;
        if (scrollRect) scrollRect.normalizedPosition = new Vector2(0, 1);

        // ����ѡ���ѽ�����رհ�ť��������򣬱�����֡�����¼������ UI ����
        // EventSystem.current?.SetSelectedGameObject(scrollRect?.gameObject);
    }
}
