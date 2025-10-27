#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Interact;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Save
{
    public static class SaveTagEditor
    {
        [MenuItem("Tools/Save System/为选中对象添加或生成 SaveTag ID")]
        public static void AddOrGenerateForSelection()
        {
            var selection = Selection.gameObjects;
            if (selection == null || selection.Length == 0)
            {
                EditorUtility.DisplayDialog("SaveTag", "请先在层级中选中一个或多个对象。", "好的");
                return;
            }

            int created = 0, generated = 0;
            foreach (var go in selection)
            {
                var tag = go.GetComponent<SaveTag>();
                if (!tag)
                {
                    tag = Undo.AddComponent<SaveTag>(go);
                    created++;
                }
                if (string.IsNullOrWhiteSpace(tag.id))
                {
                    tag.id = SaveIdUtility.GenerateReadableId(go, tag.categoryHint);
                    generated++;
                    EditorUtility.SetDirty(tag);
                }
            }

            Debug.Log($"[SaveTag] 添加组件 {created} 个，生成ID {generated} 个。");
        }

        [MenuItem("Tools/Save System/扫描当前场景的ID并检查重复")]
        public static void ScanSceneForDuplicateIds()
        {
            var scene = SceneManager.GetActiveScene();
            var all = Object.FindObjectsByType<SaveTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var map = new Dictionary<string, List<SaveTag>>();

            foreach (var t in all)
            {
                if (string.IsNullOrWhiteSpace(t.id)) continue;
                if (!map.ContainsKey(t.id)) map[t.id] = new List<SaveTag>();
                map[t.id].Add(t);
            }

            var duplicates = map.Where(kv => kv.Value.Count > 1).ToList();
            if (duplicates.Count == 0)
            {
                EditorUtility.DisplayDialog("SaveTag", $"场景 {scene.name} 未发现重复ID。", "太好了");
                return;
            }

            string msg = $"场景 {scene.name} 发现重复ID：\n\n";
            foreach (var kv in duplicates)
            {
                msg += $"- {kv.Key} ×{kv.Value.Count}\n";
                foreach (var t in kv.Value) msg += $"    · {t.gameObject.name} (Path: {GetHierarchyPath(t.transform)})\n";
            }

            Debug.LogWarning(msg);
            EditorUtility.DisplayDialog("SaveTag - 重复ID", "控制台已输出重复ID详情，请修正。", "我去改");
        }

        private static string GetHierarchyPath(Transform t)
        {
            var path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    }
}
#endif
