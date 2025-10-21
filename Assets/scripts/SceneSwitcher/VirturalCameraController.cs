using UnityEngine;
using Cinemachine;
using System.Collections;

public class VirtualCameraController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera1; // 虚拟摄像机1
    public CinemachineVirtualCamera virtualCamera2; // 虚拟摄像机2
    public GameObject blackblack; // 黑色遮罩对象

    private CanvasGroup canvasGroup;
    private bool isTransitioning = false;

    private void Start()
    {
        // 初始化CanvasGroup组件
        if (blackblack != null)
        {
            canvasGroup = blackblack.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = blackblack.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f; // 初始透明度为0
        }
        else
        {
            Debug.LogError("Blackblack GameObject is not assigned.");
        }

        // 初始状态启用虚拟摄像机1，禁用虚拟摄像机2
        SetCameraStatus(virtualCamera1, true);
        SetCameraStatus(virtualCamera2, false);
    }

    // 按下某个键切换虚拟摄像机
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isTransitioning)
        {
            StartCoroutine(TransitionCameras());
        }
    }

    // 设置虚拟摄像机的状态
    private void SetCameraStatus(CinemachineVirtualCamera cam, bool isActive)
    {
        if (cam != null)
        {
            cam.Priority = isActive ? 10 : 0; // 设置优先级以启用或禁用
        }
        else
        {
            Debug.LogError("Virtual Camera is null!");
        }
    }

    // 协程：切换两个虚拟摄像机
    private IEnumerator TransitionCameras()
    {
        isTransitioning = true;

        // 显示黑色遮罩并逐渐变暗
        yield return StartCoroutine(FadeToBlack());

        // 切换摄像机
        if (virtualCamera1.Priority > virtualCamera2.Priority)
        {
            SetCameraStatus(virtualCamera1, false);
            SetCameraStatus(virtualCamera2, true);
        }
        else
        {
            SetCameraStatus(virtualCamera1, true);
            SetCameraStatus(virtualCamera2, false);
        }

        // 等待摄像机切换完成（假设摄像机切换时间为1秒）
        yield return new WaitForSeconds(1f);

        // 渐渐显示背景
        yield return StartCoroutine(FadeFromBlack());

        isTransitioning = false;
    }

    // 协程：使黑色遮罩逐渐变暗
    private IEnumerator FadeToBlack()
    {
        float elapsedTime = 0f;
        while (elapsedTime < 2f)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / 2f);
            yield return null;
        }
        canvasGroup.alpha = 1f; // 确保完全不透明
    }

    // 协程：使黑色遮罩逐渐消失
    private IEnumerator FadeFromBlack()
    {
        float elapsedTime = 0f;
        while (elapsedTime < 2f)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / 2f);
            yield return null;
        }
        canvasGroup.alpha = 0f; // 确保完全透明
    }
}