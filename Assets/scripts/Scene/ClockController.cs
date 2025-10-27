using UnityEngine;

namespace Scene
{
    public class ClockController : MonoBehaviour
    {
        public Transform second; // �������
        public Transform minute; // �������
        public Transform hour; // ʱ�����
        public Transform minuteAfter; // �µķ������
        public Transform hourAfter; // �µ�ʱ�����
        public Transform background; // ��������
        public Transform year1999; // ���1999����
        public Transform year2000; // ���2000����
        public Transform year0101; // ���0101����
        public Transform pivotMarker; // ���ڿ��ӻ�����ת���ı��
        public float rotationSpeed = 100f; // ��ת�ٶ�
        public float delayBeforeRotation = 0.5f; // ��תǰ���ӳ�ʱ��
        public float pauseAfterRotation = 1f; // ��ת�������ͣ��ʱ��
        public float delayBetweenYears = 0.5f; // 2000��ʾ��תΪ0101���ӳ�ʱ��

        private float totalRotation = 0f; // ��������ת�Ƕ�
        private bool isRotating = false; // ����Ƿ�������ת
        private bool isPaused = false; // ����Ƿ�����ͣ״̬
        private float startTime = 0f; // ��ʼ��ת��ʱ��
        private float pauseStartTime = 0f; // ͣ�ٿ�ʼ��ʱ��
        private float switchTime = 0f; // �л���ݵ�ʱ��

        void Start()
        {
            // ��ʼ״̬������second, minute_after, hour_after, year2000, year0101
            if (second != null) second.gameObject.SetActive(false);
            if (minuteAfter != null) minuteAfter.gameObject.SetActive(false);
            if (hourAfter != null) hourAfter.gameObject.SetActive(false);
            if (year2000 != null) year2000.gameObject.SetActive(false);
            if (year0101 != null) year0101.gameObject.SetActive(false);

            // ��ʼ״̬����ʾminute, hour, year1999
            if (minute != null) minute.gameObject.SetActive(true);
            if (hour != null) hour.gameObject.SetActive(true);
            if (year1999 != null) year1999.gameObject.SetActive(true);
        }

        void Update()
        {
            // ����Ƿ���'e'��
            if (Input.GetKeyDown(KeyCode.E) && !isRotating && !isPaused)
            {
                ShowSecondAndStartRotation();
            }

            // ���������ת����δ���30�ȣ��������ת
            if (isRotating && totalRotation < 30f)
            {
                float currentTime = Time.time - startTime;
                if (currentTime >= delayBeforeRotation)
                {
                    // ��ȡ��ת���ĵ���������
                    Vector3 pivotWorldPosition = pivotMarker.position;

                    // ������ת�Ƕ�����
                    float rotationStep = -rotationSpeed * Time.deltaTime;
                    totalRotation -= rotationStep;

                    // ����ת������ת����
                    second.RotateAround(pivotWorldPosition, Vector3.forward, rotationStep);
                }
            }
            else if (isRotating && totalRotation >= 30f)
            {
                // ��ת��ɺ�ֹͣ��ת����ʼͣ��
                StopRotationAndPause();
            }

            // ����ͣ���߼�
            if (isPaused)
            {
                float currentTime = Time.time - pauseStartTime;
                if (currentTime >= pauseAfterRotation)
                {
                    // ͣ�ٽ������л���ʾ�Ķ���
                    SwitchObjectsAfterPause();
                }
            }

            // ��������л��߼�
            if (!isRotating && !isPaused && totalRotation >= 30f)
            {
                float currentTime = Time.time - switchTime;
                if (currentTime >= delayBetweenYears)
                {
                    SwitchYearDisplay();
                }
            }
        }

        void ShowSecondAndStartRotation()
        {
            if (second != null) second.gameObject.SetActive(true);
            isRotating = true;
            startTime = Time.time;
            totalRotation = 0f;
        }

        void StopRotationAndPause()
        {
            isRotating = false;
            isPaused = true;
            pauseStartTime = Time.time;
        }

        void SwitchObjectsAfterPause()
        {
            isPaused = false;
            switchTime = Time.time;

            // ����minute, hour, year1999
            if (minute != null) minute.gameObject.SetActive(false);
            if (hour != null) hour.gameObject.SetActive(false);
            if (year1999 != null) year1999.gameObject.SetActive(false);

            // ��ʾminute_after, hour_after, year2000
            if (minuteAfter != null) minuteAfter.gameObject.SetActive(true);
            if (hourAfter != null) hourAfter.gameObject.SetActive(true);
            if (year2000 != null) year2000.gameObject.SetActive(true);
        }

        void SwitchYearDisplay()
        {
            // ����year2000
            if (year2000 != null) year2000.gameObject.SetActive(false);

            // ��ʾyear0101
            if (year0101 != null) year0101.gameObject.SetActive(true);
        }
    }
}


