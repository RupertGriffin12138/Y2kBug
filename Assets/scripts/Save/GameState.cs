using System;
using UnityEditor;
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
    }

    public static void LoadGameOrNew(string firstScene = "Town")
    {
        Current = SaveManager.LoadOrDefault(firstScene);
    }

    public static void SaveNow()
    {
        if (Current == null)
        {
            Debug.LogWarning("[GameState] SaveNow 时 Current == null，自动创建默认数据。");
            Current = SaveManager.CreateDefault();
        }
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
        if (Current?.disabledObjectIds == null) return false;
        foreach (var x in Current.disabledObjectIds)
            if (x == id) return true;
        return false;
    }

    public static void AddDisabledObject(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (Current == null) Current = SaveManager.CreateDefault();

        // 避免重复
        if (IsObjectDisabled(id)) return;

        ArrayUtility.Add(ref Current.disabledObjectIds, id);
    }

    public static void AddItem(string itemId, int count)
    {
        if (string.IsNullOrEmpty(itemId) || count == 0) return;
        if (Current == null) Current = SaveManager.CreateDefault();

        int idx = Array.IndexOf(Current.inventoryIds, itemId);
        if (idx >= 0)
        {
            Current.inventoryCounts[idx] += count;
            if (Current.inventoryCounts[idx] <= 0)
            {
                // 数量为 0 时移除
                ArrayUtility.RemoveAt(ref Current.inventoryIds, idx);
                ArrayUtility.RemoveAt(ref Current.inventoryCounts, idx);
            }
        }
        else if (count > 0)
        {
            ArrayUtility.Add(ref Current.inventoryIds, itemId);
            ArrayUtility.Add(ref Current.inventoryCounts, count);
        }
    }

    public static void CollectDoc(string docId)
    {
        if (string.IsNullOrEmpty(docId)) return;
        if (Current == null) Current = SaveManager.CreateDefault();

        if (Array.IndexOf(Current.docCollectedIds, docId) < 0)
            ArrayUtility.Add(ref Current.docCollectedIds, docId);
    }

    public static void MarkDocRead(string docId)
    {
        if (string.IsNullOrEmpty(docId)) return;
        if (Current == null) Current = SaveManager.CreateDefault();

        if (Array.IndexOf(Current.docReadIds, docId) < 0)
            ArrayUtility.Add(ref Current.docReadIds, docId);
    }
}
