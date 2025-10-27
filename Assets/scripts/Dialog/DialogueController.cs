using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;

public class DialogueController : MonoBehaviour
{
    public InfoDialogUI infoDialogUI; // 引用InfoDialogUI实例

    private string[] scriptLines; // 剧本内容数组
    private int[] cartoonIndices; // 对应每个卡通对象的台词索引
    private int currentIndex = 0; // 当前对话索引

    void Start()
    {
        if (infoDialogUI == null)
        {
            Debug.LogError("InfoDialogUI is not assigned.");
            return;
        }

        LoadScriptFromJson();
        infoDialogUI.StartDialogue();
        StartCoroutine(ShowDialogue());
    }

    void LoadScriptFromJson()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string filePath = $"Assets/Scripts/Dialog/{sceneName}.json";

        if (!File.Exists(filePath))
        {
            Debug.LogError($"JSON file for scene '{sceneName}' does not exist at path: {filePath}");
            return;
        }

        string jsonContent = File.ReadAllText(filePath);
        var scriptData = JsonUtility.FromJson<ScriptData>(jsonContent);

        scriptLines = scriptData.lines;
        cartoonIndices = scriptData.cartoonIndices;
    }

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

            if (System.Array.IndexOf(cartoonIndices, currentIndex) >= 0)
            {
                int cartoonIndex = System.Array.IndexOf(cartoonIndices, currentIndex);
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
    }

    [System.Serializable]
    private class ScriptData
    {
        public string[] lines;
        public int[] cartoonIndices;
    }
}


