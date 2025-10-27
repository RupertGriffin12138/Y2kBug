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

        [Header("����")]
        public List<Entry> entries = new List<Entry>(); // ����ʱ���ݣ�Inspector ��ֱ�ģ�
        public ItemDB itemDB;

        // ====================== ����/��������ȫ�ֱ������ƣ� ======================
        [Header("��������")]
        [Tooltip("�������ܲ�λ������UI �ܸ�������")]
        public int totalSlotCount = 6;

        [Header("ȫ�ֽ��ȣ��糡����Ч��")]
        [Tooltip("����һ��ȫ�� ScriptableObject���� GameProgress.asset������ backpackUnlocked Ϊȫ�ֽ�����־")]
        public GameProgress progress;

        // ��ǰ���ò�λ����δ����=1������=totalSlotCount
        public int UnlockedSlotCount => Mathf.Clamp(
            (progress != null && progress.backpackUnlocked) ? totalSlotCount : 1,
            1, Mathf.Max(1, totalSlotCount)
        );

        public bool IsBackpackUnlocked => (progress != null && progress.backpackUnlocked);

        // ====================== �浵��Դѡ�� ======================
        [Header("�浵��Դ")]
        [Tooltip("��ѡ��Inventory �� GameState ΪΨһ��ʵ��Դ�����ٴ������ PlayerPrefs ����")]
        public bool useGameStateSource = true;

        [Tooltip("�Ӵ浵���ص�����ʱ�������������ƣ��Ƽ���ѡ��")]
        public bool ignoreCapacityWhenLoading = true;

        [Header("Debug")]
        public bool debugLog = true; // [DBG] ���ϼ��ɴ�ӡ

        public event Action OnChanged;

        const string PREFS_KEY = "save_inventory_lite";

        // ��������ʱ���� OnChanged Ƶ������
        bool _suppressChanged = false;

        // ==========================================================================

        // ���� ��ʼ�� ���� //
        void Start()
        {
            if (useGameStateSource)
            {
                LoadFromGameState();
            }
            else
            {
                Load(); // ���ݾ����̣�PlayerPrefs ���ش�ȡ��
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

        // ====================== ���� API ======================

        /// <summary>
        /// ���������б����ǣ���
        /// </summary>
        public void SetAll(List<Entry> newEntries)
        {
            entries = newEntries ?? new List<Entry>();
            if (!_suppressChanged) OnChanged?.Invoke();
        }

        /// <summary>
        /// ֱ������ָ����Ʒ��������Ϊ 0 ��ɾ������
        /// </summary>
        public void SetCount(string id, int count)
        {
            if (string.IsNullOrEmpty(id)) return;
            var e = entries.Find(x => string.Equals(x.id, id, StringComparison.OrdinalIgnoreCase));
            if (e == null)
            {
                if (count > 0)
                {
                    // �½��ѵ�������������������ʱ�ɺ��ԣ�
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
        /// �Ƿ�����ӵ�� needed ����
        /// </summary>
        public bool Has(string id, int needed = 1)
        {
            if (string.IsNullOrWhiteSpace(id) || needed <= 0) return false;
            var e = FindEntry(id);
            return e != null && e.count >= needed;
        }

        /// <summary>
        /// ���ص�ǰ������������ 0��
        /// </summary>
        public int GetCount(string id)
        {
            var e = FindEntry(id);
            return e != null ? Mathf.Max(0, e.count) : 0;
        }

        /// <summary>
        /// ����������������򴴽�������������������
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
                    // �������ǡ���Ĭ���������Һ���������ʱ���ż������
                    bool needCheckCapacity = !(_suppressChanged && ignoreCapacityWhenLoading);
                    if (needCheckCapacity && !CanCreateNewStack())
                    {
                        //if (debugLog) Debug.LogWarning("[InventoryLite.Add] capacity full, skip create new stack", this);
                        return 0;
                    }
                    entries.Add(new Entry { id = id, count = amount });
                }
                // amount < 0 ����Ŀ�����ڣ���������Ŀ��ά��ԭ�߼���
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
        /// ���Լ��٣�������ʧ�ܲ����Ķ����ɹ����� true��
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
        /// ��ӵ�� needed ������ۼ������� true������ false��
        /// </summary>
        public bool TryConsume(string id, int needed = 1)
        {
            if (!Has(id, needed)) return false;
            return Remove(id, needed);
        }

        /// <summary>
        /// ���ȫ����
        /// </summary>
        public void ClearAll()
        {
            entries.Clear();
            if (!_suppressChanged) OnChanged?.Invoke();
        }

        /// <summary>
        /// ��ǰռ�õġ��ѵ������������� UnlockedSlotCount �Ƚϣ������ UI һһ��Ӧ����
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
        /// �Ƿ�������һ���µĶѵ������Ƿ���δ��ռ�õġ��ѽ�����λ������
        /// </summary>
        public bool CanCreateNewStack()
        {
            return GetOccupiedStacks() < UnlockedSlotCount;
        }

        /// <summary>
        /// ��ȫ�ֽ���״̬���ⲿ�ı䣨��ʰȡ������ʱ������ʽ���ã����� UI ˢ�¡�
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

        // ====================== �浵/������PlayerPrefs �����̣� ======================

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

        // ====================== �� GameState ���Žӣ������̣� ======================

        /// <summary>
        /// �� GameState��Ȩ���浵�����������뵽���ر�����������UI����
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
                    // ������Ȩ�����ݣ�������������ֱ��д��
                    entries.Add(new Entry { id = ids[i], count = counts[i] });
                }
            }

            _suppressChanged = false;
            OnChanged?.Invoke();

            //if (debugLog)
            //    Debug.Log($"[InventoryLite.LoadFromGameState] loaded {entries.Count} stacks from GameState", this);
        }

        /// <summary>
        /// ����ǰ��������д�� GameState������ǰ����ͬ������
        /// </summary>
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

            //if (debugLog)
            //    Debug.Log($"[InventoryLite.SnapshotToGameState] snapshot {n} stacks to GameState", this);
        }

        /// <summary>
        /// �����ڣ����� useGameStateSource ѡ����Ķ�ȡ�����ⲿ����ˢ�£���
        /// </summary>
        public void ReloadFromSave()
        {
            if (useGameStateSource) LoadFromGameState();
            else Load();
        }

        // ====================== �ڲ����� ======================

        // �� id �ҵ���Ŀ�����Դ�Сд�����Ҳ������� null
        Entry FindEntry(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return entries.Find(e => string.Equals(e.id, id, StringComparison.OrdinalIgnoreCase));
        }
    }
}
