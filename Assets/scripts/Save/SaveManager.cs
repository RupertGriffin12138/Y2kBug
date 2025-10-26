using System;
using UnityEngine;

/// <summary>
/// 负责 SaveData 的创建/加载/保存/清档；隐藏底层存储细节。
/// </summary>
public static class SaveManager
{
    // 你可以在启动时改成 FileSaveStore()
    private static ISaveStore store = new PlayerPrefsSaveStore("SaveSlot_1");

    public static void UsePlayerPrefs(string key = "SaveSlot_1")
    {
        store = new PlayerPrefsSaveStore(key);
    }

    public static void UseFile(string filename = "demo_save.json")
    {
        store = new FileSaveStore(filename);
    }

    /// <summary>创建一份“新游戏”的默认数据。</summary>
    public static SaveData CreateDefault(string firstSceneName = "Town")
    {
        return new SaveData
        {
            backpackUnlocked = false,
            lastScene = firstSceneName,
            playerX = 0f,
            playerY = 0f,
            disabledObjectIds = Array.Empty<string>(),
            inventoryIds = Array.Empty<string>(),
            inventoryCounts = Array.Empty<int>(),
            docCollectedIds = Array.Empty<string>(),
            docReadIds = Array.Empty<string>()
        };
    }

    /// <summary>尝试从存储加载；失败则返回默认数据。</summary>
    public static SaveData LoadOrDefault(string fallbackFirstScene = "Town")
    {
        try
        {
            if (store.TryLoad(out var json))
            {
                var data = JsonUtility.FromJson<SaveData>(json);
                if (data == null) return CreateDefault(fallbackFirstScene);
                data.EnsureArraysNotNull();
                return data;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveManager] 读取存档失败，将使用默认数据。Exception: {e}");
        }
        return CreateDefault(fallbackFirstScene);
    }

    /// <summary>保存到持久化存储。</summary>
    public static void Save(SaveData data)
    {
        if (data == null)
        {
            Debug.LogError("[SaveManager] Save 失败：data 为 null。");
            return;
        }
        try
        {
            data.EnsureArraysNotNull();
            var json = JsonUtility.ToJson(data);
            store.Save(json);
#if UNITY_EDITOR
            Debug.Log($"[SaveManager] 已保存存档：{json}");
#endif
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] 保存失败：{e}");
        }
    }

    /// <summary>清除存档（用于调试或“新游戏覆盖”）。</summary>
    public static void Wipe()
    {
        try
        {
            store.Wipe();
            Debug.Log("[SaveManager] 已清除存档。");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] 清除失败：{e}");
        }
    }
}
