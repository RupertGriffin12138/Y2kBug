using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryLite : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public string id;
        public int count = 1;
    }

    [Serializable]
    public class SaveData
    {
        public List<Entry> entries = new List<Entry>();
    }

    [Header("数据")]
    public List<Entry> entries = new List<Entry>(); // 运行时数据（Inspector 可直改）
    public ItemDB itemDB;

    // ====================== 容量/解锁（新增：由全局变量控制） ======================
    [Header("容量设置")]
    [Tooltip("背包的总槽位数量（UI 总格子数）")]
    // [MOD] 总槽位数（解锁后显示的最大格子数）
    public int totalSlotCount = 6;

    [Header("全局进度（跨场景生效）")]
    [Tooltip("拖入一个全局 ScriptableObject（如 GameProgress.asset），其 backpackUnlocked 为全局解锁标志")]
    // [MOD] 全局解锁状态引用（ScriptableObject），跨场景共享
    public GameProgress progress;

    // [MOD] 当前可用槽位数：未解锁=1，解锁=totalSlotCount
    public int UnlockedSlotCount => Mathf.Clamp(
        (progress != null && progress.backpackUnlocked) ? totalSlotCount : 1,
        1, Mathf.Max(1, totalSlotCount)
    );

    // [MOD]（可选）对外暴露：是否已经解锁
    public bool IsBackpackUnlocked => (progress != null && progress.backpackUnlocked);

    [Header("Debug")]
    public bool debugLog = true; // [DBG] 勾上即可打印

    // ==========================================================================

    public event Action OnChanged;

    const string PREFS_KEY = "save_inventory_lite";

    // —— 运行时修改API —— //
    public void SetAll(List<Entry> newEntries)
    {
        entries = newEntries ?? new List<Entry>();
        OnChanged?.Invoke();
    }

    public void SetCount(string id, int count)
    {
        if (string.IsNullOrEmpty(id)) return;
        var e = entries.Find(x => string.Equals(x.id, id, StringComparison.OrdinalIgnoreCase));
        if (e == null)
        {
            if (count > 0)
            {
                // [MOD] 新建堆叠前，也要遵守容量限制
                if (!CanCreateNewStack())
                {
                    // 容量不足则不创建，直接返回
                    return;
                }
                entries.Add(new Entry { id = id, count = count });
            }
        }
        else
        {
            e.count = count;
            if (e.count <= 0) entries.Remove(e);
        }
        OnChanged?.Invoke();
    }

    // —— 存档/读档 —— //
    public void Save()
    {
        var data = new SaveData { entries = entries };
        var json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(PREFS_KEY, json);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey(PREFS_KEY)) return;
        var json = PlayerPrefs.GetString(PREFS_KEY);
        var data = JsonUtility.FromJson<SaveData>(json);
        entries = data?.entries ?? new List<Entry>();
        OnChanged?.Invoke();
    }

    // 你可以在 Start 时尝试读档
    void Start()
    {
        Load();

        // [DBG] 建议：先临时注释这行，避免多点 Load 干扰判断
        // if (progress != null) { progress.Load(); }

        if (debugLog)
        {
            Debug.Log($"[InventoryLite.Start] this={GetInstanceID()}, entries={entries?.Count ?? 0}", this);
            if (progress)
                Debug.Log($"[InventoryLite.Start] progress id={progress.GetInstanceID()}, unlocked={progress.backpackUnlocked}", progress);
            else
                Debug.LogWarning("[InventoryLite.Start] progress is NULL", this);
        }

        // 小延迟后再打一次，看是否被别人改写（时序问题）
        Invoke(nameof(_DBG_LogAfterOneFrame), 0f);
    }

    void _DBG_LogAfterOneFrame()
    {
        if (debugLog)
        {
            Debug.Log($"[InventoryLite.PostFrame] unlockedSlotCount={UnlockedSlotCount}, occupied={GetOccupiedStacks()}", this);
        }
    }

    // 可调用的方法

    // —— 辅助：按 id 找到条目（忽略大小写），找不到返回 null —— //
    InventoryLite.Entry FindEntry(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return entries.Find(e => string.Equals(e.id, id, System.StringComparison.OrdinalIgnoreCase));
    }

    // —— 查询：是否至少拥有 needed 个 —— //
    public bool Has(string id, int needed = 1)
    {
        if (string.IsNullOrWhiteSpace(id) || needed <= 0) return false;
        var e = FindEntry(id);
        return e != null && e.count >= needed;
    }

    // —— 查询：返回当前数量，若无则 0 —— //
    public int GetCount(string id)
    {
        var e = FindEntry(id);
        return e != null ? Mathf.Max(0, e.count) : 0;
    }

    // —— 添加：把 amount 加到 id 上（不存在则创建），返回最终数量 —— //
    public int Add(string id, int amount = 1)
    {
        // [DBG]
        if (debugLog)
            Debug.Log($"[InventoryLite.Add] id={id}, amount={amount}, canCreateNew={CanCreateNewStack()}, unlocked={UnlockedSlotCount}", this);

        if (string.IsNullOrWhiteSpace(id) || amount == 0) return GetCount(id);

        var e = FindEntry(id);
        if (e == null)
        {
            if (amount > 0)
            {
                // [MOD] 仅当还有“可用的已解锁槽位”时，才允许新建堆叠
                if (!CanCreateNewStack())
                {
                    if (debugLog) Debug.LogWarning("[InventoryLite.Add] capacity full, skip create new stack", this);
                    return 0;
                }
                entries.Add(new Entry { id = id, count = amount });
            }
            // amount < 0 且条目不存在，则视为 0 基础上减，不创建条目（保持原逻辑）
        }
        else
        {
            e.count += amount;
            if (e.count <= 0)
            {
                entries.Remove(e);
                OnChanged?.Invoke();
                return 0;
            }
        }

        OnChanged?.Invoke();
        return GetCount(id);
    }

    // —— 删除：尝试减少 amount；不够则失败并不改动。成功返回 true —— //
    public bool Remove(string id, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(id) || amount <= 0) return false;

        var e = FindEntry(id);
        if (e == null || e.count < amount) return false;

        e.count -= amount;
        if (e.count <= 0) entries.Remove(e);

        OnChanged?.Invoke();
        return true;
    }

    // —— 消耗：若拥有 needed 数量则扣减并返回 true，否则 false（常用于“使用物品”） —— //
    public bool TryConsume(string id, int needed = 1)
    {
        if (!Has(id, needed)) return false;
        return Remove(id, needed);
    }

    // —— 清空全部 —— //
    public void ClearAll()
    {
        entries.Clear();
        OnChanged?.Invoke();
    }

    /// <summary>
    /// [MOD] 当前占用的“堆叠数”（用于与 UnlockedSlotCount 比较）。
    /// 注意：这里以“有效条目数”作为占用槽位数，与你的 UI 一一对应。
    /// </summary>
    public int GetOccupiedStacks()
    {
        int occupied = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(entries[i].id) && entries[i].count > 0)
                occupied++;
        }
        return occupied;
    }

    /// <summary>
    /// [MOD] 是否允许创建一个新的堆叠（即是否还有未被占用的“已解锁槽位”）
    /// </summary>
    public bool CanCreateNewStack()
    {
        return GetOccupiedStacks() < UnlockedSlotCount;
    }

    /// <summary>
    /// [MOD] 当全局解锁状态被外部改变（如拾取背包）时，可显式调用，触发 UI 刷新。
    /// （PickupBackpack2D 在解锁后调用 OnChanged?.Invoke() 也可以达到相同目的。）
    /// </summary>
    public void NotifyCapacityChanged()
    {
        if (debugLog)
        {
            Debug.Log($"[InventoryLite.NotifyCapacityChanged] unlocked={UnlockedSlotCount}, isBackpackUnlocked={IsBackpackUnlocked}", this);
            if (progress)
                Debug.Log($"[InventoryLite.NotifyCapacityChanged] progress id={progress.GetInstanceID()}, unlocked={progress.backpackUnlocked}", progress);
        }
        OnChanged?.Invoke();
    }
}
