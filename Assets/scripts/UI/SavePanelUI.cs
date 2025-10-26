using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
// 如用TMP，把上面Text换成TMP_Text并using TMPro;

public class SavePanelUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject saveDetailPanel;          // 确认面板（默认隐藏）
    public Button[] saveButtons;                // 8 个存档按钮
    public Text[] slotLabels;                   // 可选：显示“场景+时间”
    public Button btnYes;                       // 确认 Yes
    public Button btnNo;                        // 确认 No
    public Text confirmText;                    // 可选：确认提示文字

    [Header("Config")]
    public Transform player;                    // 可选：保存玩家坐标
    public bool savePlayerPos = true;
    public string firstSceneName = "C1S0";

    // —— 按钮“已存档”可视化（任选其一或组合） ——
    [Header("Visual: Saved State")]
    [Tooltip("徽标法：每个槽位对应一个‘已存档’小图/红点/勾，hasSave时 SetActive(true)")]
    public GameObject[] savedBadges;            // size=8，可为空

    [Tooltip("换底图法：按钮Image的Sprite根据是否有存档切换")]
    public Image[] slotButtonImages;            // size=8，可为空
    public Sprite savedSprite;                  // 有存档时用
    public Sprite emptySprite;                  // 空存档时用

    [Tooltip("配色法：通过ColorBlock切换normalColor；若不想改配色可不填")]
    public Color savedNormalColor = new Color(0.90f, 0.96f, 1f, 1f); // 浅蓝
    public Color emptyNormalColor = Color.white;

    // 8 个固定槽位的 PlayerPrefs Key
    private readonly string[] slotKeys = new string[8]
    {
        "SaveSlot_1","SaveSlot_2","SaveSlot_3","SaveSlot_4",
        "SaveSlot_5","SaveSlot_6","SaveSlot_7","SaveSlot_8"
    };

    private int selectedIndex = -1;

    void Start()
    {
        if (saveDetailPanel) saveDetailPanel.SetActive(false);

        // 绑定 8 个按钮
        if (saveButtons != null)
        {
            for (int i = 0; i < saveButtons.Length; i++)
            {
                int idx = i; // 闭包
                if (saveButtons[i] != null)
                    saveButtons[i].onClick.AddListener(() => OnSaveButtonClicked(idx));
            }
        }

        if (btnYes) btnYes.onClick.AddListener(OnConfirmYes);
        if (btnNo) btnNo.onClick.AddListener(CloseDetailPanel);

        RefreshSlotsUI();
    }

    void OnSaveButtonClicked(int index)
    {
        selectedIndex = index;

        if (saveDetailPanel) saveDetailPanel.SetActive(true);

        bool has = HasExistingSave(index);
        string info = GetSlotInfo(index);
        string prompt = has
            ? $"覆盖该存档？\n槽位 {index + 1}\n{info}"
            : $"保存当前进度到槽位 {index + 1}？";
        if (confirmText) confirmText.text = prompt;
    }

    void OnConfirmYes()
    {
        if (selectedIndex < 0) { CloseDetailPanel(); return; }

        if (GameState.Current == null)
            GameState.LoadGameOrNew(firstSceneName);

        GameState.Current.lastScene = SceneManager.GetActiveScene().name;
        if (savePlayerPos && player)
        {
            GameState.Current.playerX = player.position.x;
            GameState.Current.playerY = player.position.y;
        }

        // 切换到对应槽位并保存
        SaveManager.UsePlayerPrefs(slotKeys[selectedIndex]);
        GameState.SaveNow();

        // 存展示用元信息（场景+时间）
        string scene = GameState.Current.lastScene ?? "";
        string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        PlayerPrefs.SetString(slotKeys[selectedIndex] + "_metaScene", scene);
        PlayerPrefs.SetString(slotKeys[selectedIndex] + "_metaTime", time);
        PlayerPrefs.Save();

        RefreshSlotsUI();
        CloseDetailPanel();

        if (InfoDialogUI.Instance)
            InfoDialogUI.Instance.ShowMessage($"已保存到槽位 {selectedIndex + 1}");
    }

    void CloseDetailPanel()
    {
        if (saveDetailPanel) saveDetailPanel.SetActive(false);
        selectedIndex = -1;
    }

    // —— 刷新所有槽位的文本 + 外观状态 ——
    void RefreshSlotsUI()
    {
        for (int i = 0; i < slotKeys.Length; i++)
        {
            bool has = HasExistingSave(i);

            // 文本
            if (slotLabels != null && i < slotLabels.Length && slotLabels[i])
                slotLabels[i].text = GetSlotInfo(i);

            // 外观
            ApplySlotVisual(i, has);
        }
    }

    // —— 应用按钮外观（徽标/换底图/配色，按你绑定的字段自动生效） ——
    void ApplySlotVisual(int index, bool hasSave)
    {
        // 1) 徽标法
        if (savedBadges != null && index < savedBadges.Length && savedBadges[index])
            savedBadges[index].SetActive(hasSave);

        // 2) 换底图法
        if (slotButtonImages != null && index < slotButtonImages.Length && slotButtonImages[index])
        {
            var img = slotButtonImages[index];
            if (hasSave && savedSprite) img.sprite = savedSprite;
            else if (!hasSave && emptySprite) img.sprite = emptySprite;
            // 如果没提供 Sprite，就不改
        }

        // 3) 配色法（只改normalColor，其他保留按钮原样）
        if (saveButtons != null && index < saveButtons.Length && saveButtons[index])
        {
            var btn = saveButtons[index];
            var colors = btn.colors;
            colors.normalColor = hasSave ? savedNormalColor : emptyNormalColor;
            btn.colors = colors;
        }
    }

    // —— 显示文本（空存档 / 场景+时间） ——
    string GetSlotInfo(int index)
    {
        if (!HasExistingSave(index)) return "空存档";
        string scene = PlayerPrefs.GetString(slotKeys[index] + "_metaScene", "");
        string time = PlayerPrefs.GetString(slotKeys[index] + "_metaTime", "");
        if (string.IsNullOrEmpty(scene)) scene = "(未知场景)";
        return $"{scene}\n{time}";
    }

    bool HasExistingSave(int index)
    {
        string key = slotKeys[index];
        return PlayerPrefs.HasKey(key) && !string.IsNullOrEmpty(PlayerPrefs.GetString(key));
    }
}
