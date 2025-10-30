using System;
using System.Collections.Generic;
using UnityEngine;

namespace Save
{
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

        // ------------------------------------------------------------------
        //  以下是运行时存档修改（全部移除 ArrayUtility）
        // ------------------------------------------------------------------

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
            if (IsObjectDisabled(id)) return;

            var list = new List<string>(Current.disabledObjectIds);
            list.Add(id);
            Current.disabledObjectIds = list.ToArray();
        }

        public static void AddItem(string itemId, int count)
        {
            if (string.IsNullOrEmpty(itemId) || count == 0) return;
            if (Current == null) Current = SaveManager.CreateDefault();

            var ids = new List<string>(Current.inventoryIds);
            var counts = new List<int>(Current.inventoryCounts);

            int idx = ids.IndexOf(itemId);
            if (idx >= 0)
            {
                counts[idx] += count;
                if (counts[idx] <= 0)
                {
                    ids.RemoveAt(idx);
                    counts.RemoveAt(idx);
                }
            }
            else if (count > 0)
            {
                ids.Add(itemId);
                counts.Add(count);
            }

            Current.inventoryIds = ids.ToArray();
            Current.inventoryCounts = counts.ToArray();
        }

        public static void CollectDoc(string docId)
        {
            if (string.IsNullOrEmpty(docId)) return;
            if (Current == null) Current = SaveManager.CreateDefault();

            var list = new List<string>(Current.docCollectedIds);
            if (!list.Contains(docId))
            {
                list.Add(docId);
                Current.docCollectedIds = list.ToArray();
            }
        }

        public static void MarkDocRead(string docId)
        {
            if (string.IsNullOrEmpty(docId)) return;
            if (Current == null) Current = SaveManager.CreateDefault();

            var list = new List<string>(Current.docReadIds);
            if (!list.Contains(docId))
            {
                list.Add(docId);
                Current.docReadIds = list.ToArray();
            }
        }

        public static void ReplaceWith(SaveData data)
        {
            Current = data;
        }

        public static bool BackpackUnlocked => Current != null && Current.backpackUnlocked;

        public static bool UnlockBackpack(bool autosave = true)
        {
            if (Current == null) Current = SaveManager.CreateDefault();
            if (Current.backpackUnlocked) return false;
            Current.backpackUnlocked = true;
            if (autosave) SaveManager.Save(Current);
            return true;
        }

        // ======================================================================
        // === 通用检测 API（全局条件判定） ===
        // ======================================================================

        public static bool HasBackpackUnlocked()
        {
            return Current != null && Current.backpackUnlocked;
        }

        public static bool HasItem(string itemId, int needed = 1)
        {
            if (string.IsNullOrEmpty(itemId) || needed <= 0 || Current == null) return false;
            for (int i = 0; i < Current.inventoryIds.Length; i++)
            {
                if (Current.inventoryIds[i] == itemId && Current.inventoryCounts[i] >= needed)
                    return true;
            }
            return false;
        }

        public static bool HasCollectedDoc(string docId)
        {
            if (string.IsNullOrEmpty(docId) || Current == null) return false;
            return Array.IndexOf(Current.docCollectedIds, docId) >= 0;
        }

        public static bool HasReadDoc(string docId)
        {
            if (string.IsNullOrEmpty(docId) || Current == null) return false;
            return Array.IndexOf(Current.docReadIds, docId) >= 0;
        }

        public static bool HasSeenDialogue(string dialogueId)
        {
            if (string.IsNullOrEmpty(dialogueId) || Current == null) return false;
            return Array.IndexOf(Current.dialogueSeenIds, dialogueId) >= 0;
        }

        public static void RemoveItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || Current == null) return;
            var ids = new List<string>(Current.inventoryIds);
            var counts = new List<int>(Current.inventoryCounts);
            int idx = ids.IndexOf(itemId);
            if (idx >= 0)
            {
                ids.RemoveAt(idx);
                counts.RemoveAt(idx);
                Current.inventoryIds = ids.ToArray();
                Current.inventoryCounts = counts.ToArray();
#if UNITY_EDITOR
                Debug.Log($"[GameState] 已彻底删除物品：{itemId}");
#endif
            }
        }
    }
}
