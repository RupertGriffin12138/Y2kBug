using System.Collections;
using System.Collections.Generic;
using Interact;
using Save;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Items
{
    /// <summary>
    /// 发放文档工具（不依赖触发器）。
    /// 仿照 ItemGrantTool 结构实现。
    /// </summary>
    public static class DocGrantTool
    {
        /// <summary>
        /// 给玩家发放文档
        /// </summary>
        /// <param name="docId">文档ID（在 DocDB 中定义）</param>
        /// <param name="markRead">是否立即标记为已阅读</param>
        /// <param name="showToast">是否显示获得提示</param>
        /// <param name="lines">对白内容（speaker, content）</param>
        public static void GiveDoc(
            string docId,
            bool markRead = false,
            bool showToast = true,
            List<(string speaker, string content)> lines = null)
        {
            if (string.IsNullOrEmpty(docId))
            {
                Debug.LogWarning("[DocGrantTool] 无效的文档ID。");
                return;
            }

            // ==== 获取系统组件 ====
            var docInventory = Object.FindObjectOfType<DocInventoryLite>();
            var docDB = docInventory ? docInventory.docDB : Object.FindObjectOfType<DocDB>();

            if (!docInventory)
                Debug.LogWarning("[DocGrantTool] 未找到 DocInventoryLite，运行时不会更新可见列表。");

            // ==== 添加进存档 ====
            bool isNew = GameState.Current == null ||
                         System.Array.IndexOf(GameState.Current.docCollectedIds, docId) < 0;

            GameState.CollectDoc(docId);
            if (markRead)
                GameState.MarkDocRead(docId);

            GameState.Current.lastScene = SceneManager.GetActiveScene().name;
            GameState.SaveNow();

            // ==== 添加到运行态文档清单 ====
            if (docInventory)
                docInventory.AddOnce(docId);

            // ==== 获取显示名称 ====
            string displayName = docId;
            if (docDB)
            {
                var def = docDB.Get(docId);
                if (def != null && !string.IsNullOrWhiteSpace(def.displayName))
                    displayName = def.displayName;
            }

            // ==== 显示获得提示 ====
            if (showToast && InfoDialogUI.Instance)
            {
                string msg = isNew ? $"获得《{displayName}》" : $"已收录《{displayName}》";
                InfoDialogUI.Instance.ShowMessage(msg);
            }

            // ==== 对话播放 ====
            if (lines is { Count: > 0 } && InfoDialogUI.Instance)
            {
                bool finished = false;
                InfoDialogUI.Instance.BeginDialogue(lines, () => finished = true);
                InfoDialogUI.Instance.StartCoroutine(WaitUntilDialogueEnd(finished));
            }

            // ==== 通知 ConditionalSpawner ====
            foreach (var spawner in Object.FindObjectsOfType<ConditionalSpawner>())
                spawner.TryCheckNow();

            Debug.Log($"[DocGrantTool] 玩家获得文档：{displayName} (已读={markRead})");
        }

        private static IEnumerator WaitUntilDialogueEnd(bool finished)
        {
            yield return new WaitUntil(() => finished);
        }
    }
}
