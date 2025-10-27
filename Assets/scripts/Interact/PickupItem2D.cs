using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class PickupItem2D : MonoBehaviour
{
    [Header("ʰȡ����")]
    [Tooltip("ItemDB �����Ʒ id")]
    public string itemId = "sparkler";
    public int amount = 1;
    public KeyCode pickupKey = KeyCode.E;
    [Tooltip("ʰȡ�����ٸ����壻����� SetActive(false)�����ڶԻ��������ִ�У�")]
    public bool destroyOnPickup = true;

    [Header("��ʾUI����ѡ��")]
    [TextArea] public string promptString = "�� <b>E</b> ʰȡ";

    [Header("���ã��������Զ��ң�")]
    public InventoryLite inventory;   // �����ֶ���ק�����ڳ������Զ� Find
    public ItemDB itemDB;             // �����裬������ inventory.itemDB

    [Header("Save")]
    [Tooltip("�������ȶ� id ���ܿ������������")]
    public SaveTag tag;
    [Tooltip("ʰȡ���Ƿ��Զ����棨�Ƽ�����ѡ��")]
    public bool autoSaveOnPickup = true;

    // ========== ʰȡ���Զ��Ի� ==========
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;
        [TextArea(2, 3)] public string content;
    }

    [Header("ʰȡ���Զ��Ի�")]
    [Tooltip("ʰȡ��ɺ��Ƿ��Զ���������ĶԻ�")]
    public bool triggerDialogueOnPickup = true;

    [Tooltip("�Ƿ��ȵ�һ�䡺��� XXX xN����Ȼ���ٽ����������ʽ�Ի�")]
    public bool showPickupToast = true;

    public List<DialogueLine> lines = new List<DialogueLine>
    {
        new DialogueLine{ speaker="�԰�", content="�������ĳ����Ҫ�Ķ�������" },
        new DialogueLine{ speaker="������", content="��Ҳ��������ó���" },
    };

    [Header("�Ի�����")]
    public KeyCode nextKey = KeyCode.Space;
    [Tooltip("���ֻ�ÿ���ַ�����ʱ���룩��ʹ��ʵʱ��ʱ������ Time.timeScale Ӱ��")]
    public float typeCharDelay = 0.04f;

    [Tooltip("���һ�������Ƿ��Զ��رնԻ��������ٰ�һ�μ���")]
    public bool autoCloseOnLastLine = true;
    [Tooltip("���һ���Զ��ر�ǰ����ʱ���룬ʵʱ��")]
    public float autoCloseDelay = 0.3f;

    // ===== �ڲ�״̬ =====
    bool _playerInRange = false;
    bool _consumed = false;           // ��ֹ�ظ�ִ��

    // ���ֻ�״̬
    Coroutine typeRoutine;
    bool lineFullyShown;
    int idx; // ��ǰ�Ի�����
    bool _talking = false;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
        tag = GetComponent<SaveTag>(); // �����Զ�����
    }

    void Start()
    {
        // �Զ�������
        if (!inventory) inventory = FindObjectOfType<InventoryLite>();
        if (!itemDB && inventory) itemDB = inventory.itemDB;

        // ȷ�� GameState ����
        if (GameState.Current == null)
            GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

        // ����Ӧ�ã����ö����ѱ����ã�ֱ�����ز����ٹ���
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
                InfoDialogUI.Instance.Clear();
        }
    }

    void Update()
    {
        if (_consumed) return;

        if (_playerInRange && Input.GetKeyDown(pickupKey))
            TryPickup();

        // �Ի��ƽ���ֻ���ڱ��ű������ĶԻ��ڼ�Ŵ��� Space��
        if (_talking && Input.GetKeyDown(nextKey))
        {
            if (!lineFullyShown) ShowLineInstant();
            else NextLine();
        }
    }

    void TryPickup()
    {
        if (_consumed) return;

        // ����У��
        if (string.IsNullOrWhiteSpace(itemId) || amount == 0)
        {
            Debug.LogWarning($"[PickupItem2D] ��Ч�� itemId ��������{itemId}, {amount}", this);
            return;
        }
        if (!inventory)
        {
            Debug.LogWarning("[PickupItem2D] δ�ҵ� InventoryLite���޷�ʰȡ��", this);
            return;
        }

        // 1) ���±���
        int newCount = inventory.Add(itemId, amount);

        // 2) ����չʾ����
        string displayName = itemId;
        if (itemDB)
        {
            var def = itemDB.Get(itemId);
            if (def != null && !string.IsNullOrWhiteSpace(def.displayName))
                displayName = def.displayName;
        }

        // 3) ���� д�� GameState ���� 
        GameState.AddItem(itemId, amount);
        if (tag && !string.IsNullOrEmpty(tag.id))
            GameState.AddDisabledObject(tag.id);
        GameState.Current.lastScene = SceneManager.GetActiveScene().name;

        if (autoSaveOnPickup)
            GameState.SaveNow();

        // 4) ����ʰȡ����ݳ�����ʾ + �Ի���������������/���أ�
        StartCoroutine(PickupFlow(displayName));
    }

    IEnumerator PickupFlow(string displayName)
    {
        _consumed = true; // ��ֹ�ظ�����

        // ���������ʾ
        if (InfoDialogUI.Instance)
            InfoDialogUI.Instance.Clear();

        // ����ѡ���ȸ��������ʾ
        if (showPickupToast && InfoDialogUI.Instance)
        {
            InfoDialogUI.Instance.ShowMessage($"��� {displayName} x{amount}");
            yield return new WaitForSecondsRealtime(0.5f);
        }

        if (triggerDialogueOnPickup && lines != null && lines.Count > 0 && InfoDialogUI.Instance)
        {
            // ���ŶԻ�
            _talking = true;
            idx = 0;
            InfoDialogUI.Instance.StartDialogue();
            ShowCurrentLineTyped();

            // �ȴ��Ի�����
            while (_talking) yield return null;

            // ����ɲ����ٴ� EndDialogue��EndNow ���ѵ��ã�
        }

        // ���������
        if (destroyOnPickup) Destroy(gameObject);
        else gameObject.SetActive(false);
    }

    // ====== �Ի����ţ���Сʵ�֣�������� InfoDialogUI�� ======
    void ShowCurrentLineTyped()
    {
        if (!InfoDialogUI.Instance) { EndNow(); return; }

        // ֹͣ��һ��
        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }

        var line = lines[idx];

        // ��ʾ���֣��԰�����գ�
        InfoDialogUI.Instance.SetNameText(string.Equals(line.speaker, "�԰�") ? "" : line.speaker);

        // ����ı�����ʼ����
        InfoDialogUI.Instance.textBoxText.text = "";
        InfoDialogUI.Instance.HideArrow();
        lineFullyShown = false;

        typeRoutine = StartCoroutine(Typewriter(line.content));
    }

    IEnumerator Typewriter(string content)
    {
        foreach (char c in content)
        {
            InfoDialogUI.Instance.textBoxText.text += c;

            // ���� ʹ��ʵʱ��ʱ������ Time.timeScale Ӱ�� ���� 
            float t = 0f;
            while (t < typeCharDelay)
            {
                // �����û���;���
                if (Input.GetKeyDown(nextKey))
                {
                    InfoDialogUI.Instance.textBoxText.text = content;
                    t = typeCharDelay; // �������ѭ��
                    break;
                }
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        lineFullyShown = true;

        // ���� ��������һ�䲢�����Զ��ر� ���� 
        bool isLast = (idx >= lines.Count - 1);
        if (isLast && autoCloseOnLastLine)
        {
            // ������ʾ��ͷ���ԵȺ��Զ�����
            InfoDialogUI.Instance.HideArrow();
            if (autoCloseDelay > 0f)
                yield return new WaitForSecondsRealtime(autoCloseDelay);
            EndNow();
        }
        else
        {
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
        if (idx < lines.Count) ShowCurrentLineTyped();
        else EndNow();
    }

    void EndNow()
    {
        _talking = false;
        if (InfoDialogUI.Instance) InfoDialogUI.Instance.EndDialogue();
    }
}
