using System;
using UnityEngine;

/// <summary>
/// 运行时的唯一真相（内存态）。外部系统读写它；SaveManager 负责落盘与加载。
/// </summary>
public static class GameState
{
    public static SaveData Current { get; private set; }

    public static void NewGame(string firstScene = "Town")
    {
        Current = SaveManager.CreateDefault(firstScene);
        EnsureArraysInitialized();
    }

    public static void LoadGameOrNew(string firstScene = "Town")
    {
        Current = SaveManager.LoadOrDefault(firstScene);
        EnsureArraysInitialized();
    }

    public static void SaveNow()
    {
        if (Current == null)
        {
            Debug.LogWarning("[GameState] SaveNow 时 Current == null，自动创建默认数据。");
            Current = SaveManager.CreateDefault();
        }
        EnsureArraysInitialized();
        SaveManager.Save(Current);
    }

    public static void Wipe()
    {
        SaveManager.Wipe();
        Current = null;
    }

    // —— 下面是几个最小操作 API（后续还会扩展） ——

    public static bool IsObjectDisabled(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        if (Current?.disabledObjectIds == null) return false;
        foreach (var x in Current.disabledObjectIds)
            if (x == id) return true;
        return false;
    }

    public static void AddDisabledObject(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (Current == null) Current = SaveManager.CreateDefault();
        EnsureArraysInitialized();

        // 避免重复
        if (IsObjectDisabled(id)) return;

        ArrayUtil.Add(ref Current.disabledObjectIds, id);
    }

    public static void AddItem(string itemId, int count)
    {
        if (string.IsNullOrEmpty(itemId) || count == 0) return;
        if (Current == null) Current = SaveManager.CreateDefault();
        EnsureArraysInitialized();

        int idx = Array.IndexOf(Current.inventoryIds, itemId);
        if (idx >= 0)
        {
            Current.inventoryCounts[idx] += count;
            if (Current.inventoryCounts[idx] <= 0)
            {
                // 数量为 0 时移除
                ArrayUtil.RemoveAt(ref Current.inventoryIds, idx);
                ArrayUtil.RemoveAt(ref Current.inventoryCounts, idx);
            }
        }
        else if (count > 0)
        {
            ArrayUtil.Add(ref Current.inventoryIds, itemId);
            ArrayUtil.Add(ref Current.inventoryCounts, count);
        }
    }

    public static void CollectDoc(string docId)
    {
        if (string.IsNullOrEmpty(docId)) return;
        if (Current == null) Current = SaveManager.CreateDefault();
        EnsureArraysInitialized();

        if (Array.IndexOf(Current.docCollectedIds, docId) < 0)
            ArrayUtil.Add(ref Current.docCollectedIds, docId);
    }

    public static void MarkDocRead(string docId)
    {
        if (string.IsNullOrEmpty(docId)) return;
        if (Current == null) Current = SaveManager.CreateDefault();
        EnsureArraysInitialized();

        if (Array.IndexOf(Current.docReadIds, docId) < 0)
            ArrayUtil.Add(ref Current.docReadIds, docId);
    }

    public static void ReplaceWith(SaveData data)
    {
        Current = data;  // 这个类内部可以写入 private set 的属性
        EnsureArraysInitialized();
    }

    public static bool BackpackUnlocked
    {
        get { return Current != null && Current.backpackUnlocked; }
    }

    public static bool UnlockBackpack(bool autosave = true)
    {
        if (Current == null) Current = SaveManager.CreateDefault();
        EnsureArraysInitialized();

        if (Current.backpackUnlocked) return false;
        Current.backpackUnlocked = true;
        if (autosave) SaveManager.Save(Current);
        return true;
    }

    // ——— 保底：确保数组字段非 null，避免首次 Add 时 NRE ———
    private static void EnsureArraysInitialized()
    {
        if (Current == null) return;

        Current.disabledObjectIds ??= Array.Empty<string>();
        Current.inventoryIds ??= Array.Empty<string>();
        Current.inventoryCounts ??= Array.Empty<int>();
        Current.docCollectedIds ??= Array.Empty<string>();
        Current.docReadIds ??= Array.Empty<string>();
    }
}
