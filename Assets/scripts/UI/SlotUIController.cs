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
    public GameObject textPage;        // TextPage（里面有 Scroll View）
    public Transform fileSlotButtonGroup; // FileSlot/ButtonGroup（其下有 Button01...）

    [Header("Behavior")]
    public bool closeTextPageOnTabSwitch = true; // 切换标签时是否顺便关闭 TextPage

    void Awake()
    {
        // 保护性判空
        if (!fileButton || !packageButton || !fileSlot || !packageSlot)
        {
            Debug.LogError("[SlotUIController] 请在 Inspector 里把引用都拖上去。");
            return;
        }

        // 绑定 Tab 按钮
        fileButton.onClick.AddListener(ShowFileSlot);
        packageButton.onClick.AddListener(ShowPackageSlot);

        // 绑定 FileSlot/ButtonGroup 下所有 Button 的点击 → 打开 TextPage
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
        //    Debug.LogWarning("[SlotUIController] 未设置 fileSlotButtonGroup，将无法自动为其子按钮绑定打开 TextPage 的逻辑。");
        //}
    }

    void Start()
    {
        // 初始：显示 PackageSlot，隐藏 FileSlot & TextPage
        SetActiveSafe(packageSlot, true);
        SetActiveSafe(fileSlot, false);
        SetActiveSafe(textPage, false);
    }

    void Update()
    {
        // Esc 关闭 TextPage
        //if (textPage && textPage.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        //{
        //    SetActiveSafe(textPage, false);
        //}
    }

    // --- 事件方法 ---

    public void ShowFileSlot()
    {
        SetActiveSafe(fileSlot, true);
        SetActiveSafe(packageSlot, false);

        if (closeTextPageOnTabSwitch) SetActiveSafe(textPage, false);

        // 新显示时，统一把“打开阅读面板”的回调补一遍（幂等）
        if (fileSlotButtonGroup)
        {
            var slots = fileSlotButtonGroup.GetComponentsInChildren<DocSlotViewLite>(true);
            foreach (var slot in slots)
            {
                // 先解绑，避免重复（DocSlotViewLite里把 OnClicked 暴露为 public event<string>）
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

    // --- 小工具 ---

    private void SetActiveSafe(GameObject go, bool active)
    {
        if (go && go.activeSelf != active) go.SetActive(active);
    }

    void HandleDocClicked(string docId)
    {
        // 你原来 DocUILite 里做的事：打开阅读面板
        // readerPanel.OpenById(docId);
        SetActiveSafe(textPage, true); // 如果你是用 textPage 开关
    }
}
