using System.Collections.Generic;
using Condition;
using Interact;
using Items;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Save
{
    /// <summary>
    /// 调试器：允许开发者在运行时一键发任意物品、文档、对话、背包等状态。
    /// 可直接挂在场景中的空物体上。
    /// </summary>
    public class GameStateDebugger : MonoBehaviour
    {
        [Header("物品发放")]
        [Tooltip("要发放的物品 ID（在 ItemDB 中定义）")]
        public string itemId = "key001";

        [Tooltip("发放数量")]
        public int itemAmount = 1;

        [Tooltip("是否显示获得提示")]
        public bool showToast = true;

        [Header("文档发放")]
        [Tooltip("要发放的文档 ID（在 DocDB 中定义）")]
        public string docId = "note1";

        [Tooltip("是否显示获得提示")]
        public bool showDocToast = true;

        [Header("测试对白（可选）")]
        [TextArea(2, 3)]
        public List<string> debugDialogueLines = new()
        {
            "旁白: 你获得了一件奇怪的东西……",
            "姜宁: 这是什么？",
            "祝榆: 看起来像一把钥匙。"
        };

        [Header("额外选项")]
        public bool autoUnlockBackpack = false;
        public bool clearSaveBeforeTest = false;
        
        private void Start()
        {
            if (GameState.Current == null)
                GameState.LoadGameOrNew("Town");
        }

        // ================== 发放物品 ==================
        [ContextMenu("Grant Item (单个)")]
        public void GrantItem()
        {
            if (clearSaveBeforeTest)
            {
                Debug.LogWarning("[GameStateDebugger] 清空存档以开始测试");
                GameState.Wipe();
                GameState.LoadGameOrNew("Town");
            }

            if (autoUnlockBackpack)
                GameState.UnlockBackpack();

            var lines = ParseDialogueLines(debugDialogueLines);
            ItemGrantTool.GiveItem(itemId, itemAmount, showToast, lines);
        }

        // ================== 发放文档 ==================
        [ContextMenu("Grant Document (单个)")]
        public void GrantDocument()
        {
            if (clearSaveBeforeTest)
            {
                Debug.LogWarning("[GameStateDebugger] 清空存档以开始测试");
                GameState.Wipe();
                GameState.LoadGameOrNew("Town");
            }

            if (string.IsNullOrEmpty(docId))
            {
                Debug.LogWarning("[GameStateDebugger] 文档ID为空，已跳过。");
                return;
            }

            if (GameState.Current == null)
                GameState.LoadGameOrNew("Town");

            bool isNew = System.Array.IndexOf(GameState.Current.docCollectedIds, docId) < 0;

            // 发放并打开阅读器
            DocGrantTool.GiveDoc(docId, true,showDocToast, ParseDialogueLines(debugDialogueLines));

            GameState.SaveNow();

            if (showDocToast && UI.InfoDialogUI.Instance)
            {
                string msg = isNew ? $"获得《{docId}》" : $"已收录《{docId}》";
                UI.InfoDialogUI.Instance.ShowMessage(msg);
            }

            Debug.Log($"[GameStateDebugger] 已发放文档：{docId} ");
            
            // === 通知 ConditionalSpawner 更新 ===
            foreach (var spawner in Object.FindObjectsOfType<ConditionalSpawner>())
                spawner.TryCheckNow();
        }

        // ================== 解析对白 ==================
        private List<(string speaker, string content)> ParseDialogueLines(List<string> raw)
        {
            var result = new List<(string speaker, string content)>();
            if (raw == null) return result;

            foreach (var line in raw)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                int colon = line.IndexOf(':');
                if (colon > 0)
                {
                    string speaker = line[..colon].Trim();
                    string content = line[(colon + 1)..].Trim();
                    result.Add((speaker, content));
                }
                else
                {
                    result.Add(("旁白", line.Trim()));
                }
            }
            return result;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GameStateDebugger))]
    public class GameStateDebuggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var dbg = (GameStateDebugger)target;
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("调试功能区", MessageType.Info);

            if (GUILayout.Button("立即发放单个物品"))
                dbg.GrantItem();

            if (GUILayout.Button("立即发放单个文档"))
                dbg.GrantDocument();

            if (GUILayout.Button("清空存档并重新加载"))
            {
                GameState.Wipe();
                GameState.LoadGameOrNew("Town");
                Debug.Log("[GameStateDebugger] 存档已清空并重置。");
            }

            if (GUILayout.Button("解密四个算盘"))
            {
                PlayerPrefs.SetInt("AbacusSolved_1", 1);
                PlayerPrefs.SetInt("AbacusSolved_2", 1);
                PlayerPrefs.SetInt("AbacusSolved_3", 1);
                PlayerPrefs.SetInt("AbacusSolved_4", 1);
                PlayerPrefs.Save();
                Debug.Log("[GameStateDebugger] 已解密算盘");
            }
        }
    }
#endif
}
