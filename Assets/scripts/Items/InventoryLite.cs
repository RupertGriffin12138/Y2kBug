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
            if (count > 0) entries.Add(new Entry { id = id, count = count });
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
        if (string.IsNullOrWhiteSpace(id) || amount == 0) return GetCount(id);

        var e = FindEntry(id);
        if (e == null)
        {
            if (amount > 0)
                entries.Add(new Entry { id = id, count = amount });
            // amount < 0 且条目不存在，则视为 0 基础上减，不创建条目
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

}
