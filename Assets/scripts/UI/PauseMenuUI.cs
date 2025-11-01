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
        public Transform playerPos;          // ��ѡ�������������
        public bool savePlayerPos = true; // ��ѡ�򱣴����2D����
        [Tooltip("���û�д����˵����� SaveSlotContext.CurrentKey����ʹ�ô�Ĭ�ϲۼ�������SaveSlot_1")]
        public string defaultSaveKey = "SaveSlot_1";

        private bool isPaused;
        private Player player;
        private PlayerMovement playerMovement;

        public bool isBoard=false;

        public void Start()
        {
            // ��ʼ����
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

            // �󶨰�ť
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
                    // --- �����ж� TextPage �Ƿ�� ---
                    if (textPage && textPage.activeSelf)
                    {
                        textPage.SetActive(false);
                        return; // ���ټ���ִ����ͣ�߼�
                    }

                    // ����ǰ�������ý��棬Esc ������ͣ�˵�
                    if (settingsPanel && settingsPanel.activeSelf)
                    {
                        BackFromSettings();
                        return;
                    }

                    // ���������������ͣ��ָ�
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

            // ֹͣ��Ϸʱ�� & ��ѡ��ͣȫ����Ƶ
            Time.timeScale = 0f;
            if (pauseAudio) AudioListener.pause = true;

            // ��ʾ��꣨����������/������꣩
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Ϊ����/�ֱ�����Ĭ�ϰ�ť
            if (btnBackToGame)
                EventSystem.current?.SetSelectedGameObject(null);
        }

        public void ResumeGame()
        {
            // �ָ�ʱ��/��Ƶ������ UI ǰ
            Time.timeScale = 1f;
            if (pauseAudio) AudioListener.pause = false;

            isPaused = false;
            if (pausePanel) pausePanel.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(false);

            // ����ѡ��
            EventSystem.current?.SetSelectedGameObject(null);
        }

        private void BackToMenu()
        {
            // 1) ����ǰ����ͣ�����Ȼָ�ʱ������
            Time.timeScale = 1f;
            if (pauseAudio) AudioListener.pause = false;
            isPaused = false;

            // 2) ����������ͣ����
            if (pausePanel) pausePanel.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(false);

            // 3) �Զ�����һ�ε�ǰ���ȣ������˳�ǰ�����ȣ�
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

            // 4) �������״̬
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // ֱ���л�
            SceneManager.LoadScene(menuSceneName);
                
        }

        public void OpenSettings()
        {
            if (pausePanel) pausePanel.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(true);
        }

        // ��������巵����ͣ�˵�
        public void BackFromSettings()
        {
            if (settingsPanel) settingsPanel.SetActive(false);
            if (pausePanel) pausePanel.SetActive(true);
        }

        public IEnumerator LoadMenuAfterFrame()
        {
            // ��һ֡��������̧���¼����г���ʱ��ʧ
            yield return null;
            SceneManager.LoadScene(menuSceneName);
        }

        // ��������ͣ״̬�뿪��ǰ�����������л��ؿ�����ȷ����������ͣ
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
        //   ���浽����ǰ�浵�ۡ�
        // =========================
        private void SaveNowToCurrentSlot()
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                Debug.LogWarning("[SavePanelUI] ���˵��н�ֹ�浵��");
                return;
            }
            // 1) ȷ�� GameState ����
            if (GameState.Current == null)
                GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

            // 2) д�뵱ǰ�������루��ѡ���������
            GameState.Current.lastScene = SceneManager.GetActiveScene().name;
            if (savePlayerPos && playerPos != null)
            {
                GameState.Current.playerX = playerPos.position.x;
                GameState.Current.playerY = playerPos.position.y;
            }

            // 3) ѡ��ǰ�浵�� Key
            string slotKey = !string.IsNullOrEmpty(SaveSlotContext.CurrentKey)
                ? SaveSlotContext.CurrentKey
                : defaultSaveKey;

            // 4) �л��洢��˵��ò۲�����
            SaveManager.UsePlayerPrefs(slotKey);
            GameState.SaveNow();

            // 5) д������+ʱ�䡱Ԫ��Ϣ�����˵���ʾ���õ���
            PlayerPrefs.SetString(slotKey + "_metaScene", GameState.Current.lastScene ?? "");
            PlayerPrefs.SetString(slotKey + "_metaTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            PlayerPrefs.Save();
            
        }
    }
}

