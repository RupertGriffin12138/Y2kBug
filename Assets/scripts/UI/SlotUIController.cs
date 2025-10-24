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

    public static SlotUIController Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        if (!fileButton || !packageButton || !fileSlot || !packageSlot)
        {
            Debug.LogError("[SlotUIController] ���� Inspector ������ö�����ȥ��");
            return;
        }

        // �� Tab ��ť
        fileButton.onClick.AddListener(ShowFileSlot);
        packageButton.onClick.AddListener(ShowPackageSlot);
    }

    void Start()
    {
        // ��ʼ����ʾ PackageSlot������ FileSlot & TextPage
        SetActiveSafe(packageSlot, true);
        SetActiveSafe(fileSlot, false);
        SetActiveSafe(textPage, false);
    }

    // --- �¼����� ---

    public void ShowFileSlot()
    {
        SetActiveSafe(fileSlot, true);
        SetActiveSafe(packageSlot, false);

        if (closeTextPageOnTabSwitch) SetActiveSafe(textPage, false);

        // ���ϵ���¼��󶨣������״δ�ʱ����Ӧ
        if (fileSlotButtonGroup)
        {
            var slots = fileSlotButtonGroup.GetComponentsInChildren<DocSlotViewLite>(true);
            foreach (var slot in slots)
            {
                slot.OnClicked -= HandleDocClicked;
                slot.OnClicked += HandleDocClicked;
            }
        }

        Canvas.ForceUpdateCanvases(); // ȷ�����ּ�ʱ����
    }

    public void ShowPackageSlot()
    {
        SetActiveSafe(packageSlot, true);
        SetActiveSafe(fileSlot, false);

        if (closeTextPageOnTabSwitch) SetActiveSafe(textPage, false);
        Canvas.ForceUpdateCanvases();
    }

    void HandleDocClicked(string docId)
    {
        // ���ı�ҳ�Ķ�
        SetActiveSafe(textPage, true);
    }

    /// <summary>ʰ���ĵ� �� ���ĵ�Slot</summary>
    public void ShowFileSlotFromPickup()
    {
        ShowFileSlot();
        // �ӳ�һ֡ˢ�£���ֹ�ռ���ʱ����ʾ
        StartCoroutine(RefreshNextFrame(fileSlot));
    }

    /// <summary>ʰ����Ʒ �� ����ƷSlot</summary>
    public void ShowPackageSlotFromPickup()
    {
        ShowPackageSlot();
        StartCoroutine(RefreshNextFrame(packageSlot));
    }

    System.Collections.IEnumerator RefreshNextFrame(GameObject go)
    {
        yield return null; // �ȴ� UI ����
        if (go) Canvas.ForceUpdateCanvases();
    }

    // -----------------------
    // �� С����
    // -----------------------
    private void SetActiveSafe(GameObject go, bool active)
    {
        if (go && go.activeSelf != active) go.SetActive(active);
    }
}
