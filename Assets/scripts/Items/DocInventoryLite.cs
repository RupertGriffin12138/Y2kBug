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

        [Header("����")]
        public List<Entry> entries = new List<Entry>();
        public DocDB docDB;

        [Header("�浵��Դ")]
        [Tooltip("��ѡ���� GameState ΪΨһ��ʵ��Դ�����ٴ����� PlayerPrefs ����")]
        public bool useGameStateSource = true;

        public event Action OnChanged;

        private const string PREFS_KEY = "save_docs_lite";

        public void Start()
        {
            if (useGameStateSource) LoadFromGameState();
            else Load();
        }

        // ====================== ����̬ API ======================

        private Entry FindEntry(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return entries.Find(e => string.Equals(e.id, id, StringComparison.OrdinalIgnoreCase));
        }

        public bool Has(string id) => FindEntry(id) != null;

        /// <summary>����ĵ�����ӵ�����ظ���ӡ������Ƿ�������</summary>
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

        // ====================== PlayerPrefs �����̣����ݣ� ======================

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

        // ====================== �� GameState ���Žӣ������̣� ======================

        /// <summary>
        /// �� GameState��Ȩ���浵�����롰����¼�ĵ���������̬�б�������UI����
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

            // �Ѷ�״̬��docReadIds��ͨ�����Ķ����/ͼ��ֱ�Ӷ� GameState ������
            // �����Ҳϣ���� DocInventoryLite ��ά�����Ѷ������ϣ������ڴ˴�һ��ͬ����

            OnChanged?.Invoke();
        }

        /// <summary>
        /// ������̬������¼�ĵ������ջ�д�� GameState������ǰ����ͬ������
        /// </summary>
        public void SnapshotToGameState()
        {
            if (GameState.Current == null) return;

            // ȥ��+��Ч����
            List<string> ids = new List<string>();
            foreach (var e in entries)
            {
                if (e == null || string.IsNullOrWhiteSpace(e.id)) continue;
                if (!ids.Contains(e.id)) ids.Add(e.id);
            }

            GameState.Current.docCollectedIds = ids.ToArray();
            // �Ѷ����ϣ�docReadIds�����Ķ����������߼��� GameState ��ά�������ﲻ����
        }

        /// <summary>
        /// �����ڣ����� useGameStateSource ѡ����Ķ�ȡ�����ⲿ����ˢ�£���
        /// </summary>
        public void ReloadFromSave()
        {
            if (useGameStateSource) LoadFromGameState();
            else Load();
        }
    }
}
