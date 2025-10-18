using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject savePanel;
    public Button btnBackToGame;
    public Button btnBackToMenu;
    public Button btnSettings;
    public Button btnSave;

    [Header("Config")]
    public string menuSceneName = "TestMenu";
    public KeyCode toggleKey = KeyCode.Escape;
    public bool pauseAudio = true;

    bool isPaused;

    void Start()
    {
        // 初始隐藏
        if (pausePanel) pausePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (savePanel) savePanel.SetActive(false);

        // 绑定按钮
        if (btnBackToGame) btnBackToGame.onClick.AddListener(ResumeGame);
        if (btnBackToMenu) btnBackToMenu.onClick.AddListener(BackToMenu);
        if (btnSettings) btnSettings.onClick.AddListener(OpenSettings);
        if (btnSave) btnSave.onClick.AddListener(OpenSave);
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

            // 若当前处于save界面，Esc 返回暂停菜单
            if (savePanel != null && savePanel.activeSelf)
            {
                BackFromSave();
                return;
            }

            // 其他情况，控制暂停与恢复
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    void PauseGame()
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

    void BackToMenu()
    {
        // 确保恢复时间/音频，避免切回菜单后仍是 0
        Time.timeScale = 1f;
        if (pauseAudio) AudioListener.pause = false;

        // 可给 0.05–0.1s 让按钮点击动效结束（可选）
        StartCoroutine(LoadMenuAfterFrame());
    }

    void OpenSettings()
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

    void OpenSave()
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (savePanel) savePanel.SetActive(true);
    }

    // 从设置面板返回暂停菜单
    public void BackFromSave()
    {
        if (savePanel) savePanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(true);
    }

    IEnumerator LoadMenuAfterFrame()
    {
        // 等一帧，避免点击抬起事件在切场景时丢失
        yield return null;
        SceneManager.LoadScene(menuSceneName);
    }

    // 若带着暂停状态离开当前场景（例如切换关卡），确保不残留暂停
    void OnDisable()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
            if (pauseAudio) AudioListener.pause = false;
            isPaused = false;
        }
    }
}
