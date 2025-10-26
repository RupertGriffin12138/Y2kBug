using System;
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
    public float playerX = 0f;      // ��ѡ�����������ҷŵ�����λ��
    public float playerY = 0f;

    // ���� �ѱ�������/���ء��Ķ��󼯺ϣ�һ����ʰȡ�һ���ԶԻ��������ȣ� ����
    public string[] disabledObjectIds = Array.Empty<string>();

    // ���� �������������飺id/count һһ��Ӧ�� ����
    public string[] inventoryIds = Array.Empty<string>();
    public int[] inventoryCounts = Array.Empty<int>();

    // ���� �ĵ����ѻ��/���Ķ��� ����
    public string[] docCollectedIds = Array.Empty<string>();
    public string[] docReadIds = Array.Empty<string>();

    // ���� ���߷�������֤����ǿգ��Ӿɰ汾/�մ浵�ָ�ʱ�ã� ����
    public void EnsureArraysNotNull()
    {
        disabledObjectIds ??= Array.Empty<string>();
        inventoryIds ??= Array.Empty<string>();
        inventoryCounts ??= Array.Empty<int>();
        docCollectedIds ??= Array.Empty<string>();
        docReadIds ??= Array.Empty<string>();
    }
}
