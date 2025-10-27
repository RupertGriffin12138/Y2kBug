using UnityEngine;
using System.Diagnostics; // for StackTrace

[CreateAssetMenu(menuName = "Game/Global/GameProgress")]
public class GameProgress : ScriptableObject
{
    [Header("背包是否已解锁（全局）")]
    [SerializeField] private bool _backpackUnlocked = false;
    public bool backpackUnlocked => _backpackUnlocked;

    const string PF_BACKPACK = "pf_backpack_unlocked";
    static bool _loadedOnce = false;

    public void UnlockBackpack(bool autosave = true)
    {
        if (!_backpackUnlocked)
        {
            _backpackUnlocked = true;
            UnityEngine.Debug.LogWarning($"[GameProgress] backpackUnlocked => TRUE  (via UnlockBackpack)\n{new StackTrace(true)}");
            if (autosave) Save();
        }
    }

    // 临时侦测：任何地方直接改都走这个方法
    public void ForceSetBackpack(bool value, string reason = "")
    {
        if (_backpackUnlocked != value)
        {
            _backpackUnlocked = value;
            UnityEngine.Debug.LogWarning($"[GameProgress] ForceSetBackpack({value}) {reason}\n{new StackTrace(true)}");
        }
    }

    public void Save()
    {
        PlayerPrefs.SetInt(PF_BACKPACK, _backpackUnlocked ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey(PF_BACKPACK))
        {
            bool v = PlayerPrefs.GetInt(PF_BACKPACK, 0) != 0;
            if (_backpackUnlocked != v)
            {
                _backpackUnlocked = v;
                UnityEngine.Debug.Log($"[GameProgress] Load -> backpackUnlocked = {v}");
            }
        }
    }

    public void LoadOnce()
    {
        if (_loadedOnce) return;
        Load();
        _loadedOnce = true;
        UnityEngine.Debug.Log("[GameProgress] LoadOnce done");
    }

    [ContextMenu("Reset Save (Editor)")]
    public void EditorReset()
    {
        _backpackUnlocked = false;
        PlayerPrefs.DeleteKey(PF_BACKPACK);
        PlayerPrefs.Save();
        UnityEngine.Debug.Log("[GameProgress] ResetSave done");
    }
}
