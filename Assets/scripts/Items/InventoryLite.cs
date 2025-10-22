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

    [Header("����")]
    public List<Entry> entries = new List<Entry>(); // ����ʱ���ݣ�Inspector ��ֱ�ģ�
    public ItemDB itemDB;

    public event Action OnChanged;

    const string PREFS_KEY = "save_inventory_lite";

    // ���� ����ʱ�޸�API ���� //
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

    // ���� �浵/���� ���� //
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

    // ������� Start ʱ���Զ���
    void Start()
    {
        Load();
    }

    // �ɵ��õķ���

    // ���� �������� id �ҵ���Ŀ�����Դ�Сд�����Ҳ������� null ���� //
    InventoryLite.Entry FindEntry(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return entries.Find(e => string.Equals(e.id, id, System.StringComparison.OrdinalIgnoreCase));
    }

    // ���� ��ѯ���Ƿ�����ӵ�� needed �� ���� //
    public bool Has(string id, int needed = 1)
    {
        if (string.IsNullOrWhiteSpace(id) || needed <= 0) return false;
        var e = FindEntry(id);
        return e != null && e.count >= needed;
    }

    // ���� ��ѯ�����ص�ǰ������������ 0 ���� //
    public int GetCount(string id)
    {
        var e = FindEntry(id);
        return e != null ? Mathf.Max(0, e.count) : 0;
    }

    // ���� ��ӣ��� amount �ӵ� id �ϣ��������򴴽����������������� ���� //
    public int Add(string id, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(id) || amount == 0) return GetCount(id);

        var e = FindEntry(id);
        if (e == null)
        {
            if (amount > 0)
                entries.Add(new Entry { id = id, count = amount });
            // amount < 0 ����Ŀ�����ڣ�����Ϊ 0 �����ϼ�����������Ŀ
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

    // ���� ɾ�������Լ��� amount��������ʧ�ܲ����Ķ����ɹ����� true ���� //
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

    // ���� ���ģ���ӵ�� needed ������ۼ������� true������ false�������ڡ�ʹ����Ʒ���� ���� //
    public bool TryConsume(string id, int needed = 1)
    {
        if (!Has(id, needed)) return false;
        return Remove(id, needed);
    }

    // ���� ���ȫ�� ���� //
    public void ClearAll()
    {
        entries.Clear();
        OnChanged?.Invoke();
    }

}
