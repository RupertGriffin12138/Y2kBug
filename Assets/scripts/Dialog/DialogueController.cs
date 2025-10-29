using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Scene;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dialog
{
    public class DialogueController : MonoBehaviour
    {
        [Header("剧本设置")]
        [Tooltip("不填则自动使用当前场景名（Resources/Dialog/{sceneName}.json）")]
        public string jsonFileName = "";

        [Header("结束行为")]
        public bool loadNextSceneOnEnd = false;
        public string nextSceneName = "";
        public float nextSceneDelay = 0f;

        private int _lastCartoonIndex = -1;
        private int[] cartoonIndices;

        [Serializable]
        private class ScriptData
        {
            public string[] lines;
            public int[] cartoonIndices;
        }

        private List<(string speaker, string content)> parsedLines;

        private void Start()
        {
            if (!InfoDialogUI.Instance)
            {
                Debug.LogError("[DialogueController] 未找到 InfoDialogUI 实例！");
                return;
            }

            string fileName = string.IsNullOrEmpty(jsonFileName)
                ? SceneManager.GetActiveScene().name
                : jsonFileName;

            string resPath = $"Dialog/{fileName}";
            TextAsset jsonAsset = Resources.Load<TextAsset>(resPath);

            if (jsonAsset == null)
            {
                Debug.LogError($"[DialogueController] 未找到 Resources/{resPath}.json");
                TryLoadNextSceneIfNeeded();
                return;
            }

            ScriptData data = JsonUtility.FromJson<ScriptData>(jsonAsset.text);
            if (data?.lines == null || data.lines.Length == 0)
            {
                Debug.LogWarning($"[DialogueController] 文件 {fileName}.json 内容为空。");
                TryLoadNextSceneIfNeeded();
                return;
            }

            cartoonIndices = data.cartoonIndices ?? Array.Empty<int>();
            if (cartoonIndices.Length > 0)
                _lastCartoonIndex = cartoonIndices[^1];

            // ========== 保留完整 speaker（含表情说明） ==========
            parsedLines = new List<(string speaker, string content)>();
            foreach (string raw in data.lines)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;

                string fullSpeaker = "";
                string content = raw;

                int colonIndex = raw.IndexOf('：');
                if (colonIndex >= 0)
                {
                    fullSpeaker = raw[..colonIndex].Trim();
                    content = raw[(colonIndex + 1)..].Trim();
                }

                parsedLines.Add((fullSpeaker == "旁白" ? "" : fullSpeaker, content));
            }

            // 注册事件
            InfoDialogUI.Instance.OnLineChanged += HandleLineChange;

            InfoDialogUI.Instance.BeginDialogue(parsedLines, OnDialogueEnd);
        }

        private void HandleLineChange(int idx)
        {
            if (parsedLines == null || idx < 0 || idx >= parsedLines.Count)
                return;

            var (speaker, _) = parsedLines[idx];

            // 去括号 只改显示名，不动背景
            string displaySpeaker = Regex.Replace(speaker, "（.*?）", "").Trim();

            if (speaker != "旁白" && !string.IsNullOrEmpty(displaySpeaker))
                InfoDialogUI.Instance.SetNameText(displaySpeaker);

            // cartoon 控制逻辑
            if (cartoonIndices is { Length: > 0 })
            {
                int cartoonIndex = Array.IndexOf(cartoonIndices, idx);
                if (cartoonIndex >= 0)
                {
                    if (idx == _lastCartoonIndex && cartoonIndex > 0)
                        InfoDialogUI.Instance.DisableAllCartoonsWithFadeOut();

                    InfoDialogUI.Instance.EnableCartoon(cartoonIndex);
                }
            }
        }

        private void OnDialogueEnd()
        {
            InfoDialogUI.Instance.OnLineChanged -= HandleLineChange;

            // 最后一个 cartoon 渐隐消失
            if (_lastCartoonIndex >= 0 && InfoDialogUI.Instance != null)
            {
                int lastIdx = Mathf.Min(_lastCartoonIndex, InfoDialogUI.Instance.cartoonObjects.Length - 1);
                var obj = InfoDialogUI.Instance.cartoonObjects[lastIdx];
                if (obj)
                    InfoDialogUI.Instance.StartCoroutine(InfoDialogUI.Instance.FadeOut(obj));
            }

            if (loadNextSceneOnEnd && !string.IsNullOrEmpty(nextSceneName))
                Invoke(nameof(LoadNextScene), nextSceneDelay);
        }

        private void LoadNextScene()
        {
            SceneFadeEffect sceneFadeEffect = FindObjectOfType<SceneFadeEffect>();
            sceneFadeEffect.FadeOutAndLoad(nextSceneName, 0.5f, 1.5f);
        }

        private void TryLoadNextSceneIfNeeded()
        {
            if (loadNextSceneOnEnd && !string.IsNullOrEmpty(nextSceneName))
                LoadNextScene();
        }
    }
}
