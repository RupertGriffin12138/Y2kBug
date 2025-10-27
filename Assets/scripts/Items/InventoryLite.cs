using System;
using System.Collections.Generic;
using Core;
using Save;
using UnityEngine;

namespace Items
{
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

        // ====================== 容量/解锁（由全局变量控制） ======================
        [Header("容量设置")]
        [Tooltip("背包的总槽位数量（UI 总格子数）")]
        public int totalSlotCount = 6;

        [Header("全局进度（跨场景生效）")]
        [Tooltip("拖入一个全局 ScriptableObject（如 GameProgress.asset），其 backpackUnlocked 为全局解锁标志")]
        public GameProgress progress;

        // 当前可用槽位数：未解锁=1，解锁=totalSlotCount
        public int UnlockedSlotCount => Mathf.Clamp(
            (progress != null && progress.backpackUnlocked) ? totalSlotCount : 1,
            1, Mathf.Max(1, totalSlotCount)
        );

        public bool IsBackpackUnlocked => (progress != null && progress.backpackUnlocked);

        // ====================== 存档来源选择 ======================
        [Header("存档来源")]
        [Tooltip("勾选后，Inventory 以 GameState 为唯一真实来源；不再从自身的 PlayerPrefs 读。")]
        public bool useGameStateSource = true;

        [Tooltip("从存档加载到背包时，忽略容量限制（推荐勾选）")]
        public bool ignoreCapacityWhenLoading = true;

        [Header("Debug")]
        public bool debugLog = true; // [DBG] 勾上即可打印

        public event Action OnChanged;

        const string PREFS_KEY = "save_inventory_lite";

        // 批量导入时抑制 OnChanged 频繁触发
        bool _suppressChanged = false;

        // ==========================================================================

        // —— 初始化 —— //
        void Start()
        {
            if (useGameStateSource)
            {
                LoadFromGameState();
            }
            else
            {
                Load(); // 兼容旧流程（PlayerPrefs 本地存取）
            }

            //if (debugLog)
            //{
            //    Debug.Log($"[InventoryLite.Start] this={GetInstanceID()}, entries={entries?.Count ?? 0}", this);
            //    if (progress)
            //        Debug.Log($"[InventoryLite.Start] progress id={progress.GetInstanceID()}, unlocked={progress.backpackUnlocked}", progress);
            //    else
            //        Debug.LogWarning("[InventoryLite.Start] progress is NULL", this);
            //}

            Invoke(nameof(_DBG_LogAfterOneFrame), 0f);
        }

        void _DBG_LogAfterOneFrame()
        {
            //if (debugLog)
            //{
            //    Debug.Log($"[InventoryLite.PostFrame] unlockedSlotCount={UnlockedSlotCount}, occupied={GetOccupiedStacks()}", this);
            //}
        }

        // ====================== 对外 API ======================

        /// <summary>
        /// 设置整份列表（覆盖）。
        /// </summary>
        public void SetAll(List<Entry> newEntries)
        {
            entries = newEntries ?? new List<Entry>();
            if (!_suppressChanged) OnChanged?.Invoke();
        }

        /// <summary>
        /// 直接设置指定物品数量（可为 0 以删除）。
        /// </summary>
        public void SetCount(string id, int count)
        {
            if (string.IsNullOrEmpty(id)) return;
            var e = entries.Find(x => string.Equals(x.id, id, StringComparison.OrdinalIgnoreCase));
            if (e == null)
            {
                if (count > 0)
                {
                    // 新建堆叠需检查容量（批量加载时可忽略）
                    bool needCheckCapacity = !(_suppressChanged && ignoreCapacityWhenLoading);
                    if (needCheckCapacity && !CanCreateNewStack())
                    {
                        //if (debugLog) Debug.LogWarning("[InventoryLite.SetCount] capacity full, skip create new stack", this);
                        //return;
                    }
                    entries.Add(new Entry { id = id, count = count });
                }
            }
            else
            {
                e.count = count;
                if (e.count <= 0) entries.Remove(e);
            }
            if (!_suppressChanged) OnChanged?.Invoke();
        }

        /// <summary>
        /// 是否至少拥有 needed 个。
        /// </summary>
        public bool Has(string id, int needed = 1)
        {
            if (string.IsNullOrWhiteSpace(id) || needed <= 0) return false;
            var e = FindEntry(id);
            return e != null && e.count >= needed;
        }

        /// <summary>
        /// 返回当前数量，若无则 0。
        /// </summary>
        public int GetCount(string id)
        {
            var e = FindEntry(id);
            return e != null ? Mathf.Max(0, e.count) : 0;
        }

