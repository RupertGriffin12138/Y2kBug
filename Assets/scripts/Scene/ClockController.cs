using UnityEngine;

namespace Scene
{
    public class ClockController : MonoBehaviour
    {
        public Transform second; // 秒针对象
        public Transform minute; // 分针对象
        public Transform hour; // 时针对象
        public Transform minuteAfter; // 新的分针对象
        public Transform hourAfter; // 新的时针对象
        public Transform background; // 背景对象
        public Transform year1999; // 年份1999对象
        public Transform year2000; // 年份2000对象
        public Transform year0101; // 年份0101对象
        public Transform pivotMarker; // 用于可视化的旋转中心标记
        public float rotationSpeed = 100f; // 旋转速度
        public float delayBeforeRotation = 0.5f; // 旋转前的延迟时间
        public float pauseAfterRotation = 1f; // 旋转结束后的停顿时间
        public float delayBetweenYears = 0.5f; // 2000显示后转为0101的延迟时间

        private float totalRotation = 0f; // 跟踪总旋转角度
        private bool isRotating = false; // 标记是否正在旋转
        private bool isPaused = false; // 标记是否处于暂停状态
        private float startTime = 0f; // 开始旋转的时间
        private float pauseStartTime = 0f; // 停顿开始的时间
        private float switchTime = 0f; // 切换年份的时间

        void Start()
        {
            // 初始状态下隐藏second, minute_after, hour_after, year2000, year0101
            if (second != null) second.gameObject.SetActive(false);
            if (minuteAfter != null) minuteAfter.gameObject.SetActive(false);
            if (hourAfter != null) hourAfter.gameObject.SetActive(false);
            if (year2000 != null) year2000.gameObject.SetActive(false);
            if (year0101 != null) year0101.gameObject.SetActive(false);

            // 初始状态下显示minute, hour, year1999
            if (minute != null) minute.gameObject.SetActive(true);
            if (hour != null) hour.gameObject.SetActive(true);
            if (year1999 != null) year1999.gameObject.SetActive(true);
        }

        void Update()
        {
            // 检查是否按下'e'键
            if (Input.GetKeyDown(KeyCode.E) && !isRotating && !isPaused)
            {
                ShowSecondAndStartRotation();
            }

            // 如果正在旋转并且未完成30度，则继续旋转
            if (isRotating && totalRotation < 30f)
            {
                float currentTime = Time.time - startTime;
                if (currentTime >= delayBeforeRotation)
                {
                    // 获取旋转中心的世界坐标
                    Vector3 pivotWorldPosition = pivotMarker.position;

                    // 计算旋转角度增量
                    float rotationStep = -rotationSpeed * Time.deltaTime;
                    totalRotation -= rotationStep;

                    // 绕旋转中心旋转物体
                    second.RotateAround(pivotWorldPosition, Vector3.forward, rotationStep);
                }
            }
            else if (isRotating && totalRotation >= 30f)
            {
                // 旋转完成后停止旋转并开始停顿
                StopRotationAndPause();
            }

            // 处理停顿逻辑
            if (isPaused)
            {
                float currentTime = Time.time - pauseStartTime;
                if (currentTime >= pauseAfterRotation)
                {
                    // 停顿结束后切换显示的对象
                    SwitchObjectsAfterPause();
                }
            }

            // 处理年份切换逻辑
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

            // 隐藏minute, hour, year1999
            if (minute != null) minute.gameObject.SetActive(false);
            if (hour != null) hour.gameObject.SetActive(false);
            if (year1999 != null) year1999.gameObject.SetActive(false);

            // 显示minute_after, hour_after, year2000
            if (minuteAfter != null) minuteAfter.gameObject.SetActive(true);
            if (hourAfter != null) hourAfter.gameObject.SetActive(true);
            if (year2000 != null) year2000.gameObject.SetActive(true);
        }

        void SwitchYearDisplay()
        {
            // 隐藏year2000
            if (year2000 != null) year2000.gameObject.SetActive(false);

            // 显示year0101
            if (year0101 != null) year0101.gameObject.SetActive(true);
        }
    }
}


