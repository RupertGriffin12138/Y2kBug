using System;
using System.Collections;
using System.IO;
using Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#pragma warning disable CS0219 // �����ѱ���ֵ������δʹ�ù�����ֵ

// ���� TextMeshPro��using TMPro; ���� Text ���� TMP_Text

namespace UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Main Menu Panels")]
        public GameObject bottomGroup;     // ���˵���ť��
        public GameObject settingsPanel;   // ���ò˵����
        public GameObject loadPanel;       // ��λѡ����壨8�ۣ�
        public GameObject confirmPanel;    // ȷ�϶Ի���Yes/No��
        public Text confirmText;           // ȷ����ʾ�ı�

        [Header("Buttons")]
        public Button newGameButton;
        public Button continueButton;
        public Button settingsButton;
        public Button clearSaveButton;
        public Button btnYes;              // ȷ�϶Ի��� Yes
        public Button btnNo;               // ȷ�϶Ի��� No
        public Button[] slotButtons = new Button[8]; // 8���۰�ť
        public Text[] slotLabels = new Text[8];      // ��ѡ����ʾ������+ʱ�䡱

        [Header("Slot Visuals (optional)")]
        [Tooltip("�ձ귨���д浵ʱ��ʾ savedBadges[i]���ղ���ʾ emptyBadges[i]")]
        public GameObject[] savedBadges = new GameObject[8];
        public GameObject[] emptyBadges = new GameObject[8];

        [Tooltip("����ͼ���������Ƿ��д浵�л� Image �� Sprite")]
        public Image[] slotImages = new Image[8];
        public Sprite savedSprite;
        public Sprite emptySprite;

        [Header("Config")]
        public string firstSceneName = "C1S0";
        public KeyCode toggleKey = KeyCode.Escape;

        public enum SaveBackend { PlayerPrefs, File }
        [Tooltip("Demo �Ƽ� PlayerPrefs����ѡ File������ persistentDataPath д��� json �ļ���")]
        public SaveBackend backend = SaveBackend.PlayerPrefs;

        [Tooltip("��ʹ�� PlayerPrefs ��Ϊ���ʱ����Keyǰ׺")]
        public string saveKeyPrefix = "SaveSlot_";   // ʵ��Ϊ SaveSlot_1..SaveSlot_8

        [Tooltip("��ʹ���ļ���Ϊ���ʱ���ļ�ǰ׺")]
        public string fileNamePrefix = "save_";      // ʵ��Ϊ save_1.json..save_8.json

        // ���� �ڲ�״̬ ���� 
        private int pendingIndex = -1;   // ����ȷ�ϵĲ�λ

        void Awake()
        {
            // ��ʼ����趨����ָ���1����������/����ʱ���л�����ѡ�ۣ�
            if (backend == SaveBackend.PlayerPrefs)
                SaveManager.UsePlayerPrefs(GetSlotKey(0));
            else
                SaveManager.UseFile(GetSlotKey(0));
        }

        void Start()
        {
            // ��ʼ������
            if (settingsPanel) settingsPanel.SetActive(false);
            if (loadPanel) loadPanel.SetActive(false);
            if (confirmPanel) confirmPanel.SetActive(false);

            // ������ť
            if (newGameButton) newGameButton.onClick.AddListener(OnNewGameClicked);
            if (continueButton) continueButton.onClick.AddListener(OnContinueClicked);
            if (settingsButton) settingsButton.onClick.AddListener(() => StartCoroutine(OpenSettingsDelayed()));
            if (clearSaveButton) clearSaveButton.onClick.AddListener(OnClearSaveClicked);

            // �󶨲۰�ť
            for (int i = 0; i < slotButtons.Length; i++)
            {
                int idx = i;
                if (slotButtons[i])
                    slotButtons[i].onClick.AddListener(() => OnClickSlot(idx));
            }

            // ��ȷ�Ͽ�
            if (btnYes) btnYes.onClick.AddListener(OnConfirmYes);
            if (btnNo) btnNo.onClick.AddListener(() => { if (confirmPanel) confirmPanel.SetActive(false); });

            RefreshContinueButtonState();
            RefreshSlotsUI();
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                // ���ý���� �� ����
                if (settingsPanel && settingsPanel.activeSelf) { BackFromSettings(); return; }

                // ��λ���� �� �رղ�λ���
                if (loadPanel && loadPanel.activeSelf)
                {
                    loadPanel.SetActive(false);
                    if (bottomGroup) bottomGroup.SetActive(true);
                    return;
                }

                // ȷ������ �� �ر�ȷ��
                if (confirmPanel && confirmPanel.activeSelf)
                {
                    confirmPanel.SetActive(false);
                    return;
                }
            }
        }

        // ========= ����ť������Ϸ =========
        private void OnNewGameClicked()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            
            PlayerPrefs.SetInt("BoardKey_Prefab", 1);
            int slot = FindFirstEmptySlotIndex();
            if (slot == -1)
            {
                slot = 0; // Ҳ���Ե���ѡ�񸲸��ĸ���
                Debug.LogWarning("[MainMenuUI] ���в�������Ĭ�ϸ��ǲ�1��");
            }

            UseSlot(slot);
            GameState.NewGame(firstSceneName);
            GameState.SaveNow();
            SaveMeta(slot, firstSceneName, DateTime.Now);

            SaveSlotContext.CurrentKey = GetSlotKey(slot);
            StartCoroutine(LoadSceneAfterDelay(firstSceneName, 0.1f));
        }

        // ========= ����ť��������Ϸ���򿪲�λ���/��ȡ�� =========
        void OnContinueClicked()
        {
            if (bottomGroup) bottomGroup.SetActive(false);
            if (loadPanel) loadPanel.SetActive(true);
            RefreshSlotsUI();
        }

        // ========= �嵵��ʾ��ȫ�� =========
        void OnClearSaveClicked()
        {
            for (int i = 0; i < 8; i++) WipeSlot(i);
            RefreshContinueButtonState();
            RefreshSlotsUI();
            Debug.Log("[MainMenuUI] ��������д浵��");
        }

        // ========= ��λ��ť��� =========
        void OnClickSlot(int index)
        {
            if (!HasExistingSave(index)) return; // �ղ۱������ˣ�����ͨ�����������ٷ���һ��
            pendingIndex = index;

            string info = GetSlotInfo(index);
            if (confirmPanel) confirmPanel.SetActive(true);
            if (confirmText) confirmText.text = $"��ȡ��λ {index + 1}��\n{info}";
        }

        // ========= ȷ�ϣ���ȡ������浵 =========
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

        // ========= ������� =========
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

        // ========= UI ˢ�� =========
        void RefreshContinueButtonState()
        {
            bool hasAny = false;
            for (int i = 0; i < 8; i++) { if (HasExistingSave(i)) { hasAny = true; break; } }
            // ���ϣ���޴浵ʱ���á�������������һ�и�Ϊ��continueButton.interactable = hasAny;
            if (continueButton) continueButton.interactable = true;
        }

        void RefreshSlotsUI()
        {
            for (int i = 0; i < 8; i++)
            {
                bool has = HasExistingSave(i);

                // 1) �������д浵���ܵ�
                if (slotButtons != null && i < slotButtons.Length && slotButtons[i])
                    slotButtons[i].interactable = has;

                // 2) �ı�����ʾ������+ʱ�䡱�򡰿մ浵��
                if (slotLabels != null && i < slotLabels.Length && slotLabels[i])
                    slotLabels[i].text = has ? GetSlotInfo(i) : "�մ浵";

                // 3) �ձ귨����/������
                if (savedBadges != null && i < savedBadges.Length && savedBadges[i])
                    savedBadges[i].SetActive(has);
                if (emptyBadges != null && i < emptyBadges.Length && emptyBadges[i])
                    emptyBadges[i].SetActive(!has);

                // 4) ����ͼ�����л���ť Image �� Sprite
                if (slotImages != null && i < slotImages.Length && slotImages[i])
                {
                    if (has && savedSprite) slotImages[i].sprite = savedSprite;
                    else if (!has && emptySprite) slotImages[i].sprite = emptySprite;
                }
            }
        }

        // ========= ��λ���� =========
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
            // ��ȡԪ��Ϣ������+ʱ�䣩
            string scene, time;
            GetSlotMeta(index, out scene, out time);
            if (string.IsNullOrEmpty(scene)) scene = "(δ֪����)";
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