        /// <summary>
        /// 添加数量（不存在则创建）。返回最终数量。
        /// </summary>
        public int Add(string id, int amount = 1)
        {
            //if (debugLog)
            //    Debug.Log($"[InventoryLite.Add] id={id}, amount={amount}, canCreateNew={CanCreateNewStack()}, unlocked={UnlockedSlotCount}", this);

            if (string.IsNullOrWhiteSpace(id) || amount == 0) return GetCount(id);

            var e = FindEntry(id);
            if (e == null)
            {
                if (amount > 0)
                {
                    // 仅当不是“静默批量加载且忽略容量”时，才检查容量
                    bool needCheckCapacity = !(_suppressChanged && ignoreCapacityWhenLoading);
                    if (needCheckCapacity && !CanCreateNewStack())
                    {
                        //if (debugLog) Debug.LogWarning("[InventoryLite.Add] capacity full, skip create new stack", this);
                        return 0;
                    }
                    entries.Add(new Entry { id = id, count = amount });
                }
                // amount < 0 且条目不存在，不创建条目（维持原逻辑）
            }
            else
            {
                e.count += amount;
                if (e.count <= 0)
                {
                    entries.Remove(e);
                    if (!_suppressChanged) OnChanged?.Invoke();
                    return 0;
                }
            }

            if (!_suppressChanged) OnChanged?.Invoke();
            return GetCount(id);
        }

        /// <summary>
        /// 尝试减少；不够则失败并不改动。成功返回 true。
        /// </summary>
        public bool Remove(string id, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(id) || amount <= 0) return false;

            var e = FindEntry(id);
            if (e == null || e.count < amount) return false;

            e.count -= amount;
            if (e.count <= 0) entries.Remove(e);

            if (!_suppressChanged) OnChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 若拥有 needed 数量则扣减并返回 true，否则 false。
        /// </summary>
        public bool TryConsume(string id, int needed = 1)
        {
            if (!Has(id, needed)) return false;
            return Remove(id, needed);
        }

        /// <summary>
        /// 清空全部。
        /// </summary>
        public void ClearAll()
        {
            entries.Clear();
            if (!_suppressChanged) OnChanged?.Invoke();
        }

        /// <summary>
        /// 当前占用的“堆叠数”（用于与 UnlockedSlotCount 比较；与你的 UI 一一对应）。
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
        /// 是否允许创建一个新的堆叠（即是否还有未被占用的“已解锁槽位”）。
        /// </summary>
        public bool CanCreateNewStack()
        {
            return GetOccupiedStacks() < UnlockedSlotCount;
        }

        /// <summary>
        /// 当全局解锁状态被外部改变（如拾取背包）时，可显式调用，触发 UI 刷新。
        /// </summary>
        public void NotifyCapacityChanged()
        {
            //if (debugLog)
            //{
            //    Debug.Log($"[InventoryLite.NotifyCapacityChanged] unlocked={UnlockedSlotCount}, isBackpackUnlocked={IsBackpackUnlocked}", this);
            //    if (progress)
            //        Debug.Log($"[InventoryLite.NotifyCapacityChanged] progress id={progress.GetInstanceID()}, unlocked={progress.backpackUnlocked}", progress);
            //}
            if (!_suppressChanged) OnChanged?.Invoke();
        }

        // ====================== 存档/读档（PlayerPrefs 旧流程） ======================

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

            _suppressChanged = true;
            entries = data?.entries ?? new List<Entry>();
            _suppressChanged = false;

            OnChanged?.Invoke();
        }

        // ====================== 与 GameState 的桥接（新流程） ======================

        /// <summary>
        /// 从 GameState（权威存档）把数据载入到本地背包（读档→UI）。
        /// </summary>
        public void LoadFromGameState()
        {
            if (GameState.Current == null) return;

            var ids = GameState.Current.inventoryIds;
            var counts = GameState.Current.inventoryCounts;

            _suppressChanged = true;
            entries.Clear();

            if (ids != null && counts != null)
            {
                int n = Mathf.Min(ids.Length, counts.Length);
                for (int i = 0; i < n; i++)
                {
                    if (string.IsNullOrWhiteSpace(ids[i]) || counts[i] <= 0) continue;
                    // 读档：权威数据，忽略容量限制直接写入
                    entries.Add(new Entry { id = ids[i], count = counts[i] });
                }
            }

            _suppressChanged = false;
            OnChanged?.Invoke();

            //if (debugLog)
            //    Debug.Log($"[InventoryLite.LoadFromGameState] loaded {entries.Count} stacks from GameState", this);
        }

        /// <summary>
        /// 将当前背包快照写回 GameState（保存前兜底同步）。
        /// </summary>
        public void SnapshotToGameState()
        {
            if (GameState.Current == null) return;

            // 统计有效条目
            int n = 0;
            for (int i = 0; i < entries.Count; i++)
                if (!string.IsNullOrWhiteSpace(entries[i].id) && entries[i].count > 0) n++;

            GameState.Current.inventoryIds = new string[n];
            GameState.Current.inventoryCounts = new int[n];

            int k = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (string.IsNullOrWhiteSpace(e.id) || e.count <= 0) continue;
                GameState.Current.inventoryIds[k] = e.id;
                GameState.Current.inventoryCounts[k] = e.count;
                k++;
            }

            //if (debugLog)
            //    Debug.Log($"[InventoryLite.SnapshotToGameState] snapshot {n} stacks to GameState", this);
        }

        /// <summary>
        /// 便捷入口：根据 useGameStateSource 选择从哪读取（供外部调用刷新）。
        /// </summary>
        public void ReloadFromSave()
        {
            if (useGameStateSource) LoadFromGameState();
            else Load();
        }

        // ====================== 内部工具 ======================

        // 按 id 找到条目（忽略大小写），找不到返回 null
        Entry FindEntry(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return entries.Find(e => string.Equals(e.id, id, StringComparison.OrdinalIgnoreCase));
        }
    }
}
