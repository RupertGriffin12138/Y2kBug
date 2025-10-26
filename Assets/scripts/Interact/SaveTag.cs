using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ������Ҫ���浵��������/һ���Դ����Ķ����ϣ��ṩΨһ id��
/// Demo �׶�ֻ�������ַ����������Ҽ��˵��Զ�����һ�Ρ�
/// </summary>
[DisallowMultipleComponent]
public class SaveTag : MonoBehaviour
{
    [Tooltip("Ϊ�ö�����дһ��ȫ��Ψһ��ID������ `town.pickup.apple01`��")]
    public string id;

    [Tooltip("����ѡ������ǩ���������Զ�����IDʱƴ�ӡ�����pickup / switch / dlg")]
    public string categoryHint = "pickup";

#if UNITY_EDITOR
    [ContextMenu("SaveTag/����ID�����ڳ���+��������")]
    private void GenerateIdContextMenu()
    {
        id = SaveIdUtility.GenerateReadableId(gameObject, categoryHint);
        EditorUtility.SetDirty(this);
        Debug.Log($"[SaveTag] ������ID: {id}", this);
    }

    private void OnValidate()
    {
        // ��У�飺�ո���»��ߣ�ȥ����β�հ�
        if (!string.IsNullOrEmpty(id))
        {
            id = id.Trim().Replace(' ', '_');
        }
    }
#endif
}
