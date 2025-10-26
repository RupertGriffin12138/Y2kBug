using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2D �Ի���������������ʾE��E��ʼ�Ի����ո���һ�䣻����д��SaveManager������������
/// ������InfoDialogUI, SaveManager, SaveData(dialogueSeenIds/HasSeenDialogue/TryMarkDialogueSeen)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger2D_Save : MonoBehaviour
{
    [Header("ΨһID�����ڴ浵���أ�")]
    public string dialogueId = "dlg_001";

    [Header("���ֿ򣨿ɿգ�")]
    public string speakerName = "NPC";

    [Header("�Ի����ݣ���䣩")]
    [TextArea(2, 3)]
    public List<string> lines = new List<string>
    {
        "��ã������ߡ�",
        "���ո������һ�䡣",
        "ף����;˳����"
    };

    [Header("��ʾ�밴��")]
    [TextArea] public string hintPressE = "�� <b>E</b> �Ի�";
    public KeyCode interactKey = KeyCode.E;
    public KeyCode nextKey = KeyCode.Space;

    [Header("����")]
    public string playerTag = "Player";

    [Header("��Ϊ")]
    public bool blockWhenPaused = true;
    public bool destroyAfterFinish = true;

    private bool inside;
    private bool talking;
    private int idx;

    // ���ػ���һ��SaveData������ÿ�ζ������л�
    private SaveData save;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Start()
    {
        // ����浵�����������Ĭ�ϣ��ɰ���� firstSceneName��
        save = SaveManager.LoadOrDefault("Town");

        // ����ɹ��öԻ����ٳ���
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
            InfoDialogUI.Instance.StartDialogue(); // ���idle/��ͷ
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

        // ��ǽ��Ȳ�����
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
            inside = false; // ���ף������ظ���ʾ
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
