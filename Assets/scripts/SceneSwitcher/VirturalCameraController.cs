using UnityEngine;
using Cinemachine;

public class VirtualCameraController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera1; // 虚拟摄像机1
    public CinemachineVirtualCamera virtualCamera2; // 虚拟摄像机2

    private void Start()
    {
        // 初始状态启用虚拟摄像机1，禁用虚拟摄像机2
        EnableVirtualCamera(virtualCamera1);
        DisableVirtualCamera(virtualCamera2);
    }

    // 按下某个键切换虚拟摄像机
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            // 切换虚拟摄像机
            if (virtualCamera1.Priority > virtualCamera2.Priority)
            {
                SwitchCameras(virtualCamera2, virtualCamera1);
            }
            else
            {
                SwitchCameras(virtualCamera1, virtualCamera2);
            }
        }
    }

    // 启用指定的虚拟摄像机
    private void EnableVirtualCamera(CinemachineVirtualCamera cam)
    {
        if (cam != null)
        {
            cam.Priority = 10; // 设置较高的优先级以启用
        }
        else
        {
            Debug.LogError("Virtual Camera is null!");
        }
    }

    // 禁用指定的虚拟摄像机
    private void DisableVirtualCamera(CinemachineVirtualCamera cam)
    {
        if (cam != null)
        {
            cam.Priority = 0; // 设置较低的优先级以禁用
        }
        else
        {
            Debug.LogError("Virtual Camera is null!");
        }
    }

    // 切换两个虚拟摄像机
    private void SwitchCameras(CinemachineVirtualCamera activeCam, CinemachineVirtualCamera inactiveCam)
    {
        EnableVirtualCamera(activeCam);
        DisableVirtualCamera(inactiveCam);
        Debug.Log( "Switched to {activeCam.name}");
    }
}