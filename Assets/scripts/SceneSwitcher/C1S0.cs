using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSceneSwitcher : MonoBehaviour
{
    // Ŀ�곡������
    public string targetSceneName = "C1S1";

    void Start()
    {
        // ����Э�����ӳټ���Ŀ�곡��
        StartCoroutine(LoadTargetSceneAfterDelay());
    }

    IEnumerator LoadTargetSceneAfterDelay()
    {
        // �ӳ�2��
        yield return new WaitForSeconds(2f);

        // ����Ŀ�곡��
        SceneManager.LoadScene(targetSceneName);
    }
}