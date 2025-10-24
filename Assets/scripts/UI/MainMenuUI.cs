using System.Collections;
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
    public Button settingsButton;
    //public Button backButton;

    public KeyCode toggleKey = KeyCode.Escape;

    void Start()
    {
        // ��ʼ�����������ý���
        settingsPanel.SetActive(false);

        // �󶨰�ť�¼�
        newGameButton.onClick.AddListener(OnNewGameClicked);
        settingsButton.onClick.AddListener(() => StartCoroutine(OpenSettingsDelayed()));
        //backButton.onClick.AddListener(CloseSettings);
    }

    // �����New Game��ʱ���س���
    void OnNewGameClicked()
    {
        // ��ѡ�����ŵ����Ч�򶯻��ӳټ���
        StartCoroutine(LoadSceneAfterDelay("C1S1 firework", 0.1f));
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
        bottomGroup.SetActive(false);
        settingsPanel.SetActive(true);
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
        settingsPanel.SetActive(false);
        bottomGroup.SetActive(true);
    }
}
