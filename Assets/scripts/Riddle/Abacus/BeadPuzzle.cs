using System.Collections;
using Audio;
using UI;
using UnityEngine;

#pragma warning disable CS0414 // 字段已被赋值，但它的值从未被使用

namespace Riddle.Abacus
{
    public sealed class BeadPuzzle : MonoBehaviour
    {
        private string puzzleId;  
        
        private string correctCode;
        
        [Header("背景图（不同算盘不同）")]
        public SpriteRenderer backgroundRenderer;
        public Sprite bg1;
        public Sprite bg2;
        public Sprite bg3;
        public Sprite bg4;

        private Vector3 returnPosition;
        private int frameNum;
        private readonly int[] lineNum = new int[2];
        private readonly int[,] beadRecord = new int[9, 2];
        private GameObject[] upperObjects;
        private GameObject[] lowerObjects;
        public int[,] clickCount = new int[10, 3];
        private bool isSolved = false;

        private void Start()
        {
            // === 从缓冲器读取当前算盘 ID ===
            string id = AbacusBuffer.currentId;
            puzzleId = id;
            returnPosition = AbacusBuffer.returnPosition;

            // === 根据 ID 设置正确答案 ===
            correctCode = id switch
            {
                "1" => "000000000-000000013",
                "2" => "000000000-000000060",
                "3" => "000000000-000000010",
                "4" => "000000000-000001996",
                _   => "000000000-000000013"
            };

            // === 根据 ID 切换背景 ===
            if (backgroundRenderer)
            {
                backgroundRenderer.sprite = id switch
                {
                    "1" => bg1,
                    "2" => bg2,
                    "3" => bg3,
                    "4" => bg4,
                    _ => bg1
                };
            }

            // === 初始化逻辑 ===
            frameNum = 0;
            for (int i = 0; i < clickCount.GetLength(0); i++)
            for (int j = 0; j < clickCount.GetLength(1); j++)
                clickCount[i, j] = 0;

            if (InfoDialogUI.Instance)
            {
                InfoDialogUI.Instance.ShowMessage("（桌子上好像刻着什么东西，要试试拨算盘吗。）");
                InfoDialogUI.Instance.ShowDefaultCharacter();
            }
        }

        private void Update()
        {
            var ctrl = AbacusControl.Instance;
            if (!ctrl) return;
            
            int currentLine = ctrl.CurrentLine;
            int currentFrame = ctrl.CurrentFrame;
            
            lineFind(currentLine);

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (isSolved) return;
                clickCount[currentLine, currentFrame]++;
                HandleClick(currentLine, currentFrame);
                AudioClipHelper.Instance.Play_SuanPan();
            }

            if (!isSolved && CheckAnswer())
            {
                isSolved = true;
                InfoDialogUI.Instance.ShowMessage("答案正确！算盘已被锁定。");
                ctrl.LockInput(); 

                PlayerPrefs.SetInt($"AbacusSolved_{puzzleId}", 1);
                PlayerPrefs.Save();

                StartCoroutine(FinishPuzzle());
            }
        }

        private IEnumerator FinishPuzzle()
        {
            yield return new WaitForSeconds(1f);

            var back = FindObjectOfType<AbacusBack>();
            if (back)
                back.ReturnToClassroom(returnPosition); 
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene("C1CJC");
        }

        private bool CheckAnswer()
        {
            // 把每一列的数值转成“数字”
            string abacusValue = "";
            for (int i = 0; i < 9; i++)
            {
                int upper = beadRecord[i, 0]; // 0~2
                int lower = beadRecord[i, 1]; // 0~5

                int columnValue = upper * 5 + lower;
                abacusValue += columnValue.ToString();
            }

            // 去掉前导 0（防止 000001996 变成 1996）
            abacusValue = abacusValue.TrimStart('0');

            if (!int.TryParse(abacusValue, out int value))
                return false;

            int target = puzzleId switch
            {
                "1" => 13,
                "2" => 60,
                "3" => 10,
                "4" => 1996,
                _   => 13
            };

            return value == target;
        }

        private void lineFind(int lineIndex)
        {
            Transform lineTransform = transform.Find("Line_" + lineIndex);
            if (!lineTransform) return;
            Transform upperGroup = lineTransform.Find("upper");
            Transform lowerGroup = lineTransform.Find("lower");
            if (!upperGroup || !lowerGroup) return;

            upperObjects = new GameObject[2];
            lowerObjects = new GameObject[5];
            for (int i = 0; i < 2; i++) upperObjects[i] = upperGroup.Find(i.ToString()).gameObject;
            for (int i = 0; i < 5; i++) lowerObjects[i] = lowerGroup.Find(i.ToString()).gameObject;
        }

        private void HandleClick(int lineIndex, int frameIndex)
        {
            if (frameIndex == 0)
            {
                clickCount[lineIndex, frameIndex] %= 3;
                beadRecord[lineIndex, frameIndex] = clickCount[lineIndex, frameIndex];
                switch (clickCount[lineIndex, frameIndex])
                {
                    case 1: MoveObjectDown(upperObjects[0]); break;
                    case 2: MoveObjectDown(upperObjects[1]); break;
                    case 0: ResetUpperObjects(); break;
                }
            }
            else
            {
                clickCount[lineIndex, frameIndex] %= 6;
                beadRecord[lineIndex, frameIndex] = clickCount[lineIndex, frameIndex];
                switch (clickCount[lineIndex, frameIndex])
                {
                    case 1: MoveObjectUp(lowerObjects[0]); break;
                    case 2: MoveObjectUp(lowerObjects[1]); break;
                    case 3: MoveObjectUp(lowerObjects[2]); break;
                    case 4: MoveObjectUp(lowerObjects[3]); break;
                    case 5: MoveObjectUp(lowerObjects[4]); break;
                    case 0: ResetLowerObjects(); break;
                }
            }
        }

        private void MoveObjectDown(GameObject obj)
        {
            var pos = obj.transform.position;
            pos.y -= 0.2f;
            obj.transform.position = pos;
        }

        private void MoveObjectUp(GameObject obj)
        {
            var pos = obj.transform.position;
            pos.y += 0.2f;
            obj.transform.position = pos;
        }

        private void ResetUpperObjects()
        {
            foreach (var obj in upperObjects)
            {
                var pos = obj.transform.position;
                pos.y += 0.2f;
                obj.transform.position = pos;
            }
        }

        private void ResetLowerObjects()
        {
            foreach (var obj in lowerObjects)
            {
                var pos = obj.transform.position;
                pos.y -= 0.2f;
                obj.transform.position = pos;
            }
        }
    }
}
