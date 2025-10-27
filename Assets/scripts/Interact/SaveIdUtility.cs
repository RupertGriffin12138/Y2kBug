using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Interact
{
    public static class SaveIdUtility
    {
        /// <summary>
        /// ���ɿɶ�ID��scene.category.objectName_suffix
        /// ����town.pickup.Apple_(d4f2a7)
        /// ע�⣺���� demo �Ѻõġ�����ɶ���ID�������ϸ�GUID��
        /// </summary>
        public static string GenerateReadableId(GameObject go, string categoryHint = "obj")
        {
            string scene = SceneManager.GetActiveScene().name;
            if (string.IsNullOrWhiteSpace(scene))
                scene = "scene";

            string name = go.name;

            // �淶����Сдscene��Сдcategory��nameȥ�ո�
            scene = NormalizeToken(scene, toLower: true);
            categoryHint = NormalizeToken(categoryHint, toLower: true);
            name = NormalizeToken(name, toLower: false);

            // ׷��һ���������׺�������ظ����ʣ���������ɶ��ԣ�
            string shortSuffix = Guid.NewGuid().ToString("N").Substring(0, 6);

            return $"{scene}.{categoryHint}.{name}_({shortSuffix})";
        }

        private static string NormalizeToken(string input, bool toLower)
        {
            if (string.IsNullOrWhiteSpace(input)) return "null";
            string s = input.Trim();
            if (toLower) s = s.ToLowerInvariant();
            // �滻�հ׺ͷǷ��ַ�Ϊ�»���
            s = Regex.Replace(s, @"\s+", "_");
            s = Regex.Replace(s, @"[^A-Za-z0-9_\-\.]", "_");
            return s;
        }
    }
}
