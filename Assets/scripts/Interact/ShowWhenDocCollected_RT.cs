using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 条件出现 + 一次性交互后销毁（不含传送）
/// 满足文档条件才显示并可交互；使用一次后写入 GameState.disabledObjectIds，自动保存（可选），然后销毁自身。
/// 支持 2D/3D 触发器；可轮询实时刷新或外部调用 RefreshNow。
/// </summary>
public class ShowWhenDocCollected_RT : MonoBehaviour
{
    [Header("Condition")]
    [Tooltip("GameState 中的文档 id")]
    public string requiredDocId = "note1";

    [Tooltip("勾选：要求已阅读；不勾选：要求已收集")]
    public bool requireReadInsteadOfCollected = false;

    [Header("Disable persistence")]
    [Tooltip("一次性使用后写入的永久禁用 id；启动时若检测到该 id 已禁用，则本物体直接销毁。留空则自动读取同物体 SaveTag.id")]
    public string objectIdForDisableList = "";

    [Header("Targets")]
    [Tooltip("需要显隐的对象列表；默认不包含自身，避免把脚本宿主关掉")]
    public List<GameObject> targets = new List<GameObject>();

    [Tooltip("允许把自身 GameObject 也关掉。默认 false。若为 false 且 targets 中含有自身，则隐藏时只会关闭渲染和碰撞，不会 SetActive(false)")]
    public bool allowDeactivateSelf = false;

    [Header("Refresh policy")]
    [Tooltip("> 0 表示按该秒数轮询；= 0 表示仅在 OnEnable 和 RefreshNow 时评估")]
    public float refreshIntervalSeconds = 0.25f;

    [Tooltip("一旦显示过就不再因条件变为不满足而隐藏")]
    public bool onlyAppearNotDisappear = true;

    [Header("Startup policy")]
    [Tooltip("首次评估不满足时，立即销毁并停止后续逻辑")]
    public bool destroyIfNotMetOnStart = false;

    [Header("Interaction")]
    [Tooltip("玩家 Tag")]
    public string playerTag = "Player";

    [Tooltip("交互按键")]
    public KeyCode interactKey = KeyCode.E;

    [Tooltip("暂停时禁止交互")]
    public bool blockWhenPaused = true;

    [Tooltip("进入触发区的提示文本；留空则不显示")]
    public string interactHint = "Press E";

    [Tooltip("一次性使用后自动保存")]
    public bool autoSaveOnUse = true;

    // caches
    private SaveTag _autoSaveTag;
    private Collider2D[] _cols2DAll;
    private Collider[] _cols3DAll;
    private Renderer[] _renderersAll;

    // state
    private bool _lastVisible;
    private bool _hasAppliedOnce;
    private Coroutine _refreshCo;
    private bool _inside;
    private bool _consumed;

    void Reset()
    {
        var c3 = GetComponent<Collider>();
        if (c3) c3.isTrigger = true;
        var c2 = GetComponent<Collider2D>();
        if (c2) c2.isTrigger = true;
    }

    void Awake()
    {
        // 不再默认把自身加入 targets，避免把自己关掉导致协程无法运行。
        _cols2DAll = GetComponentsInChildren<Collider2D>(true);
        _cols3DAll = GetComponentsInChildren<Collider>(true);
        _renderersAll = GetComponentsInChildren<Renderer>(true);

        _autoSaveTag = GetComponent<SaveTag>();
        if (string.IsNullOrWhiteSpace(objectIdForDisableList) &&
            _autoSaveTag && !string.IsNullOrWhiteSpace(_autoSaveTag.id))
        {
            objectIdForDisableList = _autoSaveTag.id.Trim();
        }
    }

    void OnEnable()
    {
        if (GameState.Current == null)
            GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

        // 永久禁用优先
        var disableId = objectIdForDisableList?.Trim();
        if (!string.IsNullOrEmpty(disableId) && GameState.IsObjectDisabled(disableId))
        {
            Destroy(gameObject);
            return;
        }

        bool firstVisible = Evaluate();
        if (destroyIfNotMetOnStart && !firstVisible)
        {
            DestroyTargetsThenSelf();
            return;
        }
        Apply(firstVisible);

        // 仅在宿主仍处于激活且启用时才启动轮询
        if (refreshIntervalSeconds > 0f && isActiveAndEnabled)
            _refreshCo = StartCoroutine(AutoRefresh());
    }

    void OnDisable()
    {
        if (_refreshCo != null)
        {
            StopCoroutine(_refreshCo);
            _refreshCo = null;
        }
        if (InfoDialogUI.Instance) InfoDialogUI.Instance.Clear();
        _inside = false;
    }

    IEnumerator AutoRefresh()
    {
        var wfs = new WaitForSeconds(refreshIntervalSeconds);
        while (enabled)
        {
            if (onlyAppearNotDisappear && _hasAppliedOnce && _lastVisible)
            {
                yield return wfs;
                continue;
            }
            Apply(Evaluate());
            yield return wfs;
        }
    }

    public void RefreshNow()
    {
        if (onlyAppearNotDisappear && _hasAppliedOnce && _lastVisible)
            return;
        Apply(Evaluate());
    }

