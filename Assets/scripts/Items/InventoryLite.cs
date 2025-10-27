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
    public List<Entry> entries = new List<Entry>();   // ����ʱ���ݣ�Inspector ��ֱ�ģ�
    public ItemDB itemDB;

    // ====================== ����/�������� GameState ΪΨһ���ࣩ ======================
    [Header("��������")]
    [Tooltip("����δ����ʱ�Ŀ��ò�λ������ͨ�� 0 �� 1��")]
    public int slotCountWhenLocked = 0;

    [Tooltip("����������Ŀ��ò�λ���������� 6��")]
    public int slotCountWhenUnlocked = 6;

    /// <summary>�Ƿ��ѽ���������Ψһ��Դ��GameState.BackpackUnlocked����</summary>
    public bool IsBackpackUnlocked => (GameState.Current != null && GameState.BackpackUnlocked);

    /// <summary>��ǰ���ò�λ����UI ֱ��ʹ�ã���</summary>
    public int UnlockedSlotCount
    {
        get
        {
            bool unlocked = (GameState.Current != null && GameState.BackpackUnlocked);
            int value = unlocked ? slotCountWhenUnlocked : slotCountWhenLocked;
            // �н�������Ƿ�����
            return Mathf.Clamp(value, 0, Mathf.Max(0, slotCountWhenUnlocked));
        }
    }

    // ====================== �浵��Դѡ�񣨼��ݾ����̣� ======================
    [Header("�浵��Դ")]
    [Tooltip("��ѡ���� GameState ΪΨһ��ʵ��Դ������ѡ��ʹ�� PlayerPrefs �����̡�")]
    public bool useGameStateSource = true;

    [Tooltip("�Ӵ浵���ص�����ʱ�������������ƣ��Ƽ���ѡ��")]
    public bool ignoreCapacityWhenLoading = true;

    [Header("Debug")]
    public bool debugLog = false; // ������־

    public event Action OnChanged;

    const string PREFS_KEY = "save_inventory_lite";

    // ��������ʱ���� OnChanged Ƶ������
    bool _suppressChanged = false;

    // ==========================================================================

    void Start()
    {
        if (useGameStateSource)
        {
            // ȷ�� GameState �Ѽ���
            if (GameState.Current == null)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                GameState.LoadGameOrNew(scene);
            }
            LoadFromGameState();
        }
        else
        {
            Load(); // ���ݾ����̣�PlayerPrefs��
        }

        if (debugLog)
            Debug.Log($"[InventoryLite.Start] entries={entries.Count}, unlocked={UnlockedSlotCount}, isUnlocked={IsBackpackUnlocked}", this);
    }

    // ====================== ���� API ======================

    /// <summary>���������嵥��</summary>
    public void SetAll(List<Entry> newEntries)
    {
        entries = newEntries ?? new List<Entry>();
        if (!_suppressChanged) OnChanged?.Invoke();
    }

    /// <summary>ֱ������ָ����Ʒ��������Ϊ 0 ��ɾ������</summary>
    public void SetCount(string id, int count)
    {
        if (string.IsNullOrEmpty(id)) return;
        var e = entries.Find(x => string.Equals(x.id, id, StringComparison.OrdinalIgnoreCase));
        if (e == null)
        {
            if (count > 0)
            {
                bool needCheckCapacity = !(_suppressChanged && ignoreCapacityWhenLoading);
                if (needCheckCapacity && !CanCreateNewStack())
                {
                    if (debugLog) Debug.LogWarning("[InventoryLite.SetCount] capacity full, skip create new stack", this);
                    // ������������ǿ��д��Ͱ���һ�� return ע�͵��������飩
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

    /// <summary>�Ƿ�����ӵ�� needed ����</summary>
    public bool Has(string id, int needed = 1)
    {
        if (string.IsNullOrWhiteSpace(id) || needed <= 0) return false;
        var e = FindEntry(id);
        return e != null && e.count >= needed;
    }

    /// <summary>���ص�ǰ������������ 0��</summary>
    public int GetCount(string id)
    {
        var e = FindEntry(id);
        return e != null ? Mathf.Max(0, e.count) : 0;
    }

    /// <summary>����������������򴴽�������������������</summary>
    public int Add(string id, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(id) || amount == 0) return GetCount(id);

        var e = FindEntry(id);
        if (e == null)
        {
            if (amount > 0)
            {
                bool needCheckCapacity = !(_suppressChanged && ignoreCapacityWhenLoading);
                if (needCheckCapacity && !CanCreateNewStack())
                {
                    if (debugLog) Debug.LogWarning("[InventoryLite.Add] capacity full, skip create new stack", this);
                    return GetCount(id);
                }
                entries.Add(new Entry { id = id, count = amount });
            }
            // amount < 0 �Ҳ����ڣ���������Ŀ
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

    /// <summary>���Լ��٣�������ʧ�ܲ����Ķ����ɹ����� true��</summary>
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

    /// <summary>��ӵ�� needed ������ۼ������� true������ false��</summary>
    public bool TryConsume(string id, int needed = 1)
    {
        if (!Has(id, needed)) return false;
        return Remove(id, needed);
    }

    /// <summary>���ȫ����</summary>
    public void ClearAll()
    {
        entries.Clear();
        if (!_suppressChanged) OnChanged?.Invoke();
    }

    /// <summary>��ǰռ�õġ��ѵ������������� UnlockedSlotCount �Ƚϣ������ UI һһ��Ӧ����</summary>
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

    /// <summary>�Ƿ�������һ���µĶѵ������Ƿ���δ��ռ�õġ��ѽ�����λ������</summary>
    public bool CanCreateNewStack()
    {
        return GetOccupiedStacks() < UnlockedSlotCount;
    }

    /// <summary>��ȫ�ֽ���״̬���ⲿ�ı䣨�����������ʱ������ʽ���ã����� UI ˢ�¡�</summary>
    public void NotifyCapacityChanged()
    {
        if (debugLog)
            Debug.Log($"[InventoryLite.NotifyCapacityChanged] unlocked={UnlockedSlotCount}, isBackpackUnlocked={IsBackpackUnlocked}", this);
        if (!_suppressChanged) OnChanged?.Invoke();
    }

    // ====================== �浵/������PlayerPrefs �����̣� ======================

    public void Save()
    {
        var data = new SaveData { entries = entries };
        var json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(PREFS_KEY, json);
        PlayerPrefs.Save();

        if (debugLog) Debug.Log("[InventoryLite.Save] saved to PlayerPrefs", this);
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey(PREFS_KEY)) return;
        var json = PlayerPrefs.GetString(PREFS_KEY);
        var data = JsonUtility.FromJson<SaveData>(json);

        _suppressChanged = true;
        entries = data?.entries ?? new List<Entry>();
        _suppressChanged = false;

        if (debugLog) Debug.Log($"[InventoryLite.Load] loaded {entries.Count} stacks from PlayerPrefs", this);
        OnChanged?.Invoke();
    }

    // ====================== �� GameState ���Žӣ������̣� ======================

    /// <summary>�� GameState��Ȩ���浵�����������뵽���ر�����������UI����</summary>
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
                // ������Ȩ�����ݣ�������������ֱ��д��
                entries.Add(new Entry { id = ids[i], count = counts[i] });
            }
        }

        _suppressChanged = false;
        if (debugLog) Debug.Log($"[InventoryLite.LoadFromGameState] loaded {entries.Count} stacks from GameState", this);
        OnChanged?.Invoke();
    }

    /// <summary>����ǰ��������д�� GameState������ǰ����ͬ������</summary>
    public void SnapshotToGameState()
    {
        if (GameState.Current == null) return;

        // ͳ����Ч��Ŀ
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

        if (debugLog) Debug.Log($"[InventoryLite.SnapshotToGameState] snapshot {n} stacks to GameState", this);
    }

    /// <summary>�����ڣ����� useGameStateSource ѡ����Ķ�ȡ�����ⲿ����ˢ�£���</summary>
    public void ReloadFromSave()
    {
        if (useGameStateSource) LoadFromGameState();
        else Load();
    }

    // ====================== �ڲ����� ======================

    Entry FindEntry(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return entries.Find(e => string.Equals(e.id, id, StringComparison.OrdinalIgnoreCase));
    }
}
