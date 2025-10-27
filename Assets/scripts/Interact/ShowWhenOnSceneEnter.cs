using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 场景加载时检查 GameState.BackpackUnlocked：
/// - 若已解锁背包 => 激活所有目标对象（显示）
/// - 若未解锁 => 隐藏或销毁
/// 不会实时刷新，只在 Start() 时判断一次。
/// </summary>
public class ShowWhenBackpackUnlocked : MonoBehaviour
{
    [Tooltip("要显示/隐藏的目标对象列表；留空则控制本物体。")]
    public List<GameObject> targets = new List<GameObject>();

    [Tooltip("未解锁时是否直接销毁（否则仅隐藏）")]
    public bool destroyIfLocked = false;

    void Start()
    {
        // 如果没指定目标，则默认控制自己
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
