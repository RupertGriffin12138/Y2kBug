using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Menu Panels")]
    public GameObject bottomGroup;   // ���˵���ť��
    public GameObject settingsPanel; // ���ò˵����

    [Header("Buttons")]
    public Button newGameButton;
    public Button continueButton;
    public Button settingsButton;
    public Button clearSaveButton;
    //public Button backButton;

    [Header("Config")]
    public string firstSceneName = "C1S0";
    public KeyCode toggleKey = KeyCode.Escape;

    public enum SaveBackend { PlayerPrefs, File }
    [Tooltip("Demo �Ƽ� PlayerPrefs����ѡ File������ persistentDataPath дһ�� json �ļ���")]
    public SaveBackend backend = SaveBackend.PlayerPrefs;

    [Tooltip("��ʹ�� PlayerPrefs ��Ϊ���ʱ�� Key")]
    public string saveKey = "SaveSlot_1";

    [Tooltip("��ʹ���ļ���Ϊ���ʱ���ļ���")]
    public string fileName = "demo_save.json";

    void Awake()
    {
        // ѡ��洢��ˣ������ǵ�2���е� SaveManager ���룩
        if (backend == SaveBackend.PlayerPrefs)
            SaveManager.UsePlayerPrefs(saveKey);
        else
            SaveManager.UseFile(fileName);
    }


    void Start()
    {
        // ��ʼ�����������ý���
        settingsPanel.SetActive(false);

        // �󶨰�ť�¼�
        if (newGameButton) newGameButton.onClick.AddListener(OnNewGameClicked);
        if (settingsButton) settingsButton.onClick.AddListener(() => StartCoroutine(OpenSettingsDelayed()));
        if (continueButton) continueButton.onClick.AddListener(OnContinueClicked);
        if (clearSaveButton) clearSaveButton.onClick.AddListener(OnClearSaveClicked);

        // �����Ƿ���ڴ浵��������������Ϸ���Ƿ�ɵ�
        RefreshContinueButtonState();
    }

    // ���� ����Ϸ������Ĭ�ϴ浵 �� �������� �� �����׹� ����
    void OnNewGameClicked()
    {
        GameState.NewGame(firstSceneName);
        GameState.SaveNow(); // ��ѡ������д���ʼ�浵�����㡰������Ϸ��
        StartCoroutine(LoadSceneAfterDelay(firstSceneName, 0.1f));
    }

    // ���� ������Ϸ����ȡ�浵 �� ���� lastScene�����׹أ� ����
    void OnContinueClicked()
    {
        GameState.LoadGameOrNew(firstSceneName);
        string scene = string.IsNullOrEmpty(GameState.Current?.lastScene)
            ? firstSceneName
            : GameState.Current.lastScene;

        StartCoroutine(LoadSceneAfterDelay(scene, 0.1f));
    }

    // ���� �嵵����ѡ����ɾ���浵��ˢ�� UI ����
    void OnClearSaveClicked()
    {
        SaveManager.Wipe();
        RefreshContinueButtonState();
        // Ҳ���Ե�������ʾ��������浵
        Debug.Log("[MainMenuUI] ������浵��");
    }

    IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator OpenSettingsDelayed()
    {
        yield return new WaitForSeconds(0.2f); // �ӳ� 0.1 ��
        OpenSettings();
    }


    void OpenSettings()
    {
        if (bottomGroup) bottomGroup.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            // ����ǰ�������ý��棬Esc ������ͣ�˵�
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                BackFromSettings();
                return;
            }
        }
    }

    void BackFromSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
        if (bottomGroup) bottomGroup.SetActive(true);
    }

    // ���� ���ߣ�ˢ�¡�������Ϸ����ť������ ����
    void RefreshContinueButtonState()
    {
        bool hasSave = HasExistingSave();
        if (continueButton) continueButton.interactable = hasSave;
    }

    // ���� ̽���Ƿ��д浵��������ѡ��˷ֱ��ж� ����
    bool HasExistingSave()
    {
        if (backend == SaveBackend.PlayerPrefs)
        {
            return PlayerPrefs.HasKey(saveKey) && !string.IsNullOrEmpty(PlayerPrefs.GetString(saveKey));
        }
        else
        {
            string path = Path.Combine(Application.persistentDataPath, fileName);
            return File.Exists(path) && new FileInfo(path).Length > 2; // �����жϷǿ�
        }
    }
}
