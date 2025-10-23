using System;
using System.Collections.Generic;
using UnityEngine;

public class DocInventoryLite : MonoBehaviour
{
    [Serializable]
    public class Entry { public string id; public int count = 1; }

    [Serializable]
    public class SaveData { public List<Entry> entries = new List<Entry>(); }

    [Header("数据")]
    public List<Entry> entries = new List<Entry>();
    public DocDB docDB;

    public event Action OnChanged;

    const string PREFS_KEY = "save_docs_lite";

    void Start() { Load(); }

    Entry FindEntry(string id)
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

    // 存档/读档
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
}
