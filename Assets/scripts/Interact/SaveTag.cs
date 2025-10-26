using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 贴在需要被存档控制显隐/一次性触发的对象上，提供唯一 id。
/// Demo 阶段只需手填字符串，或用右键菜单自动生成一次。
/// </summary>
[DisallowMultipleComponent]
public class SaveTag : MonoBehaviour
{
    [Tooltip("为该对象填写一个全局唯一的ID，例如 `town.pickup.apple01`。")]
    public string id;

    [Tooltip("（可选）类别标签，仅用于自动生成ID时拼接。例：pickup / switch / dlg")]
    public string categoryHint = "pickup";

#if UNITY_EDITOR
    [ContextMenu("SaveTag/生成ID（基于场景+对象名）")]
    private void GenerateIdContextMenu()
    {
        id = SaveIdUtility.GenerateReadableId(gameObject, categoryHint);
        EditorUtility.SetDirty(this);
        Debug.Log($"[SaveTag] 已生成ID: {id}", this);
    }

    private void OnValidate()
    {
        // 简单校验：空格→下划线；去除首尾空白
        if (!string.IsNullOrEmpty(id))
        {
            id = id.Trim().Replace(' ', '_');
        }
    }
#endif
}
