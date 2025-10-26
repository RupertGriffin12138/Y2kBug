using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Demo ��С�����д浵���ݣ��������ֶ������飬���� JsonUtility��
/// </summary>
[Serializable]
public class SaveData
{
    // ���� ȫ�ֽ��� ����  
    public bool backpackUnlocked = false;

    // ���� λ��/��������ѡ�� ����  
    public string lastScene = "";   // ���� "Town"
    public float playerX = 0f;
    public float playerY = 0f;

    // ���� �ѱ�������/���ء��Ķ��󼯺ϣ�һ����ʰȡ�һ���ԶԻ��������ȣ� ����  
    public string[] disabledObjectIds = Array.Empty<string>();

    // ���� �������������飺id/count һһ��Ӧ�� ����  
    public string[] inventoryIds = Array.Empty<string>();
    public int[] inventoryCounts = Array.Empty<int>();

    // ���� �ĵ����ѻ��/���Ķ��� ����  
    public string[] docCollectedIds = Array.Empty<string>();
    public string[] docReadIds = Array.Empty<string>();

    // ���� �Ի����ȣ�����ɵĶԻ�������ID�� ����  
    public string[] dialogueSeenIds = Array.Empty<string>();


    // ���� ���߷�������֤����ǿգ��Ӿɰ汾/�մ浵�ָ�ʱ�ã� ����  
    public void EnsureArraysNotNull()
    {
        disabledObjectIds ??= Array.Empty<string>();
        inventoryIds ??= Array.Empty<string>();
        inventoryCounts ??= Array.Empty<int>();
        docCollectedIds ??= Array.Empty<string>();
        docReadIds ??= Array.Empty<string>();
        dialogueSeenIds ??= Array.Empty<string>();
    }

    /// <summary>�Ƿ�����ɸöԻ���</summary>
    public bool HasSeenDialogue(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        EnsureArraysNotNull();
        return Array.IndexOf(dialogueSeenIds, id) >= 0;
    }

    /// <summary>���Ի����Ϊ��ɣ���δ��������ӣ���</summary>
    public bool TryMarkDialogueSeen(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        EnsureArraysNotNull();

        if (HasSeenDialogue(id))
            return false; // �Ѵ��ڣ����ظ����

        var list = dialogueSeenIds.ToList();
        list.Add(id);
        dialogueSeenIds = list.ToArray();
        return true;
    }
}
