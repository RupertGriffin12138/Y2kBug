using System.Collections.Generic;
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
            // 自动初始化存档
            if (GameState.Current == null)
                GameState.LoadGameOrNew("Town");
        }

        /// <summary>
        /// 发放单个物品
        /// </summary>
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
        
        /// <summary>
        /// 将“旁白: 内容”格式的字符串列表转成对白元组
        /// </summary>
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
    /// <summary>
    /// 自定义 Inspector 按钮
    /// </summary>
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

            if (GUILayout.Button("清空存档并重新加载"))
            {
                GameState.Wipe();
                GameState.LoadGameOrNew("Town");
                Debug.Log("[GameStateDebugger] 存档已清空并重置。");
            }
            
            if (GUILayout.Button("解密四个算盘"))
            {
                PlayerPrefs.SetInt("AbacusSolved1", 1);  // 1 表示已解开
                PlayerPrefs.SetInt("AbacusSolved2", 1);  // 1 表示已解开
                PlayerPrefs.SetInt("AbacusSolved3", 1);  // 1 表示已解开
                PlayerPrefs.SetInt("AbacusSolved4", 1);  // 1 表示已解开
                PlayerPrefs.Save(); // 立即写入硬盘
                Debug.Log("[GameStateDebugger] 已解密算盘");
            }
        }
    }
#endif
}
