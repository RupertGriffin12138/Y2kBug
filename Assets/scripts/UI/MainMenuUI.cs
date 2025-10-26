using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Menu Panels")]
    public GameObject bottomGroup;   // 主菜单按钮组
    public GameObject settingsPanel; // 设置菜单面板

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
    [Tooltip("Demo 推荐 PlayerPrefs；若选 File，会在 persistentDataPath 写一个 json 文件。")]
    public SaveBackend backend = SaveBackend.PlayerPrefs;

    [Tooltip("当使用 PlayerPrefs 作为后端时的 Key")]
    public string saveKey = "SaveSlot_1";

    [Tooltip("当使用文件作为后端时的文件名")]
    public string fileName = "demo_save.json";

    void Awake()
    {
        // 选择存储后端（与我们第2步中的 SaveManager 对齐）
        if (backend == SaveBackend.PlayerPrefs)
            SaveManager.UsePlayerPrefs(saveKey);
        else
            SaveManager.UseFile(fileName);
    }


    void Start()
    {
        // 初始化：隐藏设置界面
        settingsPanel.SetActive(false);

        // 绑定按钮事件
        if (newGameButton) newGameButton.onClick.AddListener(OnNewGameClicked);
        if (settingsButton) settingsButton.onClick.AddListener(() => StartCoroutine(OpenSettingsDelayed()));
        if (continueButton) continueButton.onClick.AddListener(OnContinueClicked);
        if (clearSaveButton) clearSaveButton.onClick.AddListener(OnClearSaveClicked);

        // 根据是否存在存档，决定“继续游戏”是否可点
        RefreshContinueButtonState();
    }

    // —— 新游戏：创建默认存档 → 立即保存 → 加载首关 ——
    void OnNewGameClicked()
    {
        GameState.NewGame(firstSceneName);
        GameState.SaveNow(); // 可选：立刻写入初始存档，方便“继续游戏”
        StartCoroutine(LoadSceneAfterDelay(firstSceneName, 0.1f));
    }

    // —— 继续游戏：读取存档 → 加载 lastScene（或首关） ——
    void OnContinueClicked()
    {
        GameState.LoadGameOrNew(firstSceneName);
        string scene = string.IsNullOrEmpty(GameState.Current?.lastScene)
            ? firstSceneName
            : GameState.Current.lastScene;

        StartCoroutine(LoadSceneAfterDelay(scene, 0.1f));
    }

    // —— 清档（可选）：删除存档并刷新 UI ——
    void OnClearSaveClicked()
    {
        SaveManager.Wipe();
        RefreshContinueButtonState();
        // 也可以弹个轻提示：已清除存档
        Debug.Log("[MainMenuUI] 已清除存档。");
    }

    IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator OpenSettingsDelayed()
    {
        yield return new WaitForSeconds(0.2f); // 延迟 0.1 秒
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
            // 若当前处于设置界面，Esc 返回暂停菜单
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

    // —— 工具：刷新“继续游戏”按钮可用性 ——
    void RefreshContinueButtonState()
    {
        bool hasSave = HasExistingSave();
        if (continueButton) continueButton.interactable = hasSave;
    }

    // —— 探测是否有存档：根据所选后端分别判断 ——
    bool HasExistingSave()
    {
        if (backend == SaveBackend.PlayerPrefs)
        {
            return PlayerPrefs.HasKey(saveKey) && !string.IsNullOrEmpty(PlayerPrefs.GetString(saveKey));
        }
        else
        {
            string path = Path.Combine(Application.persistentDataPath, fileName);
            return File.Exists(path) && new FileInfo(path).Length > 2; // 粗略判断非空
        }
    }
}
