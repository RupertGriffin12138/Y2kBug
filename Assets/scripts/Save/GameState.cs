using System;
using UnityEditor;
using UnityEngine;

namespace Save
{
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

        public static void ReplaceWith(SaveData data)
        {
            Current = data;  // ������ڲ�����д�� private set ������
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
        // === ͨ�ü�� API��ȫ�������ж��� ===
        // ======================================================================

        /// <summary>�Ƿ��Ѿ�����������</summary>
        public static bool HasBackpackUnlocked()
        {
            return Current != null && Current.backpackUnlocked;
        }

        /// <summary>�Ƿ�ӵ��ָ����Ʒ��Ĭ������ 1 ������</summary>
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

        /// <summary>�Ƿ�������ù�ָ���ĵ���</summary>
        public static bool HasCollectedDoc(string docId)
        {
            if (string.IsNullOrEmpty(docId) || Current == null) return false;
            return Array.IndexOf(Current.docCollectedIds, docId) >= 0;
        }

        /// <summary>�Ƿ����Ķ�ָ���ĵ���</summary>
        public static bool HasReadDoc(string docId)
        {
            if (string.IsNullOrEmpty(docId) || Current == null) return false;
            return Array.IndexOf(Current.docReadIds, docId) >= 0;
        }

        /// <summary>�Ƿ��ѿ���ָ���Ի���</summary>
        public static bool HasSeenDialogue(string dialogueId)
        {
            if (string.IsNullOrEmpty(dialogueId) || Current == null) return false;
            return Array.IndexOf(Current.dialogueSeenIds, dialogueId) >= 0;
        }
        
        /// <summary>
        /// �Ӵ浵�г����Ƴ�ָ����Ʒ�����������Ƕ��٣���
        /// ɾ�������Ʒ��������ʾ�ڱ����С�
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
                Debug.Log($"[GameState] �ѳ���ɾ����Ʒ��{itemId}");
#endif
            }
        }


    }
}
