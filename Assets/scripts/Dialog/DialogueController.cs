using System;
using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dialog
{
    public class DialogueController : MonoBehaviour
    {
        public InfoDialogUI infoDialogUI; // 引用InfoDialogUI实例

        private string[] scriptLines; // 剧本内容数组
        private int[] cartoonIndices; // 对应每个卡通对象的台词索引
        private int currentIndex = 0; // 当前对话索引

        [Header("Scene Transition")]
        public bool loadNextSceneOnEnd = false;     // 播放结束后是否跳转
        public string nextSceneName = "";           // 下一个场景名（需在 Build Settings 中添加）
        public float nextSceneDelay = 0f;           // 跳转前延时（秒）

        void Start()
        {
            if (infoDialogUI == null)
            {
                Debug.LogError("InfoDialogUI is not assigned.");
                return;
            }

            // 改为 Resources 读取，避免打包后路径读盘失败
            if (!LoadScriptFromResources())
            {
                // 读不到剧本就直接结束，避免后续空引用
                infoDialogUI.EndDialogue();
                TryLoadNextSceneIfNeeded(); // 允许没有对话也跳场景
                return;
            }

            infoDialogUI.StartDialogue();
            StartCoroutine(ShowDialogue());
        }

        // ================== 改动 1：使用 Resources 读取 ==================
        // 需要把 json 放到 Assets/Resources/Dialog/ 下，文件名 = 场景名，不带 .json
        bool LoadScriptFromResources()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            string resPath = $"Dialog/{sceneName}"; // 对应 Assets/Resources/Dialog/{sceneName}.json

            TextAsset jsonAsset = Resources.Load<TextAsset>(resPath);
            if (jsonAsset == null)
            {
                Debug.LogError($"[Dialogue] TextAsset not found at Resources/{resPath}.json (place JSON at Assets/Resources/Dialog/)");
                return false;
            }

            var scriptData = JsonUtility.FromJson<ScriptData>(jsonAsset.text);
            if (scriptData == null)
            {
                Debug.LogError("[Dialogue] JSON parse failed.");
                return false;
            }

            scriptLines = scriptData.lines ?? Array.Empty<string>();
            cartoonIndices = scriptData.cartoonIndices ?? Array.Empty<int>();

            if (scriptLines.Length == 0)
            {
                Debug.LogWarning("[Dialogue] lines is empty.");
            }
            return true;
        }
        // ===============================================================

        IEnumerator ShowDialogue()
        {
            while (currentIndex < scriptLines.Length)
            {
                string line = scriptLines[currentIndex];
                string name = "";
                string dialogue = "";

                int colonIndex = line.IndexOf('：');
                if (colonIndex >= 0)
                {
                    name = line.Substring(0, colonIndex).Trim();
                    dialogue = line.Substring(colonIndex + 1).Trim();
                }
                else
                {
                    dialogue = line.Trim();
                }

                // 设置名字文本
                if (name == "旁白")
                {
                    infoDialogUI.SetNameText("");
                }
                else if (name == "姜宁（开心表情）" || name == "姜宁" || name == "姜宁（恐惧表情）")
                {
                    infoDialogUI.SetNameText("姜宁");
                }
                else
                {
                    infoDialogUI.SetNameText(name);
                }

                infoDialogUI.textBoxText.text = "";

                // 处理特殊情况 "[烟花棒画面]"
                // 这里保持你的原有判断，不改其他逻辑
                if (dialogue == "姜宁：十。")
                {
                    infoDialogUI.DisableAllCartoonsWithFadeOut();
                    infoDialogUI.EnableCartoon(infoDialogUI.cartoonObjects.Length - 1); // 启用最后一个卡通对象 (T_cartoon_6)

                    infoDialogUI.ShowMessage(dialogue);
                    yield return new WaitForSeconds(2f); // 等待一段时间后继续
                    currentIndex++;
                    continue;
                }

                // 根据人物名称启用相应的背景图像
                infoDialogUI.EnableCharacterBackground(name);

                if (Array.IndexOf(cartoonIndices, currentIndex) >= 0)
                {
                    int cartoonIndex = Array.IndexOf(cartoonIndices, currentIndex);
                    infoDialogUI.EnableCartoon(cartoonIndex);
                }

                foreach (char c in dialogue.ToCharArray())
                {
                    infoDialogUI.textBoxText.text += c;
                    yield return new WaitForSeconds(0.05f);
                }

                infoDialogUI.ShowArrow();
                yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.E));
                infoDialogUI.HideArrow();

                currentIndex++;
            }

            infoDialogUI.EndDialogue();

            // ========= 改动 2：结束后可选择切到下一个场景 =========
            TryLoadNextSceneIfNeeded();
        }

        void TryLoadNextSceneIfNeeded()
        {
            if (loadNextSceneOnEnd && !string.IsNullOrEmpty(nextSceneName))
            {
                StartCoroutine(LoadNextSceneCoroutine());
            }
        }

        IEnumerator LoadNextSceneCoroutine()
        {
            if (nextSceneDelay > 0f)
                yield return new WaitForSeconds(nextSceneDelay);

            // 确保在 Build Settings 中已添加 nextSceneName
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
        // =============================================================

        [Serializable]
        private class ScriptData
        {
            public string[] lines;
            public int[] cartoonIndices;
        }
    }
}
