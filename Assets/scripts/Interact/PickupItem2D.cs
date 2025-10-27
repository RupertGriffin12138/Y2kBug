using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class PickupItem2D : MonoBehaviour
{
    [Header("拾取配置")]
    [Tooltip("ItemDB 里的物品 id")]
    public string itemId = "sparkler";
    public int amount = 1;
    public KeyCode pickupKey = KeyCode.E;
    [Tooltip("拾取后销毁该物体；否则仅 SetActive(false)（会在对话播放完后执行）")]
    public bool destroyOnPickup = true;

    [Header("提示UI（可选）")]
    [TextArea] public string promptString = "按 <b>E</b> 拾取";

    [Header("引用（可留空自动找）")]
    public InventoryLite inventory;   // 若不手动拖拽，会在场景中自动 Find
    public ItemDB itemDB;             // 若不设，则尝试用 inventory.itemDB

    [Header("Save")]
    [Tooltip("必须有稳定 id 才能跨读档保持隐藏")]
    public SaveTag tag;
    [Tooltip("拾取后是否自动保存（推荐：勾选）")]
    public bool autoSaveOnPickup = true;

    // ========== 拾取后自动对话 ==========
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;
        [TextArea(2, 3)] public string content;
    }

    [Header("拾取后自动对话")]
    [Tooltip("拾取完成后是否自动播放下面的对话")]
    public bool triggerDialogueOnPickup = true;

    [Tooltip("是否先弹一句『获得 XXX xN』，然后再进入下面的正式对话")]
    public bool showPickupToast = true;

    public List<DialogueLine> lines = new List<DialogueLine>
    {
        new DialogueLine{ speaker="旁白", content="你捡起了某样重要的东西……" },
        new DialogueLine{ speaker="？？？", content="这也许会派上用场。" },
    };

    [Header("对话参数")]
    public KeyCode nextKey = KeyCode.Space;

    [Tooltip("逐字机每个字符的延时（秒），使用实时计时，不受 Time.timeScale 影响")]
    public float typeCharDelay = 0.04f;

    [Tooltip("最后一句打完后是否自动关闭（否则显示箭头，等待你按键结束）")]
    public bool autoCloseOnLastLine = false;   // 默认：不自动关，显示箭头等待按键

    [Tooltip("仅当自动关闭开启时有效：最后一句自动关闭前的延时（秒，实时）")]
    public float autoCloseDelay = 0.8f;

    [Header("输入防穿透")]
    [Tooltip("进入新一句后，忽略该时长内的按键（秒，实时）")]
    public float inputLockDuration = 0.15f;

    // ===== 内部状态 =====
    bool _playerInRange = false;
    bool _consumed = false;           // 防止重复执行（仅指拾取逻辑）
    Coroutine typeRoutine;
    bool lineFullyShown;
    int idx; // 当前对话索引
    bool _talking = false;
    float _inputLockUntil = 0f;       // 防按键穿透

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
        tag = GetComponent<SaveTag>(); // 方便自动挂上
    }

    void Start()
    {
        // 自动补引用
        if (!inventory) inventory = FindObjectOfType<InventoryLite>();
        if (!itemDB && inventory) itemDB = inventory.itemDB;

        // 确保 GameState 可用
        if (GameState.Current == null)
            GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

        // 读档应用：若该对象已被禁用，直接隐藏并不再工作
        if (tag && !string.IsNullOrEmpty(tag.id) && GameState.IsObjectDisabled(tag.id))
        {
            gameObject.SetActive(false);
            _consumed = true;
            return;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_consumed) return;
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.ShowMessage(promptString);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (_consumed) return;
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.Clear(); // 这里只清“提示”，不会影响对话框
        }
    }

    void Update()
    {
        // —— 1) 对话推进（即使已consumed也允许推进） —— //
        if (_talking && Time.unscaledTime >= _inputLockUntil && Input.GetKeyDown(nextKey))
        {
            if (!lineFullyShown) ShowLineInstant();
            else NextLine();
            return;
        }

        // —— 2) 已拾取后，不再进入拾取逻辑 —— //
        if (_consumed) return;

        // —— 3) 正常拾取入口 —— //
        if (_playerInRange && Input.GetKeyDown(pickupKey))
            TryPickup();
    }

    void TryPickup()
    {
        if (_consumed) return;

        // 基础校验
        if (string.IsNullOrWhiteSpace(itemId) || amount == 0)
        {
            Debug.LogWarning($"[PickupItem2D] 无效的 itemId 或数量：{itemId}, {amount}", this);
            return;
        }
        if (!inventory)
        {
            Debug.LogWarning("[PickupItem2D] 未找到 InventoryLite，无法拾取。", this);
            return;
        }

        // 1) 更新背包
        int newCount = inventory.Add(itemId, amount);

        // 2) 计算展示名称
        string displayName = itemId;
        if (itemDB)
        {
            var def = itemDB.Get(itemId);
            if (def != null && !string.IsNullOrWhiteSpace(def.displayName))
                displayName = def.displayName;
        }

        // 3) —— 写回 GameState —— 
        GameState.AddItem(itemId, amount);
        if (tag && !string.IsNullOrEmpty(tag.id))
            GameState.AddDisabledObject(tag.id);
        GameState.Current.lastScene = SceneManager.GetActiveScene().name;

        if (autoSaveOnPickup)
            GameState.SaveNow();

        // 4) 启动拾取后的演出：提示 + 对话（播放完再销毁/隐藏）
        StartCoroutine(PickupFlow(displayName));
    }

    IEnumerator PickupFlow(string displayName)
    {
        _consumed = true; // 防止重复触发

        // 清掉交互提示
        if (InfoDialogUI.Instance)
            InfoDialogUI.Instance.Clear();

        // （可选）先给个获得提示
        if (showPickupToast && InfoDialogUI.Instance)
        {
            InfoDialogUI.Instance.ShowMessage($"获得 {displayName} x{amount}");
            yield return new WaitForSecondsRealtime(0.5f);
        }

        if (triggerDialogueOnPickup && lines != null && lines.Count > 0 && InfoDialogUI.Instance)
        {
            // 播放对话
            _talking = true;
            idx = 0;
            InfoDialogUI.Instance.StartDialogue();

            // 进入第一句前，短暂锁输入，避免“拾取键”穿透
            _inputLockUntil = Time.unscaledTime + inputLockDuration;

            ShowCurrentLineTyped();

            // 等待对话结束
            while (_talking) yield return null;
        }

        // 最后处理自身
        if (destroyOnPickup) Destroy(gameObject);
        else gameObject.SetActive(false);
    }

    // ====== 对话播放（最小实现，复用你的 InfoDialogUI） ======
    void ShowCurrentLineTyped()
    {
        if (!InfoDialogUI.Instance) { EndNow(); return; }

        // 停止上一条
        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }

        var line = lines[idx];

        // 显示名字（旁白则清空）
        InfoDialogUI.Instance.SetNameText(string.Equals(line.speaker, "旁白") ? "" : line.speaker);

        // 清空文本，开始打字
        InfoDialogUI.Instance.textBoxText.text = "";
        InfoDialogUI.Instance.HideArrow();
        lineFullyShown = false;

        // 进入新一句时，加一小段输入冷却，避免上一帧按键穿透
        _inputLockUntil = Time.unscaledTime + inputLockDuration;

        typeRoutine = StartCoroutine(Typewriter(line.content));
    }

    IEnumerator Typewriter(string content)
    {
        foreach (char c in content)
        {
            InfoDialogUI.Instance.textBoxText.text += c;

            // —— 使用实时计时，避免 Time.timeScale 影响 —— 
            float t = 0f;
            while (t < typeCharDelay)
            {
                // 允许用户中途快进
                if (Time.unscaledTime >= _inputLockUntil && Input.GetKeyDown(nextKey))
                {
                    InfoDialogUI.Instance.textBoxText.text = content;
                    t = typeCharDelay; // 结束外层循环
                    break;
                }
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        lineFullyShown = true;

        bool isLast = (idx >= lines.Count - 1);
        if (isLast && autoCloseOnLastLine)
        {
            // 自动关闭分支：不再显示箭头，稍等后自动结束
            InfoDialogUI.Instance.HideArrow();
            if (autoCloseDelay > 0f)
                yield return new WaitForSecondsRealtime(autoCloseDelay);
            EndNow();
        }
        else
        {
            // 非自动关闭（或不是最后一句）→ 显示箭头等待玩家按键
            InfoDialogUI.Instance.ShowArrow();
        }

        typeRoutine = null;
    }

    void ShowLineInstant()
    {
        if (!InfoDialogUI.Instance) { EndNow(); return; }

        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }

        InfoDialogUI.Instance.textBoxText.text = lines[idx].content;
        lineFullyShown = true;

        bool isLast = (idx >= lines.Count - 1);
        if (isLast && autoCloseOnLastLine)
        {
            InfoDialogUI.Instance.HideArrow();
            if (autoCloseDelay > 0f)
                StartCoroutine(AutoCloseAfterDelay());
            else
                EndNow();
        }
        else
        {
            InfoDialogUI.Instance.ShowArrow();
        }
    }

    IEnumerator AutoCloseAfterDelay()
    {
        yield return new WaitForSecondsRealtime(autoCloseDelay);
        EndNow();
    }

    void NextLine()
    {
        idx++;
        if (idx < lines.Count)
        {
            ShowCurrentLineTyped();
        }
        else
        {
            // 最后一行：如果不自动关闭，则此处由玩家按键来到这里 → 正常结束
            EndNow();
        }
    }

    void EndNow()
    {
        _talking = false;
        if (InfoDialogUI.Instance) InfoDialogUI.Instance.EndDialogue();
    }
}
