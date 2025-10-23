using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DocUILite : MonoBehaviour
{
    [Header("����Դ")]
    public DocInventoryLite docInventory;

    [Header("UI ��λ����ť��/���ӣ�")]
    public List<DocSlotViewLite> slots = new List<DocSlotViewLite>();

    [Header("��ҳ")]
    public Button pageButton;                      // FileSlot ���Ǹ����հ�ť��������ҳ
    public bool hidePageButtonIfSinglePage = true; // �� 0/1 ҳʱ�Զ�����
    public bool loopPages = true;                  // ĩҳ��ص��� 1 ҳ

    [Header("�Ķ����")]
    public DocReaderPanel readerPanel;  // ��������

    [Header("����")]
    public bool warnOnMissingDoc = true;

    // --- ����ʱ���� ---
    private readonly List<(string id, string displayName)> _docList = new(); // ��Ч����ʾ���ĵ�����ӵ�������� DocDB �ҵ���
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

    // -------- �����л�����Դ --------
    public void Bind(DocInventoryLite inv)
    {
        if (docInventory != null) docInventory.OnChanged -= Refresh;
        docInventory = inv;
        if (docInventory != null) docInventory.OnChanged += Refresh;
        Refresh();
    }

    // -------- ����ˢ�£��ؽ��ĵ��б�ˢ�µ�ǰҳ�� --------
    public void Refresh()
    {
        // ���ؽ�����ʾ�ĵ��б�
        RebuildDocList();

        // ҳ�붵�ף��� doc �����ٵ��µ�ǰҳԽ��ʱ���˵����һҳ��
        if (TotalPages == 0) _currentPage = 0;
        else _currentPage = Mathf.Clamp(_currentPage, 0, TotalPages - 1);

        // ˢ�� UI
        RefreshPage();

        // ��ҳ��ť��ʾ����
        if (pageButton)
        {
            bool showBtn = TotalPages > 1 || (!hidePageButtonIfSinglePage && TotalPages >= 1);
            if (pageButton.gameObject.activeSelf != showBtn)
                pageButton.gameObject.SetActive(showBtn);
        }
    }

    // -------- �ؽ��ĵ��б�ֻ��һ�Σ���ҳʱ��Ҫ����ɨ Inventory�� --------
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

        // �������򣨰�չʾ������ȡ��ע��
        // _docList.Sort((a, b) => string.Compare(a.displayName, b.displayName, System.StringComparison.OrdinalIgnoreCase));
    }

    // -------- ֻˢ�µ�ǰҳ���ݵ���λ --------
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

    // -------- ��ҳ��ѭ���� --------
    void NextPage()
    {
        if (TotalPages <= 0) return;

        int next = _currentPage + 1;
        if (next >= TotalPages)
            next = loopPages ? 0 : TotalPages - 1;

        if (next != _currentPage)
        {
            _currentPage = next;
            // Ϊ���գ���ҳ��ǿ��ˢ��һ�β��֣����⡰���֡����һ�ε�����ĸ�������
            Canvas.ForceUpdateCanvases();
            RefreshPage();
        }
        else
        {
            // ��ѭ�������������һҳ����ˢ��һ�Σ���֤ UI ����
            RefreshPage();
        }
    }
}
