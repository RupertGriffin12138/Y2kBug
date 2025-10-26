using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2D 对话触发器：靠近提示E，E开始对话，空格下一句；结束写入SaveManager并可销毁自身。
/// 依赖：InfoDialogUI, SaveManager, SaveData(dialogueSeenIds/HasSeenDialogue/TryMarkDialogueSeen)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger2D_Save : MonoBehaviour
{
    [Header("唯一ID（用于存档判重）")]
    public string dialogueId = "dlg_001";

    [Header("名字框（可空）")]
    public string speakerName = "NPC";

    [Header("对话内容（逐句）")]
    [TextArea(2, 3)]
    public List<string> lines = new List<string>
    {
        "你好，旅行者。",
        "按空格继续下一句。",
        "祝你旅途顺利！"
    };

    [Header("提示与按键")]
    [TextArea] public string hintPressE = "按 <b>E</b> 对话";
    public KeyCode interactKey = KeyCode.E;
    public KeyCode nextKey = KeyCode.Space;

    [Header("过滤")]
    public string playerTag = "Player";

    [Header("行为")]
    public bool blockWhenPaused = true;
    public bool destroyAfterFinish = true;

    private bool inside;
    private bool talking;
    private int idx;

    // 本地缓存一次SaveData，避免每次都反序列化
    private SaveData save;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Start()
    {
        // 载入存档（若无则给出默认：可按需改 firstSceneName）
        save = SaveManager.LoadOrDefault("Town");

        // 已完成过该对话则不再出现
        if (save.HasSeenDialogue(dialogueId))
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (save != null && save.HasSeenDialogue(dialogueId)) return;

        inside = true;

        if (InfoDialogUI.Instance)
        {
            if (!string.IsNullOrEmpty(speakerName))
                InfoDialogUI.Instance.SetNameText(speakerName);

            InfoDialogUI.Instance.ShowMessage(hintPressE);
            InfoDialogUI.Instance.ShowArrow();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        inside = false;

        if (!talking && InfoDialogUI.Instance)
            InfoDialogUI.Instance.Clear();
    }

    void Update()
    {
        if (blockWhenPaused && Time.timeScale == 0f) return;

        if (!talking)
        {
            if (inside && Input.GetKeyDown(interactKey))
                BeginTalk();
        }
        else
        {
            if (Input.GetKeyDown(nextKey))
                NextLine();
        }
    }

    void BeginTalk()
    {
        if (lines == null || lines.Count == 0) return;

        talking = true;
        idx = 0;

        if (InfoDialogUI.Instance)
        {
            InfoDialogUI.Instance.StartDialogue(); // 清空idle/箭头
            if (!string.IsNullOrEmpty(speakerName))
                InfoDialogUI.Instance.SetNameText(speakerName);

            InfoDialogUI.Instance.ShowMessage(lines[idx]);
        }
    }

    void NextLine()
    {
        idx++;
        if (idx < lines.Count)
        {
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.ShowMessage(lines[idx]);
        }
        else
        {
            EndTalk();
        }
    }

    void EndTalk()
    {
        talking = false;

        // 标记进度并保存
        if (save == null) save = SaveManager.LoadOrDefault("Town");
        if (save.TryMarkDialogueSeen(dialogueId))
        {
            SaveManager.Save(save);
        }

        if (InfoDialogUI.Instance)
            InfoDialogUI.Instance.EndDialogue();

        if (destroyAfterFinish)
            Destroy(gameObject);
        else
            inside = false; // 保底：不再重复提示
    }

    // 选中时可视化触发范围
    void OnDrawGizmosSelected()
    {
        var col = GetComponent<Collider2D>();
        if (!col) return;

        var prev = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(1f, 1f, 0f, 0.25f);

        if (col is BoxCollider2D b) Gizmos.DrawCube((Vector3)b.offset, (Vector3)b.size);
        if (col is CircleCollider2D c) Gizmos.DrawSphere((Vector3)c.offset, c.radius);

        Gizmos.matrix = prev;
    }
}
