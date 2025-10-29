using System.Collections.Generic;
using Save;
using UnityEngine;

namespace Interact
{
    /// <summary>
    /// 条件生成控制器（列表版）：
    /// 仅当满足设定条件时激活目标对象，可用于跨场景生成新物品 / NPC / 门 / 对话触发器。
    /// </summary>
    public class ConditionalSpawner : MonoBehaviour
    {
        [Header("标识（可选，用于调试）")]
        public string spawnerId = "spawn_001";

        [Header("要激活的目标对象")]
        public GameObject targetObject;

        [Header("触发条件（全部满足或任一满足时激活）")]
        [Tooltip("必须已拥有的物品 ID 列表")]
        public List<string> requiredItemIds = new();
        [Tooltip("必须已收集的文档 ID 列表")]
        public List<string> requiredDocIds = new();
        [Tooltip("必须已看过的对白 ID 列表")]
        public List<string> requiredDialogueIds = new();
        [Tooltip("是否要求背包已解锁")]
        public bool requireBackpackUnlocked = false;

        [Header("条件逻辑")]
        [Tooltip("勾选 = 满足任一条件即可；不勾选 = 所有条件都需满足")]
        public bool useOrLogic = false;

        [Header("是否在激活后自动标记（防止重复）")]
        public bool markOnce = true;

        private bool hasSpawned;

        private void Start()
        {
            if (!targetObject)
            {
                Debug.LogWarning($"[ConditionalSpawner] {name} 没有目标对象！", this);
                return;
            }

            // 默认先隐藏
            targetObject.SetActive(false);

            // 检查是否满足条件
            if (CheckConditions())
            {
                ActivateTarget();
            }
        }

        private bool CheckConditions()
        {
            bool allMet = true;
            bool anyMet = false;

            // === 物品条件 ===
            foreach (var id in requiredItemIds)
            {
                if (string.IsNullOrEmpty(id)) continue;
                bool has = GameState.HasItem(id);
                if (useOrLogic) anyMet |= has;
                else if (!has) allMet = false;
            }

            // === 文档条件 ===
            foreach (var id in requiredDocIds)
            {
                if (string.IsNullOrEmpty(id)) continue;
                bool has = GameState.HasCollectedDoc(id);
                if (useOrLogic) anyMet |= has;
                else if (!has) allMet = false;
            }

            // === 对话条件 ===
            foreach (var id in requiredDialogueIds)
            {
                if (string.IsNullOrEmpty(id)) continue;
                bool has = GameState.HasSeenDialogue(id);
                if (useOrLogic) anyMet |= has;
                else if (!has) allMet = false;
            }

            // === 背包条件 ===
            if (requireBackpackUnlocked)
            {
                bool has = GameState.BackpackUnlocked;
                if (useOrLogic) anyMet |= has;
                else if (!has) allMet = false;
            }

            return useOrLogic ? anyMet : allMet;
        }

        private void ActivateTarget()
        {
            if (hasSpawned) return;
            hasSpawned = true;

            targetObject.SetActive(true);

            Debug.Log($"[ConditionalSpawner] 已激活目标：{targetObject.name}");

            // 仅执行一次（如需要）
            if (markOnce && !string.IsNullOrEmpty(spawnerId))
                GameState.AddDisabledObject(spawnerId);
        }
    }
}
