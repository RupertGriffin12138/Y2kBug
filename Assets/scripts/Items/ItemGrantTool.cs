using System.Collections;
using System.Collections.Generic;
using Interact;
using Save;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Items
{
    public static class ItemGrantTool
    {
        /// <summary>
        /// 直接给玩家添加物品并可附带对白（不依赖触发器）
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="amount">数量</param>
        /// <param name="showToast">是否显示获得提示</param>
        /// <param name="lines">对白内容（speaker, content）</param>
        public static void GiveItem(
            string itemId,
            int amount = 1,
            bool showToast = true,
            List<(string speaker, string content)> lines = null)
        {
            if (string.IsNullOrEmpty(itemId) || amount == 0)
            {
                Debug.LogWarning("[ItemGrantTool] 无效的物品 ID 或数量。");
                return;
            }

            // 找到背包与数据库
            var inventory = Object.FindObjectOfType<InventoryLite>();
            var itemDB = inventory ? inventory.itemDB : Object.FindObjectOfType<ItemDB>();

            // === 添加到玩家背包 ===
            if (inventory)
                inventory.Add(itemId, amount);

            // === 写入存档 ===
            GameState.AddItem(itemId, amount);
            GameState.Current.lastScene = SceneManager.GetActiveScene().name;
            GameState.SaveNow();

            // === 获取显示名称 ===
            string displayName = itemId;
            if (itemDB)
            {
                var def = itemDB.Get(itemId);
                if (def != null && !string.IsNullOrWhiteSpace(def.displayName))
                    displayName = def.displayName;
            }

            // === 提示获得物品 ===
            if (showToast && InfoDialogUI.Instance)
            {
                InfoDialogUI.Instance.ShowMessage($"获得 {displayName} x{amount}");
            }

            // === 若有对白内容，启动对白系统 ===
            if (lines is { Count: > 0 } && InfoDialogUI.Instance)
            {
                bool finished = false;
                InfoDialogUI.Instance.BeginDialogue(lines, () => finished = true);
                // 启动协程等待对白结束（需要Mono支持）
                InfoDialogUI.Instance.StartCoroutine(WaitAndUnlock(finished));
            }

            // === 通知 ConditionalSpawner 更新 ===
            foreach (var spawner in Object.FindObjectsOfType<ConditionalSpawner>())
                spawner.TryCheckNow();

            Debug.Log($"[ItemGrantTool] 玩家获得物品：{displayName} x{amount}");
        }

        private static IEnumerator WaitAndUnlock(bool finished)
        {
            yield return new WaitUntil(() => finished);
        }
    }
}
