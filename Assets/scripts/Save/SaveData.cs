using System;
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
    public float playerX = 0f;      // 可选：读档后把玩家放到大致位置
    public float playerY = 0f;

    // —— 已被“禁用/隐藏”的对象集合（一次性拾取物、一次性对话触发器等） ——
    public string[] disabledObjectIds = Array.Empty<string>();

    // —— 背包（并行数组：id/count 一一对应） ——
    public string[] inventoryIds = Array.Empty<string>();
    public int[] inventoryCounts = Array.Empty<int>();

    // —— 文档（已获得/已阅读） ——
    public string[] docCollectedIds = Array.Empty<string>();
    public string[] docReadIds = Array.Empty<string>();

    // —— 工具方法：保证数组非空（从旧版本/空存档恢复时用） ——
    public void EnsureArraysNotNull()
    {
        disabledObjectIds ??= Array.Empty<string>();
        inventoryIds ??= Array.Empty<string>();
        inventoryCounts ??= Array.Empty<int>();
        docCollectedIds ??= Array.Empty<string>();
        docReadIds ??= Array.Empty<string>();
    }
}
