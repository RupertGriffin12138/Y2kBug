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

    public static SlotUIController Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        if (!fileButton || !packageButton || !fileSlot || !packageSlot)
        {
            Debug.LogError("[SlotUIController] 请在 Inspector 里把引用都拖上去。");
            return;
        }

        // 绑定 Tab 按钮
        fileButton.onClick.AddListener(ShowFileSlot);
        packageButton.onClick.AddListener(ShowPackageSlot);
    }

    void Start()
    {
        // 初始：显示 PackageSlot，隐藏 FileSlot & TextPage
        SetActiveSafe(packageSlot, true);
        SetActiveSafe(fileSlot, false);
        SetActiveSafe(textPage, false);
    }

    // --- 事件方法 ---

    public void ShowFileSlot()
    {
        SetActiveSafe(fileSlot, true);
        SetActiveSafe(packageSlot, false);

        if (closeTextPageOnTabSwitch) SetActiveSafe(textPage, false);

        // 补上点击事件绑定，避免首次打开时无响应
        if (fileSlotButtonGroup)
        {
            var slots = fileSlotButtonGroup.GetComponentsInChildren<DocSlotViewLite>(true);
            foreach (var slot in slots)
            {
                slot.OnClicked -= HandleDocClicked;
                slot.OnClicked += HandleDocClicked;
            }
        }

        Canvas.ForceUpdateCanvases(); // 确保布局即时更新
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
        // 打开文本页阅读
        SetActiveSafe(textPage, true);
    }

    /// <summary>拾起文档 → 打开文档Slot</summary>
    public void ShowFileSlotFromPickup()
    {
        ShowFileSlot();
        // 延迟一帧刷新，防止刚激活时不显示
        StartCoroutine(RefreshNextFrame(fileSlot));
    }

    /// <summary>拾起物品 → 打开物品Slot</summary>
    public void ShowPackageSlotFromPickup()
    {
        ShowPackageSlot();
        StartCoroutine(RefreshNextFrame(packageSlot));
    }

    System.Collections.IEnumerator RefreshNextFrame(GameObject go)
    {
        yield return null; // 等待 UI 激活
        if (go) Canvas.ForceUpdateCanvases();
    }

    // -----------------------
    // ▼ 小工具
    // -----------------------
    private void SetActiveSafe(GameObject go, bool active)
    {
        if (go && go.activeSelf != active) go.SetActive(active);
    }
}
