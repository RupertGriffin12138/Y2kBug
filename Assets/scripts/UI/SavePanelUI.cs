using System;
using System.Collections;
using Save;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class SavePanelUI : MonoBehaviour
    {
        public enum PanelMode { Save, Load }
        [Header("Mode")]
        public PanelMode mode = PanelMode.Save;   // 新增模式开关

        [Header("UI References")]
        public GameObject saveDetailPanel;        // 确认面板（默认隐藏）
        public Button[] saveButtons;              // 8 个存档按钮
        public Text[] slotLabels;                 // 可选：显示“场景+时间”
        public Button btnYes;                     // 确认 Yes
        public Button btnNo;                      // 确认 No
        public TextMeshProUGUI confirmText;       // 确认提示文字

        [Header("Config")]
        public Transform player;                  // 可选：保存玩家坐标
        public bool savePlayerPos = true;
        public string firstSceneName = "C1S0";

        [Header("Visual: Saved State")]
        public GameObject[] savedBadges;
        public Image[] slotButtonImages;
        public Sprite savedSprite;
        public Sprite emptySprite;
        public Color savedNormalColor = new Color(0.90f, 0.96f, 1f, 1f);
        public Color emptyNormalColor = Color.white;

        private readonly string[] slotKeys = {
            "SaveSlot_1","SaveSlot_2","SaveSlot_3","SaveSlot_4",
            "SaveSlot_5","SaveSlot_6","SaveSlot_7","SaveSlot_8"
        };

        private int selectedIndex = -1;

        void Start()
        {
            if (saveDetailPanel) saveDetailPanel.SetActive(false);

            if (saveButtons != null)
            {
                for (int i = 0; i < saveButtons.Length; i++)
                {
                    int idx = i;
                    if (saveButtons[i] != null)
                        saveButtons[i].onClick.AddListener(() => OnSlotClicked(idx));
                }
            }

            if (btnYes) btnYes.onClick.AddListener(OnConfirmYes);
            if (btnNo) btnNo.onClick.AddListener(CloseDetailPanel);

            RefreshSlotsUI();
        }

        void OnSlotClicked(int index)
        {
            selectedIndex = index;
            if (saveDetailPanel) saveDetailPanel.SetActive(true);

            bool has = HasExistingSave(index);
            string info = GetSlotInfo(index);

            if (mode == PanelMode.Save)
            {
                string prompt = has
                    ? $"覆盖该存档？\n槽位 {index + 1}\n{info}"
                    : $"保存当前进度到槽位 {index + 1}？";
                if (confirmText) confirmText.text = prompt;
            }
            else // Load 模式
            {
                string prompt = has
                    ? $"加载该存档？\n槽位 {index + 1}\n{info}"
                    : $"该槽为空，无法加载。";
                if (confirmText) confirmText.text = prompt;
                if (!has)
                {
                    // 空槽不弹确认
                    selectedIndex = -1;
                }
            }
        }

        void OnConfirmYes()
        {
            if (selectedIndex < 0)
            {
                CloseDetailPanel();
                return;
            }

            if (mode == PanelMode.Save)
                SaveToSlot(selectedIndex);
            else
                LoadFromSlot(selectedIndex);

            CloseDetailPanel();
        }

        // ========= 保存逻辑 =========
        void SaveToSlot(int index)
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                Debug.LogWarning("[SavePanelUI] 主菜单禁止存档。");
                return;
            }

            if (GameState.Current == null)
                GameState.LoadGameOrNew(firstSceneName);

            GameState.Current.lastScene = SceneManager.GetActiveScene().name;
            if (savePlayerPos && player)
            {
                GameState.Current.playerX = player.position.x;
                GameState.Current.playerY = player.position.y;
            }

            SaveManager.UsePlayerPrefs(slotKeys[index]);
            GameState.SaveNow();

            string scene = GameState.Current.lastScene ?? "";
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            PlayerPrefs.SetString(slotKeys[index] + "_metaScene", scene);
            PlayerPrefs.SetString(slotKeys[index] + "_metaTime", time);
            PlayerPrefs.Save();

            RefreshSlotsUI();

            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.ShowMessage($"已保存到槽位 {index + 1}");
            else
                Debug.Log($"[SavePanelUI] 已保存到槽位 {index + 1}");
        }

        // ========= 读取逻辑 =========
        private void LoadFromSlot(int index)
        {
            if (!HasExistingSave(index))
            {
                Debug.LogWarning("[SavePanelUI] 空存档无法加载。");
                return;
            }

            SaveManager.UsePlayerPrefs(slotKeys[index]);
            GameState.LoadGameOrNew(firstSceneName);

            string scene = GameState.Current?.lastScene ?? firstSceneName;
            SaveSlotContext.CurrentKey = slotKeys[index];

            Debug.Log($"[SavePanelUI] 正在载入存档槽 {index + 1} -> 场景：{scene}");
            StartCoroutine(LoadSceneAfterDelay(scene, 0.2f));
        }

        private IEnumerator LoadSceneAfterDelay(string scene, float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(scene);
        }

        void CloseDetailPanel()
        {
            if (saveDetailPanel) saveDetailPanel.SetActive(false);
            selectedIndex = -1;
        }

        void RefreshSlotsUI()
        {
            for (int i = 0; i < slotKeys.Length; i++)
            {
                bool has = HasExistingSave(i);

                if (slotLabels != null && i < slotLabels.Length && slotLabels[i])
                    slotLabels[i].text = GetSlotInfo(i);

                ApplySlotVisual(i, has);
            }
        }

        void ApplySlotVisual(int index, bool hasSave)
        {
            if (savedBadges != null && index < savedBadges.Length && savedBadges[index])
                savedBadges[index].SetActive(hasSave);

            if (slotButtonImages != null && index < slotButtonImages.Length && slotButtonImages[index])
            {
                var img = slotButtonImages[index];
                if (hasSave && savedSprite) img.sprite = savedSprite;
                else if (!hasSave && emptySprite) img.sprite = emptySprite;
            }

            if (saveButtons != null && index < saveButtons.Length && saveButtons[index])
            {
                var btn = saveButtons[index];
                var colors = btn.colors;
                colors.normalColor = hasSave ? savedNormalColor : emptyNormalColor;
                btn.colors = colors;
            }
        }

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
}
