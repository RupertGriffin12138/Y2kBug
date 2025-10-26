using System;
using UnityEditor;
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
    }

    public static void LoadGameOrNew(string firstScene = "Town")
    {
        Current = SaveManager.LoadOrDefault(firstScene);
    }

    public static void SaveNow()
    {
        if (Current == null)
        {
            Debug.LogWarning("[GameState] SaveNow ʱ Current == null���Զ�����Ĭ�����ݡ�");
            Current = SaveManager.CreateDefault();
        }
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
        if (Current?.disabledObjectIds == null) return false;
        foreach (var x in Current.disabledObjectIds)
            if (x == id) return true;
        return false;
    }

    public static void AddDisabledObject(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (Current == null) Current = SaveManager.CreateDefault();

        // �����ظ�
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
                // ����Ϊ 0 ʱ�Ƴ�
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
