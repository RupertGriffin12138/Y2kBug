using UnityEngine;
using TMPro;

public class InfoDialogUI : MonoBehaviour
{
    public static InfoDialogUI Instance;  // �����λֱ�ӵ��ã�Ҳ���� Inspector ���ã�

    [Header("�Ի����ı���TMP��")]
    public TMP_Text text;

    [Header("����ͣʱ��Ĭ����ʾ")]
    [TextArea]
    public string idleHint = "������Ƶ���Ʒ�ϲ鿴��Ϣ";

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
        if (!text) return;
        if (showUseTip)
            text.text = $"{displayName}\n<size=90%>���������/ʹ�ã�</size>";
        else
            text.text = displayName;
    }

    /// <summary>��ʾ�����ı���������ϵͳ��Ϣ����</summary>
    public void ShowMessage(string message)
    {
        if (!text) return;
        text.text = message;
    }

    /// <summary>�ָ�Ĭ����ʾ��</summary>
    public void Clear()
    {
        if (!text) return;
        text.text = idleHint;
    }
}
