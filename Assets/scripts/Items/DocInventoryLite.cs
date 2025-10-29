using System;
using System.Collections.Generic;
using Save;
using UnityEngine;

namespace Items
{
    public class DocInventoryLite : MonoBehaviour
    {
        [Serializable]
        public class Entry { public string id; public int count = 1; }

        [Serializable]
        public class SaveData { public List<Entry> entries = new List<Entry>(); }

        [Header("数据")]
        public List<Entry> entries = new List<Entry>();
        public DocDB docDB;

        [Header("存档来源")]
        [Tooltip("勾选后，以 GameState 为唯一真实来源；不再从自身 PlayerPrefs 读。")]
        public bool useGameStateSource = true;

        public event Action OnChanged;

        private const string PREFS_KEY = "save_docs_lite";

        public void Start()
        {
            if (useGameStateSource) LoadFromGameState();
            else Load();
        }

        // ====================== 运行态 API ======================

        private Entry FindEntry(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return entries.Find(e => string.Equals(e.id, id, StringComparison.OrdinalIgnoreCase));
        }

        public bool Has(string id) => FindEntry(id) != null;

        /// <summary>添加文档：已拥有则不重复添加。返回是否新增。</summary>
        public bool AddOnce(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            if (Has(id)) return false;
            entries.Add(new Entry { id = id, count = 1 });
            OnChanged?.Invoke();
            return true;
        }

        public bool Remove(string id)
        {
            var e = FindEntry(id);
            if (e == null) return false;
            entries.Remove(e);
            OnChanged?.Invoke();
            return true;
        }

        public void ClearAll()
        {
            entries.Clear();
            OnChanged?.Invoke();
        }

        // ====================== PlayerPrefs 旧流程（兼容） ======================

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

        // ====================== 与 GameState 的桥接（新流程） ======================

        /// <summary>
        /// 从 GameState（权威存档）载入“已收录文档”到运行态列表（读档→UI）。
        /// </summary>
        public void LoadFromGameState()
        {
            if (GameState.Current == null) return;

            entries.Clear();

            var ids = GameState.Current.docCollectedIds;
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    if (string.IsNullOrWhiteSpace(id)) continue;
                    entries.Add(new Entry { id = id, count = 1 });
                }
            }

            // 已读状态（docReadIds）通常由阅读面板/图标直接读 GameState 决定，
            // 如果你也希望在 DocInventoryLite 里维护“已读”集合，可以在此处一并同步。

            OnChanged?.Invoke();
        }

        /// <summary>
        /// 将运行态“已收录文档”快照回写到 GameState（保存前兜底同步）。
        /// </summary>
        public void SnapshotToGameState()
        {
            if (GameState.Current == null) return;

            // 去重+有效过滤
            List<string> ids = new List<string>();
            foreach (var e in entries)
            {
                if (e == null || string.IsNullOrWhiteSpace(e.id)) continue;
                if (!ids.Contains(e.id)) ids.Add(e.id);
            }

            GameState.Current.docCollectedIds = ids.ToArray();
            // 已读集合（docReadIds）由阅读面板或其它逻辑在 GameState 中维护，这里不处理。
        }

        /// <summary>
        /// 便捷入口：根据 useGameStateSource 选择从哪读取（供外部调用刷新）。
        /// </summary>
        public void ReloadFromSave()
        {
            if (useGameStateSource) LoadFromGameState();
            else Load();
        }
    }
}
