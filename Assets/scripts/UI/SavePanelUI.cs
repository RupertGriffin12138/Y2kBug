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
        public PanelMode mode = PanelMode.Save;   // ����ģʽ����

        [Header("UI References")]
        public GameObject saveDetailPanel;        // ȷ����壨Ĭ�����أ�
        public Button[] saveButtons;              // 8 ���浵��ť
        public Text[] slotLabels;                 // ��ѡ����ʾ������+ʱ�䡱
        public Button btnYes;                     // ȷ�� Yes
        public Button btnNo;                      // ȷ�� No
        public TextMeshProUGUI confirmText;       // ȷ����ʾ����

        [Header("Config")]
        public Transform player;                  // ��ѡ�������������
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
                    ? $"���Ǹô浵��\n��λ {index + 1}\n{info}"
                    : $"���浱ǰ���ȵ���λ {index + 1}��";
                if (confirmText) confirmText.text = prompt;
            }
            else // Load ģʽ
            {
                string prompt = has
                    ? $"���ظô浵��\n��λ {index + 1}\n{info}"
                    : $"�ò�Ϊ�գ��޷����ء�";
                if (confirmText) confirmText.text = prompt;
                if (!has)
                {
                    // �ղ۲���ȷ��
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

        // ========= �����߼� =========
        void SaveToSlot(int index)
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                Debug.LogWarning("[SavePanelUI] ���˵���ֹ�浵��");
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
                InfoDialogUI.Instance.ShowMessage($"�ѱ��浽��λ {index + 1}");
            else
                Debug.Log($"[SavePanelUI] �ѱ��浽��λ {index + 1}");
        }

        // ========= ��ȡ�߼� =========
        private void LoadFromSlot(int index)
        {
            if (!HasExistingSave(index))
            {
                Debug.LogWarning("[SavePanelUI] �մ浵�޷����ء�");
                return;
            }

            SaveManager.UsePlayerPrefs(slotKeys[index]);
            GameState.LoadGameOrNew(firstSceneName);

            string scene = GameState.Current?.lastScene ?? firstSceneName;
            SaveSlotContext.CurrentKey = slotKeys[index];

            Debug.Log($"[SavePanelUI] ��������浵�� {index + 1} -> ������{scene}");
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
            if (!HasExistingSave(index)) return "�մ浵";
            string scene = PlayerPrefs.GetString(slotKeys[index] + "_metaScene", "");
            string time = PlayerPrefs.GetString(slotKeys[index] + "_metaTime", "");
            if (string.IsNullOrEmpty(scene)) scene = "(δ֪����)";
            return $"{scene}\n{time}";
        }

        bool HasExistingSave(int index)
        {
            string key = slotKeys[index];
            return PlayerPrefs.HasKey(key) && !string.IsNullOrEmpty(PlayerPrefs.GetString(key));
        }
    }
}
