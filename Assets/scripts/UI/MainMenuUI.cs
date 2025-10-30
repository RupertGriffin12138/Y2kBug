using System;
using System.Collections;
using System.IO;
using Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值

// 如用 TextMeshPro：using TMPro; 并把 Text 换成 TMP_Text

namespace UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Main Menu Panels")]
        public GameObject bottomGroup;     // 主菜单按钮组
        public GameObject settingsPanel;   // 设置菜单面板
        public GameObject loadPanel;       // 槽位选择面板（8槽）
        public GameObject confirmPanel;    // 确认对话框（Yes/No）
        public Text confirmText;           // 确认提示文本

        [Header("Buttons")]
        public Button newGameButton;
        public Button continueButton;
        public Button settingsButton;
        public Button clearSaveButton;
        public Button btnYes;              // 确认对话框 Yes
        public Button btnNo;               // 确认对话框 No
        public Button[] slotButtons = new Button[8]; // 8个槽按钮
        public Text[] slotLabels = new Text[8];      // 可选：显示“场景+时间”

        [Header("Slot Visuals (optional)")]
        [Tooltip("徽标法：有存档时显示 savedBadges[i]，空槽显示 emptyBadges[i]")]
        public GameObject[] savedBadges = new GameObject[8];
        public GameObject[] emptyBadges = new GameObject[8];

        [Tooltip("换底图法：根据是否有存档切换 Image 的 Sprite")]
        public Image[] slotImages = new Image[8];
        public Sprite savedSprite;
        public Sprite emptySprite;

        [Header("Config")]
        public string firstSceneName = "C1S0";
        public KeyCode toggleKey = KeyCode.Escape;

        public enum SaveBackend { PlayerPrefs, File }
        [Tooltip("Demo 推荐 PlayerPrefs；若选 File，会在 persistentDataPath 写多个 json 文件。")]
        public SaveBackend backend = SaveBackend.PlayerPrefs;

        [Tooltip("当使用 PlayerPrefs 作为后端时基础Key前缀")]
        public string saveKeyPrefix = "SaveSlot_";   // 实际为 SaveSlot_1..SaveSlot_8

        [Tooltip("当使用文件作为后端时的文件前缀")]
        public string fileNamePrefix = "save_";      // 实际为 save_1.json..save_8.json

        // —— 内部状态 —— 
        private int pendingIndex = -1;   // 正在确认的槽位

        void Awake()
        {
            // 初始后端设定（先指向槽1，真正加载/保存时会切换到所选槽）
            if (backend == SaveBackend.PlayerPrefs)
                SaveManager.UsePlayerPrefs(GetSlotKey(0));
            else
                SaveManager.UseFile(GetSlotKey(0));
        }

        void Start()
        {
            // 初始化隐藏
            if (settingsPanel) settingsPanel.SetActive(false);
            if (loadPanel) loadPanel.SetActive(false);
            if (confirmPanel) confirmPanel.SetActive(false);

            // 绑定主按钮
            if (newGameButton) newGameButton.onClick.AddListener(OnNewGameClicked);
            if (continueButton) continueButton.onClick.AddListener(OnContinueClicked);
            if (settingsButton) settingsButton.onClick.AddListener(() => StartCoroutine(OpenSettingsDelayed()));
            if (clearSaveButton) clearSaveButton.onClick.AddListener(OnClearSaveClicked);

            // 绑定槽按钮
            for (int i = 0; i < slotButtons.Length; i++)
            {
                int idx = i;
                if (slotButtons[i])
                    slotButtons[i].onClick.AddListener(() => OnClickSlot(idx));
            }

            // 绑定确认框
            if (btnYes) btnYes.onClick.AddListener(OnConfirmYes);
            if (btnNo) btnNo.onClick.AddListener(() => { if (confirmPanel) confirmPanel.SetActive(false); });

            RefreshContinueButtonState();
            RefreshSlotsUI();
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                // 设置界面打开 → 返回
                if (settingsPanel && settingsPanel.activeSelf) { BackFromSettings(); return; }

                // 槽位面板打开 → 关闭槽位面板
                if (loadPanel && loadPanel.activeSelf)
                {
                    loadPanel.SetActive(false);
                    if (bottomGroup) bottomGroup.SetActive(true);
                    return;
                }

                // 确认面板打开 → 关闭确认
                if (confirmPanel && confirmPanel.activeSelf)
                {
                    confirmPanel.SetActive(false);
                    return;
                }
            }
        }

        // ========= 主按钮：新游戏 =========
        private void OnNewGameClicked()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            
            PlayerPrefs.SetInt("BoardKey_Prefab", 1);
            int slot = FindFirstEmptySlotIndex();
            if (slot == -1)
            {
                slot = 0; // 也可以弹出选择覆盖哪个槽
                Debug.LogWarning("[MainMenuUI] 所有槽已满，默认覆盖槽1。");
            }

            UseSlot(slot);
            GameState.NewGame(firstSceneName);
            GameState.SaveNow();
            SaveMeta(slot, firstSceneName, DateTime.Now);

            SaveSlotContext.CurrentKey = GetSlotKey(slot);
            StartCoroutine(LoadSceneAfterDelay(firstSceneName, 0.1f));
        }

        // ========= 主按钮：继续游戏（打开槽位面板/读取） =========
        void OnContinueClicked()
        {
            if (bottomGroup) bottomGroup.SetActive(false);
            if (loadPanel) loadPanel.SetActive(true);
            RefreshSlotsUI();
        }

        // ========= 清档：示例全清 =========
        void OnClearSaveClicked()
        {
            for (int i = 0; i < 8; i++) WipeSlot(i);
            RefreshContinueButtonState();
            RefreshSlotsUI();
            Debug.Log("[MainMenuUI] 已清除所有存档。");
        }

        // ========= 槽位按钮点击 =========
        void OnClickSlot(int index)
        {
            if (!HasExistingSave(index)) return; // 空槽被禁用了，这里通常进不来；再防御一次
            pendingIndex = index;

            string info = GetSlotInfo(index);
            if (confirmPanel) confirmPanel.SetActive(true);
            if (confirmText) confirmText.text = $"读取槽位 {index + 1}？\n{info}";
        }

        // ========= 确认：读取并进入存档 =========
        void OnConfirmYes()
        {
            if (pendingIndex < 0) { if (confirmPanel) confirmPanel.SetActive(false); return; }

            UseSlot(pendingIndex);

            var data = SaveManager.LoadOrDefault(firstSceneName);
            GameState.LoadGameOrNew(firstSceneName);
            GameState.ReplaceWith(data);

            SaveSlotContext.CurrentKey = GetSlotKey(pendingIndex);

            string scene = string.IsNullOrEmpty(GameState.Current.lastScene) ? firstSceneName : GameState.Current.lastScene;
            StartCoroutine(LoadSceneAfterDelay(scene, 0.1f));

            if (confirmPanel) confirmPanel.SetActive(false);
        }

        // ========= 设置面板 =========
        IEnumerator OpenSettingsDelayed()
        {
            yield return new WaitForSeconds(0.2f);
            OpenSettings();
        }

        void OpenSettings()
        {
            if (bottomGroup) bottomGroup.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(true);
        }

        void BackFromSettings()
        {
            if (settingsPanel) settingsPanel.SetActive(false);
            if (bottomGroup) bottomGroup.SetActive(true);
        }

        // ========= UI 刷新 =========
        void RefreshContinueButtonState()
        {
            bool hasAny = false;
            for (int i = 0; i < 8; i++) { if (HasExistingSave(i)) { hasAny = true; break; } }
            // 如果希望无存档时禁用“继续”，把下一行改为：continueButton.interactable = hasAny;
            if (continueButton) continueButton.interactable = true;
        }

        void RefreshSlotsUI()
        {
            for (int i = 0; i < 8; i++)
            {
                bool has = HasExistingSave(i);

                // 1) 交互：有存档才能点
                if (slotButtons != null && i < slotButtons.Length && slotButtons[i])
                    slotButtons[i].interactable = has;

                // 2) 文本：显示“场景+时间”或“空存档”
                if (slotLabels != null && i < slotLabels.Length && slotLabels[i])
                    slotLabels[i].text = has ? GetSlotInfo(i) : "空存档";

                // 3) 徽标法：勾/叉显隐
                if (savedBadges != null && i < savedBadges.Length && savedBadges[i])
                    savedBadges[i].SetActive(has);
                if (emptyBadges != null && i < emptyBadges.Length && emptyBadges[i])
                    emptyBadges[i].SetActive(!has);

                // 4) 换底图法：切换按钮 Image 的 Sprite
                if (slotImages != null && i < slotImages.Length && slotImages[i])
                {
                    if (has && savedSprite) slotImages[i].sprite = savedSprite;
                    else if (!has && emptySprite) slotImages[i].sprite = emptySprite;
                }
            }
        }

        // ========= 槽位工具 =========
        string GetSlotKey(int index)
        {
            if (backend == SaveBackend.PlayerPrefs)
                return $"{saveKeyPrefix}{index + 1}";
            else
                return $"{fileNamePrefix}{index + 1}.json";
        }

        void UseSlot(int index)
        {
            if (backend == SaveBackend.PlayerPrefs)
                SaveManager.UsePlayerPrefs(GetSlotKey(index));
            else
                SaveManager.UseFile(GetSlotKey(index));
        }

        bool HasExistingSave(int index)
        {
            if (backend == SaveBackend.PlayerPrefs)
            {
                string key = GetSlotKey(index);
                return PlayerPrefs.HasKey(key) && !string.IsNullOrEmpty(PlayerPrefs.GetString(key));
            }
            else
            {
                string path = Path.Combine(Application.persistentDataPath, GetSlotKey(index));
                return File.Exists(path) && new FileInfo(path).Length > 2;
            }
        }

        string GetSlotInfo(int index)
        {
            // 读取元信息（场景+时间）
            string scene, time;
            GetSlotMeta(index, out scene, out time);
            if (string.IsNullOrEmpty(scene)) scene = "(未知场景)";
            if (string.IsNullOrEmpty(time)) time = "";
            return $"{scene}\n{time}";
        }

        int FindFirstEmptySlotIndex()
        {
            for (int i = 0; i < 8; i++) if (!HasExistingSave(i)) return i;
            return -1;
        }

        void SaveMeta(int index, string scene, DateTime dt)
        {
            if (backend == SaveBackend.PlayerPrefs)
            {
                string k = GetSlotKey(index);
                PlayerPrefs.SetString(k + "_metaScene", scene ?? "");
                PlayerPrefs.SetString(k + "_metaTime", dt.ToString("yyyy-MM-dd HH:mm"));
                PlayerPrefs.Save();
            }
            else
            {
                string k = "FileSlot_" + (index + 1);
                PlayerPrefs.SetString(k + "_metaScene", scene ?? "");
                PlayerPrefs.SetString(k + "_metaTime", dt.ToString("yyyy-MM-dd HH:mm"));
                PlayerPrefs.Save();
            }
        }

        void GetSlotMeta(int index, out string scene, out string time)
        {
            if (backend == SaveBackend.PlayerPrefs)
            {
                string k = GetSlotKey(index);
                scene = PlayerPrefs.GetString(k + "_metaScene", "");
                time = PlayerPrefs.GetString(k + "_metaTime", "");
            }
            else
            {
                string k = "FileSlot_" + (index + 1);
                scene = PlayerPrefs.GetString(k + "_metaScene", "");
                time = PlayerPrefs.GetString(k + "_metaTime", "");
            }
        }

        void WipeSlot(int index)
        {
            if (backend == SaveBackend.PlayerPrefs)
            {
                string k = GetSlotKey(index);
                PlayerPrefs.DeleteKey(k);
                PlayerPrefs.DeleteKey(k + "_metaScene");
                PlayerPrefs.DeleteKey(k + "_metaTime");
            }
            else
            {
                string path = Path.Combine(Application.persistentDataPath, GetSlotKey(index));
                if (File.Exists(path)) File.Delete(path);

                string k = "FileSlot_" + (index + 1);
                PlayerPrefs.DeleteKey(k + "_metaScene");
                PlayerPrefs.DeleteKey(k + "_metaTime");
            }
        }

        IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(sceneName);
        }

        void ShowTransientMessage(string msg)
        {
            Debug.Log(msg);
            // if (InfoDialogUI.Instance) InfoDialogUI.Instance.ShowMessage(msg);
        }
    }
}
