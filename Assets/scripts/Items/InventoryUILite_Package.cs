using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryUILite_Package : MonoBehaviour
{
    [Header("数据源")]
    public InventoryLite inventory;                   // 指向玩家的 InventoryLite
    public ItemDB itemDB;                               // 可留空，默认取 inventory.itemDB

    [Header("格子 (Button01们)")]
    public List<InventorySlotViewLite> slots = new List<InventorySlotViewLite>();

    [Header("显示设置")]
    [Tooltip("未解锁的格子是否直接隐藏；取消勾选时请自行在 Prefab 上做置灰表现")]
    public bool hideLockedSlots = true;            // [MOD] 简化：默认隐藏未解锁格子

    [Header("调试")]
    public bool warnOnMissingItem = true;

    //[Header("Debug")]
    //public bool debugLog = true; // [DBG]

    void OnEnable()
    {
        if (!inventory) inventory = FindObjectOfType<InventoryLite>();
        if (!itemDB && inventory) itemDB = inventory.itemDB;

        if (inventory) inventory.OnChanged += Refresh;     // [MOD]

        //if (debugLog)
        //{
        //    Debug.Log($"[PackageUI.OnEnable] this={GetInstanceID()}", this);
        //    if (inventory)
        //    {
        //        Debug.Log($"[PackageUI.OnEnable] inv={inventory.GetInstanceID()}, unlocked={inventory.UnlockedSlotCount}, isUnlocked={inventory.IsBackpackUnlocked}", inventory);
        //        if (inventory.progress)
        //            Debug.Log($"[PackageUI.OnEnable] progress id={inventory.progress.GetInstanceID()}, unlocked={inventory.progress.backpackUnlocked}", inventory.progress);
        //        else
        //            Debug.LogWarning("[PackageUI.OnEnable] inventory.progress is NULL", this);
        //    }
        //    else
        //    {
        //        Debug.LogWarning("[PackageUI.OnEnable] inventory is NULL", this);
        //    }
        //}
        Refresh();
    }

    void OnDisable()
    {
        if (inventory) inventory.OnChanged -= Refresh;     // [MOD]
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

        var inv = inventory;
        var db = itemDB ? itemDB : (inv ? inv.itemDB : null);
        var entries = inv ? inv.entries : new List<InventoryLite.Entry>();
        int unlocked = inv ? inv.UnlockedSlotCount : 1;     // [MOD]

        //if (debugLog)
        //    Debug.Log($"[PackageUI.Refresh] unlocked={unlocked}, entries={entries.Count}, slots={slots.Count}", this);


        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (!slot) continue;

            bool isUnlocked = (i < unlocked);

            // [MOD] 未解锁格子处理：隐藏或保留给你自定义的置灰表现
            if (hideLockedSlots)
            {
                slot.gameObject.SetActive(isUnlocked);
                if (!isUnlocked) continue;
            }
            else
            {
                // 不隐藏时，这里不做置灰以免干扰你现有的视觉；如果需要置灰，可在 Prefab 上加 CanvasGroup，并在这里设置 alpha/interactable
                slot.gameObject.SetActive(true);
            }

            // 显示数据：按 entries 的顺序逐个填
            if (i < entries.Count)
            {
                var e = entries[i];
                bool hasData = !string.IsNullOrWhiteSpace(e.id) && e.count > 0;

                if (hasData)
                {
                    Sprite icon = null;
                    string displayName = e.id;

                    if (db)
                    {
                        var def = db.Get(e.id);
                        if (def != null)
                        {
                            if (def.icon) icon = def.icon;
                            if (!string.IsNullOrWhiteSpace(def.displayName)) displayName = def.displayName;
                        }
                        else if (warnOnMissingItem)
                        {
                            Debug.LogWarning($"[InventoryUI] Missing def for id: '{e.id}'", this);
                        }
                    }

                    slot.Set(icon, e.count, displayName);   // [MOD] 适配你的 Set(sprite,count,displayName)
                    continue;
                }
            }

            // 无数据或超界：清空
            slot.Clear();
        }
    }
}