using UnityEngine;

/// <summary>全局进度/标志位（项目资源级，跨场景共享）</summary>
[CreateAssetMenu(menuName = "Game/Global/GameProgress")]
public class GameProgress : ScriptableObject
{
    [Header("背包是否已解锁（全局）")]
    public bool backpackUnlocked = false;

    const string PF_BACKPACK = "pf_backpack_unlocked";

    /// <summary>解锁背包，并保存</summary>
    public void UnlockBackpack(bool autosave = true)
    {
        if (!backpackUnlocked)
        {
            backpackUnlocked = true;
            if (autosave) Save();
        }
    }

    /// <summary>保存到本地（可选）</summary>
    public void Save()
    {
        PlayerPrefs.SetInt(PF_BACKPACK, backpackUnlocked ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>从本地读取（可选，在游戏启动时调用）</summary>
    public void Load()
    {
        if (PlayerPrefs.HasKey(PF_BACKPACK))
            backpackUnlocked = PlayerPrefs.GetInt(PF_BACKPACK, 0) != 0;
    }

    /// <summary>调试用：清除存档</summary>
    public void ResetSave()
    {
        backpackUnlocked = false;
        PlayerPrefs.DeleteKey(PF_BACKPACK);
    }
}
