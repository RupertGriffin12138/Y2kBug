using UnityEngine;
using Cinemachine;

public class VirtualCameraController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera1; // ���������1
    public CinemachineVirtualCamera virtualCamera2; // ���������2

    private void Start()
    {
        // ��ʼ״̬�������������1���������������2
        EnableVirtualCamera(virtualCamera1);
        DisableVirtualCamera(virtualCamera2);
    }

    // ����ĳ�����л����������
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            // �л����������
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

    // ����ָ�������������
    private void EnableVirtualCamera(CinemachineVirtualCamera cam)
    {
        if (cam != null)
        {
            cam.Priority = 10; // ���ýϸߵ����ȼ�������
        }
        else
        {
            Debug.LogError("Virtual Camera is null!");
        }
    }

    // ����ָ�������������
    private void DisableVirtualCamera(CinemachineVirtualCamera cam)
    {
        if (cam != null)
        {
            cam.Priority = 0; // ���ýϵ͵����ȼ��Խ���
        }
        else
        {
            Debug.LogError("Virtual Camera is null!");
        }
    }

    // �л��������������
    private void SwitchCameras(CinemachineVirtualCamera activeCam, CinemachineVirtualCamera inactiveCam)
    {
        EnableVirtualCamera(activeCam);
        DisableVirtualCamera(inactiveCam);
        Debug.Log( "Switched to {activeCam.name}");
    }
}