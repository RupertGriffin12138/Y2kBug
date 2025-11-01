using UnityEngine;

namespace Riddle.Abacus
{
    public sealed class AbacusControl : MonoBehaviour
    {
        public static AbacusControl Instance; // 单例访问

        [Header("Frame引用")]
        public GameObject frame0;   // 上层框
        public GameObject frame1;   // 下层框

        [Header("参数")]
        public int lineCount = 9;
        public float stepDistance = 0.3f;

        private readonly int[] lineNum = new int[2];  // [0]=上层行号, [1]=下层行号
        private int frameNum = 0;            // 当前层（0=上, 1=下）
        private bool inputLocked = false;

        private Vector3 frame0StartPos;
        private Vector3 frame1StartPos;

        public int CurrentLine => lineNum[frameNum];
        public int CurrentFrame => frameNum;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            frame0StartPos = frame0.transform.position;
            frame1StartPos = frame1.transform.position;
            SetFrame(0); // 默认在上层
        }

        private void Update()
        {
            if (inputLocked) return;

            if (Input.GetKeyDown(KeyCode.W))
                SetFrame(0);
            else if (Input.GetKeyDown(KeyCode.S))
                SetFrame(1);

            if (Input.GetKeyDown(KeyCode.D))
                MoveLine(1);
            else if (Input.GetKeyDown(KeyCode.A))
                MoveLine(-1);
        }

        private void SetFrame(int newFrame)
        {
            int oldFrame = frameNum; // 记录之前是哪一层
            frameNum = Mathf.Clamp(newFrame, 0, 1);

            // 如果切层，让新层的列号与旧层同步
            lineNum[frameNum] = lineNum[oldFrame];

            frame0.SetActive(frameNum == 0);
            frame1.SetActive(frameNum == 1);

            // 更新位置（保持列号一致）
            UpdateFramePosition(frameNum);
        }


        private void MoveLine(int dir)
        {
            // 仅修改当前层对应的行号
            lineNum[frameNum] = (lineNum[frameNum] + dir + lineCount) % lineCount;
            UpdateFramePosition(frameNum);
        }

        private void UpdateFramePosition(int frameIndex)
        {
            Vector3 step = new Vector3(stepDistance, 0, 0);

            if (frameIndex == 0)
                frame0.transform.position = frame0StartPos + step * lineNum[0];
            else
                frame1.transform.position = frame1StartPos + step * lineNum[1];
        }

        // 外部接口
        public void LockInput() => inputLocked = true;
        public void UnlockInput() => inputLocked = false;
    }
}
