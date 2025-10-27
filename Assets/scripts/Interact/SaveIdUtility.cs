using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Interact
{
    public static class SaveIdUtility
    {
        /// <summary>
        /// 生成可读ID：scene.category.objectName_suffix
        /// 例：town.pickup.Apple_(d4f2a7)
        /// 注意：这是 demo 友好的“人类可读”ID，不是严格GUID。
        /// </summary>
        public static string GenerateReadableId(GameObject go, string categoryHint = "obj")
        {
            string scene = SceneManager.GetActiveScene().name;
            if (string.IsNullOrWhiteSpace(scene))
                scene = "scene";

            string name = go.name;

            // 规范化：小写scene，小写category，name去空格
            scene = NormalizeToken(scene, toLower: true);
            categoryHint = NormalizeToken(categoryHint, toLower: true);
            name = NormalizeToken(name, toLower: false);

            // 追加一个短随机后缀，降低重复概率（保留人类可读性）
            string shortSuffix = Guid.NewGuid().ToString("N").Substring(0, 6);

            return $"{scene}.{categoryHint}.{name}_({shortSuffix})";
        }

        private static string NormalizeToken(string input, bool toLower)
        {
            if (string.IsNullOrWhiteSpace(input)) return "null";
            string s = input.Trim();
            if (toLower) s = s.ToLowerInvariant();
            // 替换空白和非法字符为下划线
            s = Regex.Replace(s, @"\s+", "_");
            s = Regex.Replace(s, @"[^A-Za-z0-9_\-\.]", "_");
            return s;
        }
    }
}
