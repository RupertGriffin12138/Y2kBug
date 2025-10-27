using System;
using UnityEngine;

/// <summary>
/// ����ʱ��Ψһ���ࣨ�ڴ�̬�����ⲿϵͳ��д����SaveManager ������������ء�
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
            Debug.LogWarning("[GameState] SaveNow ʱ Current == null���Զ�����Ĭ�����ݡ�");
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

    // ���� �����Ǽ�����С���� API������������չ�� ����

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

        // �����ظ�
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
                // ����Ϊ 0 ʱ�Ƴ�
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
        Current = data;  // ������ڲ�����д�� private set ������
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

    // ������ ���ף�ȷ�������ֶη� null�������״� Add ʱ NRE ������
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
