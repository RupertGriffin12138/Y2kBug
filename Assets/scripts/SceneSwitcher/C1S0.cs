using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSceneSwitcher : MonoBehaviour
{
    // 目标场景名称
    public string targetSceneName = "C1S1";

    void Start()
    {
        // 启动协程以延迟加载目标场景
        StartCoroutine(LoadTargetSceneAfterDelay());
    }

    IEnumerator LoadTargetSceneAfterDelay()
    {
        // 延迟2秒
        yield return new WaitForSeconds(2f);

        // 加载目标场景
        SceneManager.LoadScene(targetSceneName);
    }
}