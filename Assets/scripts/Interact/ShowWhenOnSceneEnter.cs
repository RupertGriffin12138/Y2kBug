using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��������ʱ��� GameState.BackpackUnlocked��
/// - ���ѽ������� => ��������Ŀ�������ʾ��
/// - ��δ���� => ���ػ�����
/// ����ʵʱˢ�£�ֻ�� Start() ʱ�ж�һ�Ρ�
/// </summary>
public class ShowWhenBackpackUnlocked : MonoBehaviour
{
    [Tooltip("Ҫ��ʾ/���ص�Ŀ������б���������Ʊ����塣")]
    public List<GameObject> targets = new List<GameObject>();

    [Tooltip("δ����ʱ�Ƿ�ֱ�����٣���������أ�")]
    public bool destroyIfLocked = false;

    void Start()
    {
        // ���ûָ��Ŀ�꣬��Ĭ�Ͽ����Լ�
        if (targets == null || targets.Count == 0)
            targets = new List<GameObject> { gameObject };

        bool unlocked = GameState.BackpackUnlocked;

        foreach (var t in targets)
        {
            if (t == null) continue;

            if (unlocked)
            {
                t.SetActive(true);
            }
            else
            {
                if (destroyIfLocked)
                    Destroy(t);
                else
                    t.SetActive(false);
            }
        }
    }
}
