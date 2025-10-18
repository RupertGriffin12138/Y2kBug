using UnityEngine;
using UnityEngine.UI;

public class SavePanelUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject saveDetailPanel;    // �浵�������
    public Button[] saveButtons;          // �˸��浵��ť
    public Button btnYes;                 // ��������е�Yes��ť
    public Button btnNo;                  // ��������е�No��ť

    void Start()
    {
        // ��ʼ�����������Ĭ������
        if (saveDetailPanel) saveDetailPanel.SetActive(false);

        // Ϊ�˸��浵��ť�󶨵���¼�
        foreach (Button btn in saveButtons)
        {
            if (btn != null)
                btn.onClick.AddListener(() => OnSaveButtonClicked(btn));
        }

        // ΪYes / No��ť�󶨹ر��¼�
        if (btnYes) btnYes.onClick.AddListener(CloseDetailPanel);
        if (btnNo) btnNo.onClick.AddListener(CloseDetailPanel);
    }

    void OnSaveButtonClicked(Button clickedButton)
    {
        // ���������������ʾ��ͬ�浵��Ϣ���߼�����ʱֻ�������飩
        if (saveDetailPanel)
            saveDetailPanel.SetActive(true);
    }

    void CloseDetailPanel()
    {
        if (saveDetailPanel)
            saveDetailPanel.SetActive(false);
    }
}
