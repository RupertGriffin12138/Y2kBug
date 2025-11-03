using System.Collections.Generic;
using Condition;
using UnityEngine;
using UnityEditor;
using Items;

namespace Save
{
    /// <summary>
    /// 调试器：允许开发者在运行时一键发任意物品、文档或对白已读。
    /// 可直接挂在场景中的空物体上。
    /// </summary>
    public class GameStateDebugger : MonoBehaviour
    {
        [System.Serializable]
        public class ItemGrant
        {
            public string itemId = "key001";
            public int amount = 1;
            public bool showToast = true;
        }

        [System.Serializable]
        public class DocGrant
        {
            public string docId = "note1";
            public bool showToast = true;
        }

        [Header("物品发放（最多5个）")]
        public List<ItemGrant> items = new()
        {
            new ItemGrant { itemId = "key_strange_door", amount = 1 },
            new ItemGrant { itemId = "sparkler", amount = 1 },
            new ItemGrant { itemId = "key_school_door", amount = 1 },
            new ItemGrant { itemId = "second_hand", amount = 1 },
            new ItemGrant { itemId = "school_uniform", amount = 1 }
        };

        [Header("文档发放（最多7个）")]
        public List<DocGrant> docs = new()
        {
            new DocGrant { docId = "note1" },
            new DocGrant { docId = "note2" },
            new DocGrant { docId = "notice" },
            new DocGrant { docId = "teach" },
            new DocGrant { docId = "diary1" },
            new DocGrant { docId = "diary2" },
            new DocGrant { docId = "guard" },
        };

        [Header("对白标记")]
        public string dialogueIdToMark = "dlg_001";

        [Header("全局选项")]
        public bool autoUnlockBackpack = false;
        public bool clearSaveBeforeTest = false;

        private void Start()
        {
            if (GameState.Current == null)
                GameState.LoadGameOrNew("Town");
        }

        private List<(string speaker, string content)> DefaultLines()
        {
            return new()
            {
                ("旁白", "你获得了一件奇怪的东西……"),
                ("姜宁", "这是什么？"),
                ("祝榆", "看起来像一把钥匙。")
            };
        }

        private void GrantItem(string id, int amount, bool showToast)
        {
            if (string.IsNullOrEmpty(id)) return;

            if (clearSaveBeforeTest)
            {
                Debug.LogWarning("[GameStateDebugger] 清空存档以开始测试");
                GameState.Wipe();
                GameState.LoadGameOrNew("Town");
            }

            if (autoUnlockBackpack)
                GameState.UnlockBackpack();

            ItemGrantTool.GiveItem(id, amount, showToast, DefaultLines());
            Debug.Log($"[GameStateDebugger] 已发放物品：{id} x{amount}");
        }

        private void GrantDoc(string id, bool showToast)
        {
            if (string.IsNullOrEmpty(id)) return;

            if (clearSaveBeforeTest)
            {
                Debug.LogWarning("[GameStateDebugger] 清空存档以开始测试");
                GameState.Wipe();
                GameState.LoadGameOrNew("Town");
            }

            bool isNew = System.Array.IndexOf(GameState.Current.docCollectedIds, id) < 0;
            DocGrantTool.GiveDoc(id, true, showToast, DefaultLines());
            GameState.SaveNow();

            if (showToast && UI.InfoDialogUI.Instance)
            {
                string msg = isNew ? $"获得《{id}》" : $"已收录《{id}》";
                UI.InfoDialogUI.Instance.ShowMessage(msg);
            }

            foreach (var spawner in Object.FindObjectsOfType<ConditionalSpawner>())
                spawner.TryCheckNow();

            Debug.Log($"[GameStateDebugger] 已发放文档：{id}");
        }

        private void MarkDialogueSeen(string dialogueId)
        {
            if (string.IsNullOrEmpty(dialogueId))
            {
                Debug.LogWarning("[GameStateDebugger] 对话ID为空，已跳过。");
                return;
            }

            bool added = GameState.TryMarkDialogueSeen(dialogueId);
            if (added)
                Debug.Log($"[GameStateDebugger] 已标记对白已读：{dialogueId}");
            else
                Debug.Log($"[GameStateDebugger] 白已存在：{dialogueId}");

            foreach (var spawner in Object.FindObjectsOfType<ConditionalSpawner>())
                spawner.TryCheckNow();
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

            GUILayout.Label("=== 物品发放 ===", EditorStyles.boldLabel);
            foreach (var it in dbg.items)
            {
                if (GUILayout.Button($"发放物品 {it.itemId} x{it.amount}"))
                    dbg.GetType().GetMethod("GrantItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.Invoke(dbg, new object[] { it.itemId, it.amount, it.showToast });

            }

            GUILayout.Space(5);
            GUILayout.Label("=== 文档发放 ===", EditorStyles.boldLabel);
            foreach (var d in dbg.docs)
            {
                if (GUILayout.Button($"发放文档 {d.docId}"))
                    dbg.SendMessage("GrantDoc", new object[] { d.docId, d.showToast });
            }

            GUILayout.Space(5);
            GUILayout.Label("=== 白标记 ===", EditorStyles.boldLabel);
            if (GUILayout.Button("标记对白为已读"))
                dbg.SendMessage("MarkDialogueSeen", dbg.dialogueIdToMark);

            GUILayout.Space(10);
            if (GUILayout.Button("清空存档并重新加载"))
            {
                GameState.Wipe();
                GameState.LoadGameOrNew("Town");
                Debug.Log("[GameStateDebugger] 存档已清空并重置。");
            }
        }
    }
#endif
}
