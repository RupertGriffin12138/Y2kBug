using System;
using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "GameLite/DocDB", fileName = "DocDB")]
    public class DocDB : ScriptableObject
    {
        [Serializable]
        public class DocDef
        {
            public string id;               // ȫ��Ψһ
            public string displayName;      // ��ť����ʾ������
            [TextArea(4, 20)]
            public string content;          // �򿪺�չʾ�� TextPage ����ϸ����
        }

        [Header("�ĵ���Ŀ")]
        public List<DocDef> docs = new List<DocDef>();

        [Header("����ѡ������ͼ��")]
        //public Sprite sharedIcon;

        private Dictionary<string, DocDef> _map;

        void OnEnable()
        {
            _map = new Dictionary<string, DocDef>(StringComparer.OrdinalIgnoreCase);
            foreach (var d in docs)
            {
                if (d != null && !string.IsNullOrEmpty(d.id))
                    _map.TryAdd(d.id, d);
            }
        }

        public DocDef Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (_map == null || _map.Count == 0) OnEnable();
            _map.TryGetValue(id, out var def);
            return def;
        }
    }
}
