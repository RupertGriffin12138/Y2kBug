using System;
using System.Collections;
using Save;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
        public Transform player;          // ��ѡ�������������
        public bool savePlayerPos = true; // ��ѡ�򱣴����2D����
        [Tooltip("���û�д����˵����� SaveSlotContext.CurrentKey����ʹ�ô�Ĭ�ϲۼ�������SaveSlot_1")]
        public string defaultSaveKey = "SaveSlot_1";

        private bool isPaused;

        public bool isBoard=false;

        public void Start()
        {
            // ��ʼ����
            if (pausePanel) pausePanel.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(false);

            // �󶨰�ť
            if (btnBackToGame) btnBackToGame.onClick.AddListener(ResumeGame);
            if (btnBackToMenu) btnBackToMenu.onClick.AddListener(BackToMenu);
            if (btnSettings) btnSettings.onClick.AddListener(OpenSettings);
            if (btnSave) btnSave.onClick.AddListener(SaveNowToCurrentSlot);
        }

        public void Update()
        {
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
            // ȷ���ָ�ʱ��/��Ƶ�������лز˵������� 0
            Time.timeScale = 1f;
            if (pauseAudio) AudioListener.pause = false;

            // �ɸ� 0.05�C0.1s �ð�ť�����Ч��������ѡ��
            StartCoroutine(LoadMenuAfterFrame());
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
            // 1) ȷ�� GameState ����
            if (GameState.Current == null)
                GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

            // 2) д�뵱ǰ�������루��ѡ���������
            GameState.Current.lastScene = SceneManager.GetActiveScene().name;
            if (savePlayerPos && player != null)
            {
                GameState.Current.playerX = player.position.x;
                GameState.Current.playerY = player.position.y;
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

            // 6) ������ʾ
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.ShowMessage($"�ѱ��浽��ǰ�浵��{slotKey}");
        }
    }
}

