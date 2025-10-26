using System;
using UnityEngine;

/// <summary>
/// ���� SaveData �Ĵ���/����/����/�嵵�����صײ�洢ϸ�ڡ�
/// </summary>
public static class SaveManager
{
    // �����������ʱ�ĳ� FileSaveStore()
    private static ISaveStore store = new PlayerPrefsSaveStore("SaveSlot_1");

    public static void UsePlayerPrefs(string key = "SaveSlot_1")
    {
        store = new PlayerPrefsSaveStore(key);
    }

    public static void UseFile(string filename = "demo_save.json")
    {
        store = new FileSaveStore(filename);
    }

    /// <summary>����һ�ݡ�����Ϸ����Ĭ�����ݡ�</summary>
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

    /// <summary>���ԴӴ洢���أ�ʧ���򷵻�Ĭ�����ݡ�</summary>
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
            Debug.LogWarning($"[SaveManager] ��ȡ�浵ʧ�ܣ���ʹ��Ĭ�����ݡ�Exception: {e}");
        }
        return CreateDefault(fallbackFirstScene);
    }

    /// <summary>���浽�־û��洢��</summary>
    public static void Save(SaveData data)
    {
        if (data == null)
        {
            Debug.LogError("[SaveManager] Save ʧ�ܣ�data Ϊ null��");
            return;
        }
        try
        {
            data.EnsureArraysNotNull();
            var json = JsonUtility.ToJson(data);
            store.Save(json);
#if UNITY_EDITOR
            Debug.Log($"[SaveManager] �ѱ���浵��{json}");
#endif
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] ����ʧ�ܣ�{e}");
        }
    }

    /// <summary>����浵�����ڵ��Ի�����Ϸ���ǡ�����</summary>
    public static void Wipe()
    {
        try
        {
            store.Wipe();
            Debug.Log("[SaveManager] ������浵��");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] ���ʧ�ܣ�{e}");
        }
    }
}
