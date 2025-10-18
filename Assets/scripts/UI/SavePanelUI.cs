using UnityEngine;
using UnityEngine.UI;

public class SavePanelUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject saveDetailPanel;    // 存档详情面板
    public Button[] saveButtons;          // 八个存档按钮
    public Button btnYes;                 // 详情面板中的Yes按钮
    public Button btnNo;                  // 详情面板中的No按钮

    void Start()
    {
        // 初始化：详情面板默认隐藏
        if (saveDetailPanel) saveDetailPanel.SetActive(false);

        // 为八个存档按钮绑定点击事件
        foreach (Button btn in saveButtons)
        {
            if (btn != null)
                btn.onClick.AddListener(() => OnSaveButtonClicked(btn));
        }

        // 为Yes / No按钮绑定关闭事件
        if (btnYes) btnYes.onClick.AddListener(CloseDetailPanel);
        if (btnNo) btnNo.onClick.AddListener(CloseDetailPanel);
    }

    void OnSaveButtonClicked(Button clickedButton)
    {
        // 可以在这里添加显示不同存档信息的逻辑（暂时只弹出详情）
        if (saveDetailPanel)
            saveDetailPanel.SetActive(true);
    }

    void CloseDetailPanel()
    {
        if (saveDetailPanel)
            saveDetailPanel.SetActive(false);
    }
}
