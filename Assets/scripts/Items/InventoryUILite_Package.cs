using System.Collections.Generic;
using UnityEngine;

public class InventoryUILite_Package : MonoBehaviour
{
    [Header("����Դ")]
    public InventoryLite inventory;                   // ָ����ҵ� InventoryLite
    [Header("���� (Button01��)")]
    public List<InventorySlotViewLite> slots = new List<InventorySlotViewLite>();

    [Header("����")]
    public bool warnOnMissingItem = true; // �Ҳ��� id �� icon ʱ��ӡһ�ξ���

    void OnEnable()
    {
        if (inventory != null)
            inventory.OnChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (inventory != null)
            inventory.OnChanged -= Refresh;
    }

    /// <summary>������ʱ��������Դʱ���á�</summary>
    public void Bind(InventoryLite inv)
    {
        if (inventory != null) inventory.OnChanged -= Refresh;
        inventory = inv;
        if (inventory != null) inventory.OnChanged += Refresh;
        Refresh();
    }

    /// <summary>�� InventoryLite.entries ӳ�䵽 UI ��λ��</summary>
    public void Refresh()
    {
        if (slots == null || slots.Count == 0) return;

        var entries = (inventory ? inventory.entries : null) ?? new List<InventoryLite.Entry>();

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < entries.Count)
            {
                var e = entries[i];
                bool hasData = !string.IsNullOrWhiteSpace(e.id) && e.count > 0;
                if (hasData)
                {
                    ItemDB.ItemDef def = inventory.itemDB ? inventory.itemDB.Get(e.id) : null;
                    if (def != null && def.icon != null)
                    {
                        // �� icon��������չʾ����ȱʡ�� id ���ף�
                        string displayName = !string.IsNullOrEmpty(def.displayName) ? def.displayName : e.id;
                        slots[i].Set(def.icon, e.count, displayName);
                        continue;
                    }
                    else if (warnOnMissingItem)
                    {
                        Debug.LogWarning($"[InventoryUI] Missing def/icon for id: '{e.id}'", this);
                    }
                }
            }
            // ֻҪ���ݲ��Ϸ�������<=0��ͳһ���
            slots[i].Clear();
        }
    }
}
