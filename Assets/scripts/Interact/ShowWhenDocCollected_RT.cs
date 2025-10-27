using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �������� + һ���Խ��������٣��������ͣ�
/// �����ĵ���������ʾ���ɽ�����ʹ��һ�κ�д�� GameState.disabledObjectIds���Զ����棨��ѡ����Ȼ����������
/// ֧�� 2D/3D ������������ѯʵʱˢ�»��ⲿ���� RefreshNow��
/// </summary>
public class ShowWhenDocCollected_RT : MonoBehaviour
{
    [Header("Condition")]
    [Tooltip("GameState �е��ĵ� id")]
    public string requiredDocId = "note1";

    [Tooltip("��ѡ��Ҫ�����Ķ�������ѡ��Ҫ�����ռ�")]
    public bool requireReadInsteadOfCollected = false;

    [Header("Disable persistence")]
    [Tooltip("һ����ʹ�ú�д������ý��� id������ʱ����⵽�� id �ѽ��ã�������ֱ�����١��������Զ���ȡͬ���� SaveTag.id")]
    public string objectIdForDisableList = "";

    [Header("Targets")]
    [Tooltip("��Ҫ�����Ķ����б�Ĭ�ϲ�������������ѽű������ص�")]
    public List<GameObject> targets = new List<GameObject>();

    [Tooltip("��������� GameObject Ҳ�ص���Ĭ�� false����Ϊ false �� targets �к�������������ʱֻ��ر���Ⱦ����ײ������ SetActive(false)")]
    public bool allowDeactivateSelf = false;

    [Header("Refresh policy")]
    [Tooltip("> 0 ��ʾ����������ѯ��= 0 ��ʾ���� OnEnable �� RefreshNow ʱ����")]
    public float refreshIntervalSeconds = 0.25f;

    [Tooltip("һ����ʾ���Ͳ�����������Ϊ�����������")]
    public bool onlyAppearNotDisappear = true;

    [Header("Startup policy")]
    [Tooltip("�״�����������ʱ���������ٲ�ֹͣ�����߼�")]
    public bool destroyIfNotMetOnStart = false;

    [Header("Interaction")]
    [Tooltip("��� Tag")]
    public string playerTag = "Player";

    [Tooltip("��������")]
    public KeyCode interactKey = KeyCode.E;

    [Tooltip("��ͣʱ��ֹ����")]
    public bool blockWhenPaused = true;

    [Tooltip("���봥��������ʾ�ı�����������ʾ")]
    public string interactHint = "Press E";

    [Tooltip("һ����ʹ�ú��Զ�����")]
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
        // ����Ĭ�ϰ�������� targets��������Լ��ص�����Э���޷����С�
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

        // ���ý�������
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

        // ���������Դ��ڼ���������ʱ��������ѯ
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

        // ����Ŀ�ꡣ��Ŀ���������Ҳ�������Լ��������� SetActive(false)
        for (int i = 0; i < targets.Count; i++)
        {
            var t = targets[i];
            if (!t) continue;

            if (t == gameObject && !allowDeactivateSelf && !visible)
            {
                // ���ر���Ⱦ����ײ���������屾��ȷ�����ű���������
                ToggleArray(_renderersAll, false);
                ToggleArray(_cols2DAll, false);
                ToggleArray(_cols3DAll, false);
            }
            else
            {
                t.SetActive(visible);
            }
        }

        // ��� targets ��������������� visible ͳһ�����������Ⱦ/��ײ�ɼ���
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

        // �ȹر�����ȫ����ײ������ͬ֡�ظ�����
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

        // �������ⲿĿ��
        for (int i = 0; i < targets.Count; i++)
        {
            var t = targets[i];
            if (!t) continue;
            if (t == gameObject) continue; // ������������
            Destroy(t);
        }

        // �����������
        Destroy(gameObject);
    }

    // 3D ����
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

    // 2D ����
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
