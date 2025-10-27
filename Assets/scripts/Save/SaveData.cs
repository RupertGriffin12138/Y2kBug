using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Demo 最小可运行存档数据：仅基础字段与数组，适配 JsonUtility。
/// 兼容：保留 lastScene/playerX/playerY；新增按场景名存位置的结构与方法。
/// </summary>
[Serializable]
public class SaveData
{
    // —— 全局进度 ——  
    public bool backpackUnlocked = false;

    // —— 位置/场景（向下兼容：保留老字段） ——  
    public string lastScene = "";   // 例如 "Town"
    public float playerX = 0f;
    public float playerY = 0f;

    // 新增：按场景名记录玩家位置（适配 JsonUtility，不能用 Dictionary）
    [Serializable]
    public class ScenePos
    {
        public string scene;
        public float x;
        public float y;

        public ScenePos() { }
        public ScenePos(string scene, Vector2 pos)
        {
            this.scene = scene;
            this.x = pos.x;
            this.y = pos.y;
        }

        public Vector2 ToVector2() => new Vector2(x, y);
    }

    public ScenePos[] scenePositions = Array.Empty<ScenePos>();

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

    // =========================================================
    // 兼容/工具方法
    // =========================================================

    /// <summary>保证数组非空（从旧版本/空存档恢复时用）。</summary>
    public void EnsureArraysNotNull()
    {
        scenePositions ??= Array.Empty<ScenePos>();
        disabledObjectIds ??= Array.Empty<string>();
        inventoryIds ??= Array.Empty<string>();
        inventoryCounts ??= Array.Empty<int>();
        docCollectedIds ??= Array.Empty<string>();
        docReadIds ??= Array.Empty<string>();
        dialogueSeenIds ??= Array.Empty<string>();
    }

    /// <summary>若旧版存档里有 lastScene/x/y，但还没写入 scenePositions，则做一次迁移。</summary>
    public void MigrateLegacyPlayerPosIfNeeded()
    {
        EnsureArraysNotNull();
        if (!string.IsNullOrEmpty(lastScene))
        {
            bool exists = scenePositions.Any(p => p.scene == lastScene);
            if (!exists)
            {
                var list = scenePositions.ToList();
                list.Add(new ScenePos(lastScene, new Vector2(playerX, playerY)));
                scenePositions = list.ToArray();
            }
        }
    }

    /// <summary>设置某个场景下的玩家坐标；同时更新 lastScene 和旧版 x/y 以保持兼容。</summary>
    public void SetPlayerPos(string scene, Vector2 position)
    {
        if (string.IsNullOrEmpty(scene)) return;
        EnsureArraysNotNull();

        int idx = Array.FindIndex(scenePositions, p => p.scene == scene);
        if (idx >= 0)
        {
            scenePositions[idx].x = position.x;
            scenePositions[idx].y = position.y;
        }
        else
        {
            var list = scenePositions.ToList();
            list.Add(new ScenePos(scene, position));
            scenePositions = list.ToArray();
        }

        // 同步旧字段（方便老逻辑或调试用）
        lastScene = scene;
        playerX = position.x;
        playerY = position.y;
    }

    /// <summary>读取某个场景的玩家坐标；若没有记录，尝试用旧字段作为回退。</summary>
    public bool TryGetPlayerPos(string scene, out Vector2 position)
    {
        EnsureArraysNotNull();

        if (!string.IsNullOrEmpty(scene))
        {
            var rec = Array.Find(scenePositions, p => p.scene == scene);
            if (rec != null)
            {
                position = rec.ToVector2();
                return true;
            }
        }

        // 回退：老字段有效且场景名匹配时使用
        if (!string.IsNullOrEmpty(lastScene) && lastScene == scene)
        {
            position = new Vector2(playerX, playerY);
            return true;
        }

        position = default;
        return false;
    }

    // —— 对话进度 API（原样保留） ——  

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
