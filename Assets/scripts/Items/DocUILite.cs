using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DocUILite : MonoBehaviour
{
    [Header("数据源")]
    public DocInventoryLite docInventory;

    [Header("UI 槽位（按钮行/格子）")]
    public List<DocSlotViewLite> slots = new List<DocSlotViewLite>();

    [Header("分页")]
    public Button pageButton;                      // FileSlot 下那个“空按钮”用作翻页
    public bool hidePageButtonIfSinglePage = true; // 仅 0/1 页时自动隐藏
    public bool loopPages = true;                  // 末页后回到第 1 页

    [Header("阅读面板")]
    public DocReaderPanel readerPanel;  // 点击后打开它

    [Header("调试")]
    public bool warnOnMissingDoc = true;

    // --- 运行时缓存 ---
    private readonly List<(string id, string displayName)> _docList = new(); // 有效可显示的文档（已拥有且能在 DocDB 找到）
    private int _currentPage = 0;  // 0-based
    private int PageSize => Mathf.Max(0, slots?.Count ?? 0);
    private int TotalPages => PageSize == 0 ? 0 : Mathf.CeilToInt((_docList.Count) / (float)PageSize);

    void OnEnable()
    {
        if (docInventory != null)
            docInventory.OnChanged += Refresh;

        HookSlotClicks();
        HookPageButton();
        Refresh();
    }

    void OnDisable()
    {
        if (docInventory != null)
            docInventory.OnChanged -= Refresh;

        UnhookSlotClicks();
        UnhookPageButton();
    }

    void HookSlotClicks()
    {
        if (slots == null) return;
        foreach (var s in slots)
            if (s != null) s.OnClicked += HandleSlotClicked;
    }

    void UnhookSlotClicks()
    {
        if (slots == null) return;
        foreach (var s in slots)
            if (s != null) s.OnClicked -= HandleSlotClicked;
    }

    void HookPageButton()
    {
        if (pageButton) pageButton.onClick.AddListener(NextPage);
    }

    void UnhookPageButton()
    {
        if (pageButton) pageButton.onClick.RemoveListener(NextPage);
    }

    void HandleSlotClicked(string docId)
    {
        if (readerPanel) readerPanel.OpenById(docId);
    }

    // -------- 对外切换数据源 --------
    public void Bind(DocInventoryLite inv)
    {
        if (docInventory != null) docInventory.OnChanged -= Refresh;
        docInventory = inv;
        if (docInventory != null) docInventory.OnChanged += Refresh;
        Refresh();
    }

    // -------- 公开刷新（重建文档列表并刷新当前页） --------
    public void Refresh()
    {
        // 先重建可显示文档列表
        RebuildDocList();

        // 页码兜底（当 doc 数减少导致当前页越界时回退到最后一页）
        if (TotalPages == 0) _currentPage = 0;
        else _currentPage = Mathf.Clamp(_currentPage, 0, TotalPages - 1);

        // 刷新 UI
        RefreshPage();

        // 翻页按钮显示策略
        if (pageButton)
        {
            bool showBtn = TotalPages > 1 || (!hidePageButtonIfSinglePage && TotalPages >= 1);
            if (pageButton.gameObject.activeSelf != showBtn)
                pageButton.gameObject.SetActive(showBtn);
        }
    }

    // -------- 重建文档列表（只做一次，分页时不要反复扫 Inventory） --------
    void RebuildDocList()
    {
        _docList.Clear();

        var entries = (docInventory ? docInventory.entries : null) ?? new List<DocInventoryLite.Entry>();
        foreach (var e in entries)
        {
            bool has = e != null && !string.IsNullOrWhiteSpace(e.id) && e.count > 0;
            if (!has) continue;

            var def = docInventory && docInventory.docDB ? docInventory.docDB.Get(e.id) : null;
            if (def != null)
            {
                string show = string.IsNullOrEmpty(def.displayName) ? def.id : def.displayName;
                _docList.Add((def.id, show));
            }
            else if (warnOnMissingDoc)
            {
                Debug.LogWarning($"[DocUI] Missing doc def for id: '{e?.id}'", this);
            }
        }

        // 如需排序（按展示名）：取消注释
        // _docList.Sort((a, b) => string.Compare(a.displayName, b.displayName, System.StringComparison.OrdinalIgnoreCase));
    }

    // -------- 只刷新当前页内容到槽位 --------
    void RefreshPage()
    {
        if (slots == null || slots.Count == 0)
        {
            return;
        }

        int start = _currentPage * PageSize;

        for (int i = 0; i < slots.Count; i++)
        {
            int idx = start + i;
            if (idx >= 0 && idx < _docList.Count)
            {
                var (id, name) = _docList[idx];
                slots[i].Set(id, name);
            }
            else
            {
                slots[i].Clear();
            }
        }
    }

    // -------- 翻页（循环） --------
    void NextPage()
    {
        if (TotalPages <= 0) return;

        int next = _currentPage + 1;
        if (next >= TotalPages)
            next = loopPages ? 0 : TotalPages - 1;

        if (next != _currentPage)
        {
            _currentPage = next;
            // 为保险，翻页后强制刷新一次布局，避免“激活当帧丢第一次点击”的概率问题
            Canvas.ForceUpdateCanvases();
            RefreshPage();
        }
        else
        {
            // 非循环并且已在最后一页：仍刷新一次，保证 UI 正常
            RefreshPage();
        }
    }
}
