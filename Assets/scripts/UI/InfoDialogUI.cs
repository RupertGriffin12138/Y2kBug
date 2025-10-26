using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InfoDialogUI : MonoBehaviour
{
    public static InfoDialogUI Instance;  // �����λֱ�ӵ��ã�Ҳ���� Inspector ���ã�

    [Header("�Ի����ı���TMP��")]
    public TMP_Text textBoxText; // TextBoxText UI Text component

    [Header("���ֿ��ı���TMP��")]
    public TMP_Text nameBoxText; // NameBoxText UI Text component

    [Header("��ͷͼ��")]
    public Image arrowImage; // Arrow image to indicate pressing E key

    [Header("����ͣʱ��Ĭ����ʾ")]
    [TextArea]
    public string idleHint = "������Ƶ���Ʒ�ϲ鿴��Ϣ";

    private bool isShowingDialogue = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    void Start()
    {
        Clear(); // ��ʼ��ʾĬ����ʾ
    }

    /// <summary>��ʾ��Ʒ���� + �ڶ�����ʾ��</summary>
    public void ShowItem(string displayName, bool showUseTip = true)
    {
        if (!textBoxText || !nameBoxText) return;
        if (showUseTip)
            textBoxText.text = $"{displayName}\n<size=90%>���������/ʹ�ã�</size>";
        else
            textBoxText.text = displayName;
    }

    /// <summary>��ʾ�����ı���������ϵͳ��Ϣ����</summary>
    public void ShowMessage(string message)
    {
        if (!textBoxText) return;
        textBoxText.text = message;
    }

    /// <summary>�ָ�Ĭ����ʾ��</summary>
    public void Clear()
    {
        if (isShowingDialogue) return;
        textBoxText.text = idleHint;
        nameBoxText.text = "";
        HideArrow();
        isShowingDialogue = false;
    }

    /// <summary>���������ı���</summary>
    public void SetNameText(string name)
    {
        if (!nameBoxText) return;
        nameBoxText.text = name;
    }

    /// <summary>��ʼ��ʾ�Ի���</summary>
    public void StartDialogue()
    {
        isShowingDialogue = true;
        textBoxText.text = ""; // ���Ĭ����ʾ
        nameBoxText.text = ""; // ȷ�����ֿ�Ϊ��
        HideArrow();
    }

    /// <summary>������ʾ�Ի���</summary>
    public void EndDialogue()
    {
        isShowingDialogue = false;
        Clear();
    }

    /// <summary>��ʾ��ͷ��</summary>
    public void ShowArrow()
    {
        if (arrowImage != null)
        {
            arrowImage.enabled = true;
        }
    }

    /// <summary>���ؼ�ͷ��</summary>
    public void HideArrow()
    {
        if (arrowImage != null)
        {
            arrowImage.enabled = false;
        }
    }
}


