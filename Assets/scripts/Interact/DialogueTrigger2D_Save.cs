using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 2D �Ի����������Զ����� + ��˵���� + �����л� + ���ֻ���
/// ���봥��������ʼ���ո��ƽ���ÿ����� speaker �� content������д��浵����ѡ��������
/// ������InfoDialogUI(Instance/StartDialogue/EndDialogue/SetNameText/ShowMessage/textBoxText/EnableCharacterBackground/ShowArrow/HideArrow)��GameState��SaveData
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger2D_Save : MonoBehaviour
{
    [Header("ΨһID�����ڴ浵���أ�")]
    public string dialogueId = "dlg_001";

    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;
        [TextArea(2, 3)] public string content;
    }

    [Header("�Ի����ݣ�ÿ���������+���ݣ�")]
    public List<DialogueLine> lines = new List<DialogueLine>
    {
        new DialogueLine{ speaker = "�԰�", content = "�ٳ����ϵ��к������ⲽ����" },
        new DialogueLine{ speaker = "����", content = "ʮ��" },
        new DialogueLine{ speaker = "ף��", content = "�𼱡�" }
    };

    [Header("����")]
    public KeyCode nextKey = KeyCode.Space;

    [Header("����")]
    public string playerTag = "Player";

    [Header("��Ϊ")]
    public bool blockWhenPaused = true;
    public bool destroyAfterFinish = true;

    [Header("���ֻ�����")]
    [Tooltip("ÿ���ַ�����ʱ���룩")]
    public float typeCharDelay = 0.05f;

    // ����ʱ״̬
    private bool talking;
    private int idx;
    private Coroutine typeRoutine;
    private bool lineFullyShown;
    private bool begun;                 // �Ѿ�����������ֹ��� BeginTalk��
    private Collider2D triggerCol;      // ����̸���ڼ���ã������ظ�����

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
        // ȷ���浵�Ѽ��ص� GameState��ȫ��Ψһ���ࣩ
        if (GameState.Current == null)
            GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

        // �ѿ�����ֱ���Ƴ�
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

        // �ٴ����أ����⼫����������˳�����⣩
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

        // ̸���ڼ���ô������������ظ�����
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

        // ͣ����һ�εĴ���Э��
        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }

        var line = lines[idx];

        // ���ֿ��԰ײ���ʾ����
        InfoDialogUI.Instance.SetNameText(string.Equals(line.speaker, "�԰�") ? "" : line.speaker);

        // �����������л����������ʵ���ˣ�
        InfoDialogUI.Instance.EnableCharacterBackground(line.speaker);

        // ��ղ���ʼ����
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

            // ���ڴ����а��������� Update ���߼�������ȫ
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

        // ���� �� GameState ��ΪΨһ����д�� & ���� ���� //
        if (GameState.Current == null)
            GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

        if (GameState.Current != null && GameState.Current.TryMarkDialogueSeen(dialogueId))
        {
            GameState.SaveNow(); // ��ȫ��ϵͳһ�µı��淽ʽ�����ⱻ����
        }

        if (InfoDialogUI.Instance)
            InfoDialogUI.Instance.EndDialogue();

        if (destroyAfterFinish)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    // ѡ��ʱ���ӻ�������Χ
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
