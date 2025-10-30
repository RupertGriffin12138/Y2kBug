using System;
using UnityEditor;
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

        public static void ReplaceWith(SaveData data)
        {
            Current = data;  // 这个类内部可以写入 private set 的属性
        }

        public static bool BackpackUnlocked
        {
            get { return Current != null && Current.backpackUnlocked; }
        }

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

        /// <summary>是否已经解锁背包。</summary>
        public static bool HasBackpackUnlocked()
        {
            return Current != null && Current.backpackUnlocked;
        }

        /// <summary>是否拥有指定物品（默认至少 1 个）。</summary>
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

        /// <summary>是否曾经获得过指定文档。</summary>
        public static bool HasCollectedDoc(string docId)
        {
            if (string.IsNullOrEmpty(docId) || Current == null) return false;
            return Array.IndexOf(Current.docCollectedIds, docId) >= 0;
        }

        /// <summary>是否已阅读指定文档。</summary>
        public static bool HasReadDoc(string docId)
        {
            if (string.IsNullOrEmpty(docId) || Current == null) return false;
            return Array.IndexOf(Current.docReadIds, docId) >= 0;
        }

        /// <summary>是否已看过指定对话。</summary>
        public static bool HasSeenDialogue(string dialogueId)
        {
            if (string.IsNullOrEmpty(dialogueId) || Current == null) return false;
            return Array.IndexOf(Current.dialogueSeenIds, dialogueId) >= 0;
        }
        
        /// <summary>
        /// 从存档中彻底移除指定物品（不论数量是多少）。
        /// 删除后该物品不会再显示在背包中。
        /// </summary>
        public static void RemoveItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || Current == null)
                return;

            int idx = Array.IndexOf(Current.inventoryIds, itemId);
            if (idx >= 0)
            {
                ArrayUtility.RemoveAt(ref Current.inventoryIds, idx);
                ArrayUtility.RemoveAt(ref Current.inventoryCounts, idx);
#if UNITY_EDITOR
                Debug.Log($"[GameState] 已彻底删除物品：{itemId}");
#endif
            }
        }


    }
}
