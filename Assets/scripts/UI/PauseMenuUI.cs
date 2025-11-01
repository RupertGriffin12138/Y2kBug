using System;
using System.Collections;
using Characters.PLayer_25D;
using Characters.Player;
using Save;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("UI")]
        public GameObject pausePanel;
        public GameObject settingsPanel;
        public GameObject textPage;

        public Button btnBackToGame;
        public Button btnBackToMenu;
        public Button btnSettings;
        public Button btnSave;

        [Header("Config")]
        public string menuSceneName = "TestMenu";
        public KeyCode toggleKey = KeyCode.Escape;
        public bool pauseAudio = true;

        [Header("Save Config")]
        public Transform playerPos;          // 可选：保存玩家坐标
        public bool savePlayerPos = true; // 勾选则保存玩家2D坐标
        [Tooltip("如果没有从主菜单设置 SaveSlotContext.CurrentKey，则使用此默认槽键。例：SaveSlot_1")]
        public string defaultSaveKey = "SaveSlot_1";

        private bool isPaused;
        private Player player;
        private PlayerMovement playerMovement;

        public bool isBoard=false;

        public void Start()
        {
            // 初始隐藏
            if (pausePanel) pausePanel.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(false);

            if (!player)
            {
                player = FindObjectOfType<Player>();
            }

            if (!playerMovement)
            {
                playerMovement = FindObjectOfType<PlayerMovement>();
            }

            if (player)
            {
                playerPos = player.transform;
            }

            if (playerMovement)
            {
                playerPos = playerMovement.transform;
            }

            // 绑定按钮
            if (btnBackToGame) btnBackToGame.onClick.AddListener(ResumeGame);
            if (btnBackToMenu) btnBackToMenu.onClick.AddListener(BackToMenu);
            if (btnSettings) btnSettings.onClick.AddListener(OpenSettings);
            if (btnSave) btnSave.onClick.AddListener(SaveNowToCurrentSlot);
        }

        public void Update()
        {

            if (SceneManager.GetActiveScene().name == "Riddle abacus")
            {
                return;
            }
            if (!isBoard)
            {
                if (Input.GetKeyDown(toggleKey))
                {
                    // --- 优先判断 TextPage 是否打开 ---
                    if (textPage && textPage.activeSelf)
                    {
                        textPage.SetActive(false);
                        return; // 不再继续执行暂停逻辑
                    }

                    // 若当前处于设置界面，Esc 返回暂停菜单
                    if (settingsPanel && settingsPanel.activeSelf)
                    {
                        BackFromSettings();
                        return;
                    }

                    // 其他情况，控制暂停与恢复
                    if (isPaused) ResumeGame();
                    else PauseGame();
                }
            }
        }

        public void PauseGame()
        {
            isPaused = true;
            if (pausePanel) pausePanel.SetActive(true);
            if (settingsPanel) settingsPanel.SetActive(false);

            // 停止游戏时间 & 可选暂停全局音频
            Time.timeScale = 0f;
            if (pauseAudio) AudioListener.pause = true;

            // 显示鼠标（若你有隐藏/锁定鼠标）
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // 为键盘/手柄高亮默认按钮
            if (btnBackToGame)
                EventSystem.current?.SetSelectedGameObject(null);
        }

        public void ResumeGame()
        {
            // 恢复时间/音频在隐藏 UI 前
            Time.timeScale = 1f;
            if (pauseAudio) AudioListener.pause = false;

            isPaused = false;
            if (pausePanel) pausePanel.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(false);

            // 清理选中
            EventSystem.current?.SetSelectedGameObject(null);
        }

        private void BackToMenu()
        {
            // 1) 若当前正暂停，则先恢复时间流动
            Time.timeScale = 1f;
            if (pauseAudio) AudioListener.pause = false;
            isPaused = false;

            // 2) 隐藏所有暂停界面
            if (pausePanel) pausePanel.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(false);

            // 3) 自动保存一次当前进度（避免退出前丢进度）
            if (GameState.Current == null)
                GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

            GameState.Current.lastScene = SceneManager.GetActiveScene().name;
            if (savePlayerPos && playerPos != null)
            {
                GameState.Current.playerX = playerPos.position.x;
                GameState.Current.playerY = playerPos.position.y;
            }

            string slotKey = !string.IsNullOrEmpty(SaveSlotContext.CurrentKey)
                ? SaveSlotContext.CurrentKey
                : defaultSaveKey;

            SaveManager.UsePlayerPrefs(slotKey);
            GameState.SaveNow();

            PlayerPrefs.SetString(slotKey + "_metaScene", GameState.Current.lastScene ?? "");
            PlayerPrefs.SetString(slotKey + "_metaTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            PlayerPrefs.Save();

            // 4) 清理鼠标状态
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // 直接切换
            SceneManager.LoadScene(menuSceneName);
                
        }

        public void OpenSettings()
        {
            if (pausePanel) pausePanel.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(true);
        }

        // 从设置面板返回暂停菜单
        public void BackFromSettings()
        {
            if (settingsPanel) settingsPanel.SetActive(false);
            if (pausePanel) pausePanel.SetActive(true);
        }

        public IEnumerator LoadMenuAfterFrame()
        {
            // 等一帧，避免点击抬起事件在切场景时丢失
            yield return null;
            SceneManager.LoadScene(menuSceneName);
        }

        // 若带着暂停状态离开当前场景（例如切换关卡），确保不残留暂停
        private void OnDisable()
        {
            if (isPaused)
            {
                Time.timeScale = 1f;
                if (pauseAudio) AudioListener.pause = false;
                isPaused = false;
            }
        }
        // =========================
        //   保存到“当前存档槽”
        // =========================
        private void SaveNowToCurrentSlot()
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                Debug.LogWarning("[SavePanelUI] 主菜单中禁止存档。");
                return;
            }
            // 1) 确保 GameState 存在
            if (GameState.Current == null)
                GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

            // 2) 写入当前场景名与（可选）玩家坐标
            GameState.Current.lastScene = SceneManager.GetActiveScene().name;
            if (savePlayerPos && playerPos != null)
            {
                GameState.Current.playerX = playerPos.position.x;
                GameState.Current.playerY = playerPos.position.y;
            }

            // 3) 选择当前存档槽 Key
            string slotKey = !string.IsNullOrEmpty(SaveSlotContext.CurrentKey)
                ? SaveSlotContext.CurrentKey
                : defaultSaveKey;

            // 4) 切换存储后端到该槽并保存
            SaveManager.UsePlayerPrefs(slotKey);
            GameState.SaveNow();

            // 5) 写“场景+时间”元信息（主菜单显示会用到）
            PlayerPrefs.SetString(slotKey + "_metaScene", GameState.Current.lastScene ?? "");
            PlayerPrefs.SetString(slotKey + "_metaTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            PlayerPrefs.Save();
            
        }
    }
}

