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

    // ====================== ����/��������������ȫ�ֱ������ƣ� ======================
    [Header("��������")]
    [Tooltip("�������ܲ�λ������UI �ܸ�������")]
    // [MOD] �ܲ�λ������������ʾ������������
    public int totalSlotCount = 6;

    [Header("ȫ�ֽ��ȣ��糡����Ч��")]
    [Tooltip("����һ��ȫ�� ScriptableObject���� GameProgress.asset������ backpackUnlocked Ϊȫ�ֽ�����־")]
    // [MOD] ȫ�ֽ���״̬���ã�ScriptableObject�����糡������
    public GameProgress progress;

    // [MOD] ��ǰ���ò�λ����δ����=1������=totalSlotCount
    public int UnlockedSlotCount => Mathf.Clamp(
        (progress != null && progress.backpackUnlocked) ? totalSlotCount : 1,
        1, Mathf.Max(1, totalSlotCount)
    );

    // [MOD]����ѡ�����Ⱪ¶���Ƿ��Ѿ�����
    public bool IsBackpackUnlocked => (progress != null && progress.backpackUnlocked);

    [Header("Debug")]
    public bool debugLog = true; // [DBG] ���ϼ��ɴ�ӡ

    // ==========================================================================

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
            if (count > 0)
            {
                // [MOD] �½��ѵ�ǰ��ҲҪ������������
                if (!CanCreateNewStack())
                {
                    // ���������򲻴�����ֱ�ӷ���
                    return;
                }
                entries.Add(new Entry { id = id, count = count });
            }
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

        // [DBG] ���飺����ʱע�����У������� Load �����ж�
        // if (progress != null) { progress.Load(); }

        if (debugLog)
        {
            Debug.Log($"[InventoryLite.Start] this={GetInstanceID()}, entries={entries?.Count ?? 0}", this);
            if (progress)
                Debug.Log($"[InventoryLite.Start] progress id={progress.GetInstanceID()}, unlocked={progress.backpackUnlocked}", progress);
            else
                Debug.LogWarning("[InventoryLite.Start] progress is NULL", this);
        }

        // С�ӳٺ��ٴ�һ�Σ����Ƿ񱻱��˸�д��ʱ�����⣩
        Invoke(nameof(_DBG_LogAfterOneFrame), 0f);
    }

    void _DBG_LogAfterOneFrame()
    {
        if (debugLog)
        {
            Debug.Log($"[InventoryLite.PostFrame] unlockedSlotCount={UnlockedSlotCount}, occupied={GetOccupiedStacks()}", this);
        }
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
        // [DBG]
        if (debugLog)
            Debug.Log($"[InventoryLite.Add] id={id}, amount={amount}, canCreateNew={CanCreateNewStack()}, unlocked={UnlockedSlotCount}", this);

        if (string.IsNullOrWhiteSpace(id) || amount == 0) return GetCount(id);

        var e = FindEntry(id);
        if (e == null)
        {
            if (amount > 0)
            {
                // [MOD] �������С����õ��ѽ�����λ��ʱ���������½��ѵ�
                if (!CanCreateNewStack())
                {
                    if (debugLog) Debug.LogWarning("[InventoryLite.Add] capacity full, skip create new stack", this);
                    return 0;
                }
                entries.Add(new Entry { id = id, count = amount });
            }
            // amount < 0 ����Ŀ�����ڣ�����Ϊ 0 �����ϼ�����������Ŀ������ԭ�߼���
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

    /// <summary>
    /// [MOD] ��ǰռ�õġ��ѵ������������� UnlockedSlotCount �Ƚϣ���
    /// ע�⣺�����ԡ���Ч��Ŀ������Ϊռ�ò�λ��������� UI һһ��Ӧ��
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
    /// [MOD] �Ƿ�������һ���µĶѵ������Ƿ���δ��ռ�õġ��ѽ�����λ����
    /// </summary>
    public bool CanCreateNewStack()
    {
        return GetOccupiedStacks() < UnlockedSlotCount;
    }

    /// <summary>
    /// [MOD] ��ȫ�ֽ���״̬���ⲿ�ı䣨��ʰȡ������ʱ������ʽ���ã����� UI ˢ�¡�
    /// ��PickupBackpack2D �ڽ�������� OnChanged?.Invoke() Ҳ���Դﵽ��ͬĿ�ġ���
    /// </summary>
    public void NotifyCapacityChanged()
    {
        if (debugLog)
        {
            Debug.Log($"[InventoryLite.NotifyCapacityChanged] unlocked={UnlockedSlotCount}, isBackpackUnlocked={IsBackpackUnlocked}", this);
            if (progress)
                Debug.Log($"[InventoryLite.NotifyCapacityChanged] progress id={progress.GetInstanceID()}, unlocked={progress.backpackUnlocked}", progress);
        }
        OnChanged?.Invoke();
    }
}
