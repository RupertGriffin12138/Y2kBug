using System.Collections;
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
    public Button settingsButton;
    //public Button backButton;

    public KeyCode toggleKey = KeyCode.Escape;

    void Start()
    {
        // 初始化：隐藏设置界面
        settingsPanel.SetActive(false);

        // 绑定按钮事件
        newGameButton.onClick.AddListener(OnNewGameClicked);
        settingsButton.onClick.AddListener(() => StartCoroutine(OpenSettingsDelayed()));
        //backButton.onClick.AddListener(CloseSettings);
    }

    // 点击“New Game”时加载场景
    void OnNewGameClicked()
    {
        // 可选：播放点击音效或动画延迟加载
        StartCoroutine(LoadSceneAfterDelay("C1S1 firework", 0.1f));
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
        bottomGroup.SetActive(false);
        settingsPanel.SetActive(true);
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
        settingsPanel.SetActive(false);
        bottomGroup.SetActive(true);
    }
}
