using System.Collections.Generic;
using UnityEngine;

public class InventoryUILite_Package : MonoBehaviour
{
    [Header("数据源")]
    public InventoryLite inventory;                   // 指向玩家的 InventoryLite
    [Header("格子 (Button01们)")]
    public List<InventorySlotViewLite> slots = new List<InventorySlotViewLite>();

    [Header("调试")]
    public bool warnOnMissingItem = true; // 找不到 id 或 icon 时打印一次警告

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

    /// <summary>在运行时更换数据源时可用。</summary>
    public void Bind(InventoryLite inv)
    {
        if (inventory != null) inventory.OnChanged -= Refresh;
        inventory = inv;
        if (inventory != null) inventory.OnChanged += Refresh;
        Refresh();
    }

    /// <summary>把 InventoryLite.entries 映射到 UI 槽位。</summary>
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
                        // 传 icon、数量、展示名（缺省用 id 兜底）
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
            // 只要数据不合法或数量<=0，统一清空
            slots[i].Clear();
        }
    }
}
