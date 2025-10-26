using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;   // 新增

public class DialogueController : MonoBehaviour
{
    public InfoDialogUI infoDialogUI; // 引用InfoDialogUI实例

    [Header("对白完成后要加载的场景")]
    public string nextSceneName = "C1S1 campus";  // 这里填你的目标场景名（需加入 Build Settings）

    [Header("对话结束后是否先等待/淡出")]
    public float afterDialogueDelay = 0.5f;       // 小停顿（可为0）
    public bool useFadeOut = false;               // 若你有全屏CanvasGroup做黑场，可勾上
    public CanvasGroup fadeCanvas;                // （可选）全屏黑幕的CanvasGroup（alpha 0→1）

    // 剧本内容数组，格式为 "人物名称：台词"
    private string[] scriptLines = {
        "丁家俊：几点了现在，好了没啊，到底还有多久啊？",
        "旁白：操场边上的男孩来回踱步，不时地凑到同伴身旁，目光落在那块儿新式手表上。",
        "祝榆：丁家俊，刚到现在就三分钟，你问了不下十次了，该来的总会来的，老这么急干嘛。",
        "丁家俊：我这不是激动坏了嘛......",
        "陈雪莹：小宁，烟花在你那儿吗，我想许个愿。",
        "旁白：戴细框眼镜的女生看向一旁，声音和轻风一同灌入众人耳中，像是刚刚一直远望的天空中，几颗星星间的私语",
        "姜宁（开心表情）：那当然，我带着呢，来，给你们！",
        "丁家俊：我我我我！给我一个，我妈说这叫世纪愿望，准灵！",
        "祝榆：别挤我。",
        "旁白：烟花棒被传着递到每一双手里。夜空下，四个高中生闭上双眼，任思绪在新世纪的畅想中漫游，静候那个注定载入历史的时刻。",
        "旁白：......",
        "丁家俊：快别许愿了，睁眼！时间马上到了！ ",
        "旁白：衔着短促的话语，高中生们一齐睁开双眼，目光落处不知是地面、远方，还是近处的钟楼。新的时代，就要到来了。",
        "[烟花棒画面]",
        "姜宁：十。",
        "丁家俊：九！",
        "祝榆：八。",
        "陈雪莹：七。",
        "姜宁：六。五。四..三...二...一！21世纪快......"
    };

    // 对应每个卡通对象的台词索引
    private int[] cartoonIndices = { 0, 4, 6, 9, 11, 13 }; // 根据需要调整索引

    private int currentIndex = 0; // 当前对话索引

    void Start()
    {
        if (infoDialogUI == null)
        {
            Debug.LogError("InfoDialogUI is not assigned.");
            return;
        }

        infoDialogUI.StartDialogue();
        StartCoroutine(ShowDialogue());
    }

    IEnumerator ShowDialogue()
    {
        while (currentIndex < scriptLines.Length)
        {
            string line = scriptLines[currentIndex];
            string name = "";
            string dialogue = "";

            // 分割每一行中的角色名称和对话内容
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

            // 处理特殊情况 "[烟花棒画面]"
            if (dialogue == "[烟花棒画面]")
            {
                infoDialogUI.DisableAllCartoonsWithFadeOut();
                infoDialogUI.EnableCartoon(infoDialogUI.cartoonObjects.Length - 1); // 启用最后一个卡通对象 (T_cartoon_6)

                infoDialogUI.ShowMessage(dialogue);
                yield return new WaitForSeconds(2f); // 等待一段时间后继续
                currentIndex++;
                continue;
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

            // 检查当前索引是否对应某个卡通对象
            if (System.Array.IndexOf(cartoonIndices, currentIndex) >= 0)
            {
                int cartoonIndex = System.Array.IndexOf(cartoonIndices, currentIndex);
                infoDialogUI.EnableCartoon(cartoonIndex);
            }

            // 逐字显示对话内容
            foreach (char c in dialogue.ToCharArray())
            {
                infoDialogUI.textBoxText.text += c;
                yield return new WaitForSeconds(0.05f); // 调整显示速度
            }

            // 显示箭头
            infoDialogUI.ShowArrow();

            // 等待玩家按下 E 键（你当前逻辑就是 E 继续）
            yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.E));

            // 隐藏箭头
            infoDialogUI.HideArrow();

            currentIndex++; // 移动到下一个对话
        }

        // 对话结束 → 关闭对话UI
        infoDialogUI.EndDialogue();

        // 可选：小等待 / 淡出
        if (afterDialogueDelay > 0f)
            yield return new WaitForSeconds(afterDialogueDelay);

        if (useFadeOut && fadeCanvas != null)
            yield return StartCoroutine(FadeToBlack(fadeCanvas, 0.35f));

        // 加载下一个场景
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[DialogueController] nextSceneName 为空，未切换场景。");
        }
    }

    // （可选）淡出到黑
    IEnumerator FadeToBlack(CanvasGroup cg, float duration)
    {
        cg.blocksRaycasts = true;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);
            cg.alpha = a;
            yield return null;
        }
        cg.alpha = 1f;
    }
}