    bool Evaluate()
    {
        var disableId = objectIdForDisableList?.Trim();
        if (!string.IsNullOrEmpty(disableId) && GameState.IsObjectDisabled(disableId))
            return false;

        var data = GameState.Current;
        if (data == null) return false;
        data.EnsureArraysNotNull();

        var doc = requiredDocId?.Trim();
        if (string.IsNullOrEmpty(doc)) return false;

        if (requireReadInsteadOfCollected)
            return System.Array.IndexOf(data.docReadIds, doc) >= 0;
        else
            return System.Array.IndexOf(data.docCollectedIds, doc) >= 0;
    }

    void Apply(bool visible)
    {
        // only-appear-not-disappear
        if (onlyAppearNotDisappear && _hasAppliedOnce && _lastVisible && !visible)
            return;

        // 显隐目标。若目标是自身且不允许关自己，则跳过 SetActive(false)
        for (int i = 0; i < targets.Count; i++)
        {
            var t = targets[i];
            if (!t) continue;

            if (t == gameObject && !allowDeactivateSelf && !visible)
            {
                // 仅关闭渲染和碰撞，不关物体本身，确保本脚本仍能运行
                ToggleArray(_renderersAll, false);
                ToggleArray(_cols2DAll, false);
                ToggleArray(_cols3DAll, false);
            }
            else
            {
                t.SetActive(visible);
            }
        }

        // 如果 targets 不含自身，仍需根据 visible 统一控制自身的渲染/碰撞可见性
        if (!targets.Contains(gameObject))
        {
            ToggleArray(_renderersAll, visible);
            ToggleArray(_cols2DAll, visible);
            ToggleArray(_cols3DAll, visible);
        }

        if (!visible && _inside && InfoDialogUI.Instance) InfoDialogUI.Instance.Clear();

        _lastVisible = visible;
        _hasAppliedOnce = true;
    }

    void Update()
    {
        if (!_lastVisible) return;
        if (_consumed) return;
        if (!_inside) return;
        if (blockWhenPaused && Time.timeScale == 0f) return;

        if (Input.GetKeyDown(interactKey))
        {
            ConsumeOnce();
        }
    }

    void ConsumeOnce()
    {
        if (_consumed) return;
        _consumed = true;

        // 先关闭自身全部碰撞，避免同帧重复触发
        ToggleArray(_cols2DAll, false);
        ToggleArray(_cols3DAll, false);

        var disableId = objectIdForDisableList?.Trim();
        if (!string.IsNullOrEmpty(disableId))
            GameState.AddDisabledObject(disableId);

        if (autoSaveOnUse)
            GameState.SaveNow();

        if (InfoDialogUI.Instance)
        {
            InfoDialogUI.Instance.ShowMessage("Done");
            InfoDialogUI.Instance.HideArrow();
        }

        DestroyTargetsThenSelf();
    }

    void DestroyTargetsThenSelf()
    {
        if (InfoDialogUI.Instance) InfoDialogUI.Instance.Clear();

        // 先销毁外部目标
        for (int i = 0; i < targets.Count; i++)
        {
            var t = targets[i];
            if (!t) continue;
            if (t == gameObject) continue; // 避免两次销毁
            Destroy(t);
        }

        // 最后销毁自身
        Destroy(gameObject);
    }

    // 3D 触发
    void OnTriggerEnter(Collider other)
    {
        if (!_lastVisible) return;
        if (!other.CompareTag(playerTag)) return;
        _inside = true;

        if (InfoDialogUI.Instance && !string.IsNullOrEmpty(interactHint))
        {
            InfoDialogUI.Instance.SetNameText("");
            InfoDialogUI.Instance.ShowMessage(interactHint);
            InfoDialogUI.Instance.ShowArrow();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        _inside = false;
        if (InfoDialogUI.Instance) InfoDialogUI.Instance.Clear();
    }

    // 2D 触发
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_lastVisible) return;
        if (!other.CompareTag(playerTag)) return;
        _inside = true;

        if (InfoDialogUI.Instance && !string.IsNullOrEmpty(interactHint))
        {
            InfoDialogUI.Instance.SetNameText("");
            InfoDialogUI.Instance.ShowMessage(interactHint);
            InfoDialogUI.Instance.ShowArrow();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        _inside = false;
        if (InfoDialogUI.Instance) InfoDialogUI.Instance.Clear();
    }

    // helpers
    static void ToggleArray(Collider2D[] arr, bool on)
    {
        if (arr == null) return;
        for (int i = 0; i < arr.Length; i++)
            if (arr[i]) arr[i].enabled = on;
    }

    static void ToggleArray(Collider[] arr, bool on)
    {
        if (arr == null) return;
        for (int i = 0; i < arr.Length; i++)
            if (arr[i]) arr[i].enabled = on;
    }

    static void ToggleArray(Renderer[] arr, bool on)
    {
        if (arr == null) return;
        for (int i = 0; i < arr.Length; i++)
            if (arr[i]) arr[i].enabled = on;
    }
}
