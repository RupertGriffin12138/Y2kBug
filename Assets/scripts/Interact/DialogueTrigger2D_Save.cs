using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 2D 对话触发器（自动触发 + 多说话人 + 背景切换 + 打字机）
/// 进入触发区即开始，空格推进；每句包含 speaker 与 content；结束写入存档；可选销毁自身。
/// 依赖：InfoDialogUI(Instance/StartDialogue/EndDialogue/SetNameText/ShowMessage/textBoxText/EnableCharacterBackground/ShowArrow/HideArrow)、GameState、SaveData
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger2D_Save : MonoBehaviour
{
    [Header("唯一ID（用于存档判重）")]
    public string dialogueId = "dlg_001";

    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;
        [TextArea(2, 3)] public string content;
    }

    [Header("对话内容（每句包含人物+内容）")]
    public List<DialogueLine> lines = new List<DialogueLine>
    {
        new DialogueLine{ speaker = "旁白", content = "操场边上的男孩来回踱步……" },
        new DialogueLine{ speaker = "姜宁", content = "十。" },
        new DialogueLine{ speaker = "祝榆", content = "别急。" }
    };

    [Header("按键")]
    public KeyCode nextKey = KeyCode.Space;

    [Header("过滤")]
    public string playerTag = "Player";

    [Header("行为")]
    public bool blockWhenPaused = true;
    public bool destroyAfterFinish = true;

    [Header("打字机参数")]
    [Tooltip("每个字符的延时（秒）")]
    public float typeCharDelay = 0.05f;

    // 运行时状态
    private bool talking;
    private int idx;
    private Coroutine typeRoutine;
    private bool lineFullyShown;
    private bool begun;                 // 已经触发过（防止多次 BeginTalk）
    private Collider2D triggerCol;      // 用于谈话期间禁用，避免重复进入

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Awake()
    {
        triggerCol = GetComponent<Collider2D>();
    }

    void Start()
    {
        // 确保存档已加载到 GameState（全局唯一真相）
        if (GameState.Current == null)
            GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

        // 已看过则直接移除
        if (GameState.Current != null && GameState.Current.HasSeenDialogue(dialogueId))
        {
            if (destroyAfterFinish) Destroy(gameObject);
            else gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (begun) return;
        if (!other.CompareTag(playerTag)) return;

        // 再次判重（避免极端生命周期顺序问题）
        if (GameState.Current != null && GameState.Current.HasSeenDialogue(dialogueId)) return;

        BeginTalk();
    }

    void Update()
    {
        if (!talking) return;
        if (blockWhenPaused && Time.timeScale == 0f) return;

        if (Input.GetKeyDown(nextKey))
        {
            if (!lineFullyShown)
            {
                ShowLineInstant();
            }
            else
            {
                NextLine();
            }
        }
    }

    void BeginTalk()
    {
        if (lines == null || lines.Count == 0) return;

        begun = true;
        talking = true;
        idx = 0;

        // 谈话期间禁用触发器，避免重复触发
        if (triggerCol) triggerCol.enabled = false;

        if (InfoDialogUI.Instance)
            InfoDialogUI.Instance.StartDialogue();

        ShowCurrentLineTyped();
    }

    void NextLine()
    {
        idx++;
        if (idx < lines.Count)
            ShowCurrentLineTyped();
        else
            EndTalk();
    }

    void ShowCurrentLineTyped()
    {
        if (!InfoDialogUI.Instance) return;

        // 停掉上一次的打字协程
        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }

        var line = lines[idx];

        // 名字框：旁白不显示名字
        InfoDialogUI.Instance.SetNameText(string.Equals(line.speaker, "旁白") ? "" : line.speaker);

        // 按人物名称切换背景（如果实现了）
        InfoDialogUI.Instance.EnableCharacterBackground(line.speaker);

        // 清空并开始打字
        InfoDialogUI.Instance.textBoxText.text = "";
        lineFullyShown = false;

        InfoDialogUI.Instance.HideArrow();

        typeRoutine = StartCoroutine(Typewriter(line.content));
    }

    IEnumerator Typewriter(string content)
    {
        foreach (char c in content)
        {
            InfoDialogUI.Instance.textBoxText.text += c;
            yield return new WaitForSeconds(typeCharDelay);

            // 若在打字中按键，交由 Update 的逻辑立即补全
            if (Input.GetKeyDown(nextKey))
            {
                InfoDialogUI.Instance.textBoxText.text = content;
                break;
            }
        }

        lineFullyShown = true;
        InfoDialogUI.Instance.ShowArrow();
        typeRoutine = null;
    }

    void ShowLineInstant()
    {
        if (!InfoDialogUI.Instance) return;
        if (idx < 0 || idx >= lines.Count) return;

        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }

        InfoDialogUI.Instance.textBoxText.text = lines[idx].content;
        lineFullyShown = true;
        InfoDialogUI.Instance.ShowArrow();
    }

    void EndTalk()
    {
        talking = false;

        // —— 用 GameState 作为唯一真相写入 & 保存 —— //
        if (GameState.Current == null)
            GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

        if (GameState.Current != null && GameState.Current.TryMarkDialogueSeen(dialogueId))
        {
            GameState.SaveNow(); // 与全局系统一致的保存方式，避免被覆盖
        }

        if (InfoDialogUI.Instance)
            InfoDialogUI.Instance.EndDialogue();

        if (destroyAfterFinish)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
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
