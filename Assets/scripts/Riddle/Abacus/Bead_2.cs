using System.Collections;
using Audio;
using UI;
using UnityEngine;
#pragma warning disable CS0414 // �ֶ��ѱ���ֵ��������ֵ��δ��ʹ��

namespace Riddle.Abacus
{
    public class Bead_2 : MonoBehaviour
    {
        #region Components
        public Animator anim { get; private set; }

        #endregion

        private int frameNum;
        private int[] lineNum = new int[2];

        //������λ�ü�¼������Ƿ�����ȷλ�ý���
        private int[,] beadRecord = new int[9, 2];
        [SerializeField] private bool isSolved = false;

        private GameObject[] upperObjects;
        private GameObject[] lowerObjects;
        public int[,] clickCount = new int[10, 3];


        protected virtual void Start()
        {
            anim = GetComponentInChildren<Animator>();

            frameNum = 0;

            // ��ʼ������Ԫ��Ϊ0
            for (int i = 0; i < clickCount.GetLength(0); i++)
            {
                for (int j = 0; j < clickCount.GetLength(1); j++)
                {
                    clickCount[i, j] = 0;
                }
            }
            
            InfoDialogUI.Instance.ShowMessage("�������Ϻ������ʲô������Ҫ���Բ������𡣣�");
            InfoDialogUI.Instance.ShowDefaultCharacter();


        }

        private void lineFind()
        {
            // ��ȡ��Ӧ��Line����
            Transform lineTransform = transform.Find("Line_" + lineNum[frameNum]);
            if (lineTransform != null)
            {
                Transform upperGroup = lineTransform.Find("upper");
                Transform lowerGroup = lineTransform.Find("lower");

                if (upperGroup != null && lowerGroup != null)
                {
                    upperObjects = new GameObject[2];
                    lowerObjects = new GameObject[5];

                    for (int i = 0; i < 2; i++)
                    {
                        upperObjects[i] = upperGroup.Find(i.ToString()).gameObject;
                    }

                    for (int i = 0; i < 5; i++)
                    {
                        lowerObjects[i] = lowerGroup.Find(i.ToString()).gameObject;
                    }
                }
                else
                {
                    Debug.LogError("Upper or Lower group not found in Line_" + lineNum[frameNum]);
                }
            }
            else
            {
                Debug.LogError("Line_" + lineNum[frameNum] + " not found.");
            }
        }

        protected virtual void Update()
        {
            lineNumCount();
            frameNumCount();
            lineFind();

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (isSolved)
                {
                    return;
                }
                clickCount[lineNum[frameNum], frameNum]++;
                HandleClick();
                AudioClipHelper.Instance.Play_SuanPan();
            }

            if (beadRecord != null &&
                beadRecord[0,0]==0 && beadRecord[1,0]==0 && beadRecord[2,0]==0 &&
                beadRecord[3,0]==0 && beadRecord[4,0]==0 && beadRecord[5,0]==0 &&
                beadRecord[6,0]==0 && beadRecord[7,0]==1 && beadRecord[8,0]==0 &&
                beadRecord[0,1]==0 && beadRecord[1,1]==0 && beadRecord[2,1]==0 &&
                beadRecord[3,1]==0 && beadRecord[4,1]==0 && beadRecord[5,1]==0 &&
                beadRecord[6,1]==0 && beadRecord[7,1]==1 && beadRecord[8,1]==0)
            {
                if (!isSolved)  // ֻ�ڵ�һ�ν⿪ʱ����
                {
                    isSolved = true;
                    InfoDialogUI.Instance.ShowMessage("����ȷ�������ѱ�������");
                    var frameCtrl = FindObjectsOfType<FrameControl>();
                    foreach (var f in frameCtrl)
                    {
                        f.LockInput();
                    }

                    // ====== ������� ======
                    PlayerPrefs.SetInt("AbacusSolved2", 1);  // 1 ��ʾ�ѽ⿪
                    PlayerPrefs.Save(); // ����д��Ӳ��

                    // ====== �ӳ�ִ�л����� ======
                    StartCoroutine(WaitAndLoadScene());
                }
            }
            else
            {
                isSolved = false;
            }

        }

        private IEnumerator WaitAndLoadScene()
        {
            yield return new WaitForSeconds(1f); // �ȴ� 1 ��

            var fade = FindObjectOfType<Scene.SceneFadeEffect>();
            if (fade)
            {
                fade.FadeOutAndLoad("C1CJC", 0.5f, 1f); // �ĳ���Ҫȥ�ĳ�����
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("C1CJC");
            }
        }
        private void frameNumCount()
        {
            if (Input.GetKeyUp(KeyCode.W))
                frameNum = 0;
            if (Input.GetKeyUp(KeyCode.S))
                frameNum = 1;
        }

        private void lineNumCount()
        {
            if (Input.GetKeyUp(KeyCode.D))
                lineNum[frameNum] = (++lineNum[frameNum]) % 9;
            if (Input.GetKeyUp(KeyCode.A))
                lineNum[frameNum] = (lineNum[frameNum] + 8) % 9;
        }

        private void HandleClick()
        {
            if (frameNum == 0)
            {
                clickCount[lineNum[frameNum], frameNum] %= 3;
                beadRecord[lineNum[frameNum], frameNum] = clickCount[lineNum[frameNum], frameNum];

                switch (clickCount[lineNum[frameNum], frameNum])
                {
                    case 1:
                        MoveObjectDown(upperObjects[0]);
                        break;
                    case 2:
                        MoveObjectDown(upperObjects[1]);
                        break;
                    case 0:
                        ResetUpperObjects();
                        break;
                }
            }
            else if (frameNum == 1)
            {
                clickCount[lineNum[frameNum], frameNum] %= 6;
                beadRecord[lineNum[frameNum], frameNum] = clickCount[lineNum[frameNum], frameNum];

                switch (clickCount[lineNum[frameNum], frameNum])
                {
                    case 1:
                        MoveObjectUp(lowerObjects[0]);
                        break;
                    case 2:
                        MoveObjectUp(lowerObjects[1]);
                        break;
                    case 3:
                        MoveObjectUp(lowerObjects[2]);
                        break;
                    case 4:
                        MoveObjectUp(lowerObjects[3]);
                        break;
                    case 5:
                        MoveObjectUp(lowerObjects[4]);
                        break;
                    case 0:
                        ResetLowerObjects();
                        break;
                }
            }
        }

        private void MoveObjectDown(GameObject obj)
        {
            Vector3 position = obj.transform.position;
            position.y -= 0.2f;
            obj.transform.position = position;
        }

        private void MoveObjectUp(GameObject obj)
        {
            Vector3 position = obj.transform.position;
            position.y += 0.2f;
            obj.transform.position = position;
        }

        private void ResetUpperObjects()
        {
            foreach (GameObject obj in upperObjects)
            {
                Vector3 position = obj.transform.position;
                position.y += 0.2f;
                obj.transform.position = position;
            }
        }

        private void ResetLowerObjects()
        {
            foreach (GameObject obj in lowerObjects)
            {
                Vector3 position = obj.transform.position;
                position.y -= 0.2f;
                obj.transform.position = position;
            }
        }
    }
}
