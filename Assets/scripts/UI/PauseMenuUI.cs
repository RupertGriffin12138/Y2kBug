using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public Button btnBackToGame;
    public Button btnBackToMenu;

    [Header("Config")]
    public string menuSceneName = "TestMenu";
    public KeyCode toggleKey = KeyCode.Escape;
    public bool pauseAudio = true;

    bool isPaused;

    void Start()
    {
        // ��ʼ����
        if (pausePanel) pausePanel.SetActive(false);

        // �󶨰�ť
        if (btnBackToGame) btnBackToGame.onClick.AddListener(ResumeGame);
        if (btnBackToMenu) btnBackToMenu.onClick.AddListener(BackToMenu);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    void PauseGame()
    {
        isPaused = true;
        if (pausePanel) pausePanel.SetActive(true);

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

        // ����ѡ��
        EventSystem.current?.SetSelectedGameObject(null);
    }

    void BackToMenu()
    {
        // ȷ���ָ�ʱ��/��Ƶ�������лز˵������� 0
        Time.timeScale = 1f;
        if (pauseAudio) AudioListener.pause = false;

        // �ɸ� 0.05�C0.1s �ð�ť�����Ч��������ѡ��
        StartCoroutine(LoadMenuAfterFrame());
    }

    IEnumerator LoadMenuAfterFrame()
    {
        // ��һ֡��������̧���¼����г���ʱ��ʧ
        yield return null;
        SceneManager.LoadScene(menuSceneName);
    }

    // ��������ͣ״̬�뿪��ǰ�����������л��ؿ�����ȷ����������ͣ
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
