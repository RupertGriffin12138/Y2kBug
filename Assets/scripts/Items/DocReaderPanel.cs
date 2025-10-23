using UnityEngine;
using TMPro;

public class DocReaderPanel : MonoBehaviour
{
    [Header("����")]
    public DocDB docDB;

    [Header("UI ����")]
    public GameObject rootPanel;     // ���� TextPage ��壨�� ScrollView��
    //public TMP_Text titleText;       // �������⣨��ʾ displayName��
    public TMP_Text contentText;     // ScrollView/Viewport/Content �ϵ� TMP_Text

    void Awake()
    {
        if (rootPanel) rootPanel.SetActive(false);
    }

    public void OpenById(string docId)
    {
        if (docDB == null || string.IsNullOrEmpty(docId)) return;
        var def = docDB.Get(docId);
        if (def == null) return;

        if (contentText)
            contentText.text = def.content ?? "";

        if (rootPanel)
            rootPanel.SetActive(true);

        // ��ѡ���ù������ص�����
        //var scrollRect = contentText?.GetComponentInParent<UnityEngine.UI.ScrollRect>();
        //if (scrollRect)
        //    scrollRect.normalizedPosition = new Vector2(0, 1);
    }

    public void Close()
    {
        if (rootPanel) rootPanel.SetActive(false);
    }
}
