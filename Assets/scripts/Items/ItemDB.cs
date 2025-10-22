using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameLite/ItemDB", fileName = "ItemDB")]
public class ItemDB : ScriptableObject
{
    [Serializable]
    public class ItemDef
    {
        public string id;        // 全局唯一，如 "key_red", "potion_small"
        public string displayName;
        public Sprite icon;
    }

    public List<ItemDef> items = new List<ItemDef>();

    private Dictionary<string, ItemDef> _map;

    void OnEnable()
    {
        _map = new Dictionary<string, ItemDef>(StringComparer.OrdinalIgnoreCase);
        foreach (var it in items)
        {
            if (it != null && !string.IsNullOrEmpty(it.id) && !_map.ContainsKey(it.id))
                _map.Add(it.id, it);
        }
    }

    public ItemDef Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (_map == null || _map.Count == 0) OnEnable();
        _map.TryGetValue(id, out var def);
        return def;
    }
}
