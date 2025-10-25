using UnityEngine;

/// <summary>ȫ�ֽ���/��־λ����Ŀ��Դ�����糡������</summary>
[CreateAssetMenu(menuName = "Game/Global/GameProgress")]
public class GameProgress : ScriptableObject
{
    [Header("�����Ƿ��ѽ�����ȫ�֣�")]
    public bool backpackUnlocked = false;

    const string PF_BACKPACK = "pf_backpack_unlocked";

    /// <summary>����������������</summary>
    public void UnlockBackpack(bool autosave = true)
    {
        if (!backpackUnlocked)
        {
            backpackUnlocked = true;
            if (autosave) Save();
        }
    }

    /// <summary>���浽���أ���ѡ��</summary>
    public void Save()
    {
        PlayerPrefs.SetInt(PF_BACKPACK, backpackUnlocked ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>�ӱ��ض�ȡ����ѡ������Ϸ����ʱ���ã�</summary>
    public void Load()
    {
        if (PlayerPrefs.HasKey(PF_BACKPACK))
            backpackUnlocked = PlayerPrefs.GetInt(PF_BACKPACK, 0) != 0;
    }

    /// <summary>�����ã�����浵</summary>
    public void ResetSave()
    {
        backpackUnlocked = false;
        PlayerPrefs.DeleteKey(PF_BACKPACK);
    }
}
