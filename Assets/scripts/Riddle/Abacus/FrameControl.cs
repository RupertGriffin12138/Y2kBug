using UnityEngine;

namespace Riddle.Abacus
{
    /// <summary>
    /// 控制当前操作的行（line）与层（frame）
    /// </summary>
    public sealed class FrameControl : MonoBehaviour
    {
        [Header("Frame引用")]
        public GameObject frame0;   // 上层框
        public GameObject frame1;   // 下层框

        [Header("当前行 (0~8)")]
        [SerializeField] private int lineNum = 0;
        
        private bool inputLocked = false;

        public int CurrentLine => lineNum;
        public int CurrentFrame => frame0.activeSelf ? 0 : 1;

        private Vector3 frame0StartPos;
        private Vector3 frame1StartPos;

        private void Start()
        {
            frame0StartPos = frame0.transform.position;
            frame1StartPos = frame1.transform.position;

            frame0.SetActive(true);
            frame1.SetActive(false);
        }

        private void Update()
        {
            if (inputLocked) return;
            
            HandleFrameSwitch();
            HandleLineMove();
        }

        private void HandleFrameSwitch()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                frame0.SetActive(true);
                frame1.SetActive(false);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                frame0.SetActive(false);
                frame1.SetActive(true);
            }
        }

        private void HandleLineMove()
        {
            bool moved = false;

            if (Input.GetKeyDown(KeyCode.D))
            {
                lineNum = (lineNum + 1) % 9;
                moved = true;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                lineNum = (lineNum + 8) % 9; // 防止负数
                moved = true;
            }

            if (moved)
            {
                Vector3 step = new Vector3(0.3f, 0, 0);

                if (frame0.activeSelf)
                    frame0.transform.position = frame0StartPos + step * lineNum;
                else if (frame1.activeSelf)
                    frame1.transform.position = frame1StartPos + step * lineNum;
            }
        }
        
        // =====================================================
        // 外部接口
        // =====================================================

        /// <summary>锁定输入（禁用玩家控制）</summary>
        public void LockInput()
        {
            inputLocked = true;
        }

        /// <summary>解锁输入（恢复控制）</summary>
        public void UnlockInput()
        {
            inputLocked = false;
        }
    }
}
