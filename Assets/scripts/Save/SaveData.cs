using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Demo 最小可运行存档数据：仅基础字段与数组，适配 JsonUtility。
/// </summary>
[Serializable]
public class SaveData
{
    // —— 全局进度 ——  
    public bool backpackUnlocked = false;

    // —— 位置/场景（可选） ——  
    public string lastScene = "";   // 例如 "Town"
    public float playerX = 0f;
    public float playerY = 0f;

    // —— 已被“禁用/隐藏”的对象集合（一次性拾取物、一次性对话触发器等） ——  
    public string[] disabledObjectIds = Array.Empty<string>();

    // —— 背包（并行数组：id/count 一一对应） ——  
    public string[] inventoryIds = Array.Empty<string>();
    public int[] inventoryCounts = Array.Empty<int>();

    // —— 文档（已获得/已阅读） ——  
    public string[] docCollectedIds = Array.Empty<string>();
    public string[] docReadIds = Array.Empty<string>();

    // —— 对话进度（已完成的对话触发器ID） ——  
    public string[] dialogueSeenIds = Array.Empty<string>();


    // —— 工具方法：保证数组非空（从旧版本/空存档恢复时用） ——  
    public void EnsureArraysNotNull()
    {
        disabledObjectIds ??= Array.Empty<string>();
        inventoryIds ??= Array.Empty<string>();
        inventoryCounts ??= Array.Empty<int>();
        docCollectedIds ??= Array.Empty<string>();
        docReadIds ??= Array.Empty<string>();
        dialogueSeenIds ??= Array.Empty<string>();
    }

    /// <summary>是否已完成该对话。</summary>
    public bool HasSeenDialogue(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        EnsureArraysNotNull();
        return Array.IndexOf(dialogueSeenIds, id) >= 0;
    }

    /// <summary>将对话标记为完成（若未存在则添加）。</summary>
    public bool TryMarkDialogueSeen(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        EnsureArraysNotNull();

        if (HasSeenDialogue(id))
            return false; // 已存在，不重复添加

        var list = dialogueSeenIds.ToList();
        list.Add(id);
        dialogueSeenIds = list.ToArray();
        return true;
    }
}
