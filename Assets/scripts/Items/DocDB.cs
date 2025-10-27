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
            public string id;               // 全局唯一
            public string displayName;      // 按钮上显示的名称
            [TextArea(4, 20)]
            public string content;          // 打开后展示在 TextPage 的详细文字
        }

        [Header("文档条目")]
        public List<DocDef> docs = new List<DocDef>();

        [Header("（可选）共用图标")]
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
