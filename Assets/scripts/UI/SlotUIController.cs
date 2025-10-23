using UnityEngine;
using UnityEngine.UI;

public class SlotUIController : MonoBehaviour
{
    [Header("Top Tabs")]
    public Button fileButton;          // Slot/FileButton
    public Button packageButton;       // Slot/PackageButton

    [Header("Panels (Slots)")]
    public GameObject fileSlot;        // Slot/FileSlot
    public GameObject packageSlot;     // Slot/PackageSlot

    [Header("Text Page")]
    public GameObject textPage;        // TextPage�������� Scroll View��
    public Transform fileSlotButtonGroup; // FileSlot/ButtonGroup�������� Button01...��

    [Header("Behavior")]
    public bool closeTextPageOnTabSwitch = true; // �л���ǩʱ�Ƿ�˳��ر� TextPage

    void Awake()
    {
        // �������п�
        if (!fileButton || !packageButton || !fileSlot || !packageSlot)
        {
            Debug.LogError("[SlotUIController] ���� Inspector ������ö�����ȥ��");
            return;
        }

        // �� Tab ��ť
        fileButton.onClick.AddListener(ShowFileSlot);
        packageButton.onClick.AddListener(ShowPackageSlot);

        // �� FileSlot/ButtonGroup ������ Button �ĵ�� �� �� TextPage
        //if (fileSlotButtonGroup)
        //{
        //    var buttons = fileSlotButtonGroup.GetComponentsInChildren<Button>(true);
        //    foreach (var btn in buttons)
        //    {
        //        btn.onClick.AddListener(OpenTextPage);
        //    }
        //}
        //else
        //{
        //    Debug.LogWarning("[SlotUIController] δ���� fileSlotButtonGroup�����޷��Զ�Ϊ���Ӱ�ť�󶨴� TextPage ���߼���");
        //}
    }

    void Start()
    {
        // ��ʼ����ʾ PackageSlot������ FileSlot & TextPage
        SetActiveSafe(packageSlot, true);
        SetActiveSafe(fileSlot, false);
        SetActiveSafe(textPage, false);
    }

    void Update()
    {
        // Esc �ر� TextPage
        //if (textPage && textPage.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        //{
        //    SetActiveSafe(textPage, false);
        //}
    }

    // --- �¼����� ---

    public void ShowFileSlot()
    {
        SetActiveSafe(fileSlot, true);
        SetActiveSafe(packageSlot, false);

        if (closeTextPageOnTabSwitch) SetActiveSafe(textPage, false);

        // ����ʾʱ��ͳһ�ѡ����Ķ���塱�Ļص���һ�飨�ݵȣ�
        if (fileSlotButtonGroup)
        {
            var slots = fileSlotButtonGroup.GetComponentsInChildren<DocSlotViewLite>(true);
            foreach (var slot in slots)
            {
                // �Ƚ�󣬱����ظ���DocSlotViewLite��� OnClicked ��¶Ϊ public event<string>��
                slot.OnClicked -= HandleDocClicked;
                slot.OnClicked += HandleDocClicked;
            }
        }
    }

    public void ShowPackageSlot()
    {
        SetActiveSafe(packageSlot, true);
        SetActiveSafe(fileSlot, false);

        if (closeTextPageOnTabSwitch) SetActiveSafe(textPage, false);
    }

    //public void OpenTextPage()
    //{
    //    SetActiveSafe(textPage, true);
    //}

    // --- С���� ---

    private void SetActiveSafe(GameObject go, bool active)
    {
        if (go && go.activeSelf != active) go.SetActive(active);
    }

    void HandleDocClicked(string docId)
    {
        // ��ԭ�� DocUILite �������£����Ķ����
        // readerPanel.OpenById(docId);
        SetActiveSafe(textPage, true); // ��������� textPage ����
    }
}
