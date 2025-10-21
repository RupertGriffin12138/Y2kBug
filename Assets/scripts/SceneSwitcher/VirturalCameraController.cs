using UnityEngine;
using Cinemachine;
using System.Collections;

public class VirtualCameraController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera1; // ���������1
    public CinemachineVirtualCamera virtualCamera2; // ���������2
    public GameObject blackblack; // ��ɫ���ֶ���

    private float[,] transitionTimes = new float[3, 3]; // ��ά�������洢����ʱ��

    private CanvasGroup canvasGroup;
    private bool isTransitioning = false;

    private void Start()
    {
        // ��ʼ��CanvasGroup���
        if (blackblack != null)
        {
            canvasGroup = blackblack.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = blackblack.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f; // ��ʼ͸����Ϊ0
        }
        else
        {
            Debug.LogError("Blackblack GameObject is not assigned.");
        }

        // ��ʼ״̬�������������1���������������2
        SetCameraStatus(virtualCamera1, true);
        SetCameraStatus(virtualCamera2, false);

        //��ʼ���ȴ�ʱ������
        transitionTimes[1, 2] = 2f; // �������1�������2�Ĺ���ʱ��
        transitionTimes[2, 1] = 2f; // �������2�������1�Ĺ���ʱ��
    }

    // ����ĳ�����л����������
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isTransitioning)
        {
            StartCoroutine(TransitionCameras());
        }
    }

    // ���������������״̬
    private void SetCameraStatus(CinemachineVirtualCamera cam, bool isActive)
    {
        if (cam != null)
        {
            cam.Priority = isActive ? 10 : 0; // �������ȼ������û����
        }
        else
        {
            Debug.LogError("Virtual Camera is null!");
        }
    }

    // Э�̣��л��������������
    private IEnumerator TransitionCameras()
    {
        isTransitioning = true;

        // ��ʾ��ɫ���ֲ��𽥱䰵
        yield return StartCoroutine(FadeToBlack());

        // ȷ����ǰ��Ŀ�������
        int currentCameraIndex = virtualCamera1.Priority > virtualCamera2.Priority ? 1 : 2;
        int targetCameraIndex = virtualCamera1.Priority > virtualCamera2.Priority ? 2 : 1;

        // ��ȡ����ʱ��
        float transitionTime = GetTransitionTime(currentCameraIndex, targetCameraIndex);

        // �л������
        if (currentCameraIndex == 1)
        {
            SetCameraStatus(virtualCamera1, false);
            SetCameraStatus(virtualCamera2, true);
        }
        else
        {
            SetCameraStatus(virtualCamera1, true);
            SetCameraStatus(virtualCamera2, false);
        }

        // �ȴ�������л����
        yield return new WaitForSeconds(transitionTime);

        // ������ʾ����
        yield return StartCoroutine(FadeFromBlack());

        isTransitioning = false;
    }

    // Э�̣�ʹ��ɫ�����𽥱䰵
    private IEnumerator FadeToBlack()
    {
        float elapsedTime = 0f;
        while (elapsedTime < 2f)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / 2f);
            yield return null;
        }
        canvasGroup.alpha = 1f; // ȷ����ȫ��͸��
    }

    // Э�̣�ʹ��ɫ��������ʧ
    private IEnumerator FadeFromBlack()
    {
        float elapsedTime = 0f;
        while (elapsedTime < 2f)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / 2f);
            yield return null;
        }
        canvasGroup.alpha = 0f; // ȷ����ȫ͸��
    }

    // ��ȡ����ʱ��
    private float GetTransitionTime(int from, int to)
    {
        if (from >= 1 && from <= 2 && to >= 1 && to <= 2)
        {
            return transitionTimes[from, to];
        }
        else
        {
            Debug.LogWarning( "Invalid camera indices: {from} -> {to}");
            return 1f; // Ĭ�Ϲ���ʱ��
        }
    }
}