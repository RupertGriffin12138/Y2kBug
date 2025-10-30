using Audio;
using System.Collections;
using UnityEngine;
#pragma warning disable CS0414 // �ֶ��ѱ���ֵ��������ֵ��δ��ʹ��
using UnityEngine.SceneManagement;
#pragma warning disable CS0414 // �1�7�0�6�1�7�1�7�0�1�1�7�1�7�1�7�0�5�1�7�1�7�1�7�1�7�1�7�1�7�1�7�1�7�0�5�1�7�1�7�Ą1�7�1�7�0�0�1�7�1�7

namespace Riddle.Blackboard
{
    public class SwitchCircleController : MonoBehaviour
    {
        [SerializeField] private bool isAnswer = false;
        [SerializeField] private int count_heng = 0; // �������
        [SerializeField] private int count_zong = 0;  // �������
        [SerializeField] private int count_num = 0;   // �ۺϼ���
        [SerializeField] private float moveDistance = 1f; // �ƶ�����

        private int progress = 0;

        [SerializeField] private SpriteRenderer blackboardFade;
        [SerializeField] private float fadeDuration = 2.0f;

        private Material fadeMaterial;
        private static readonly int FadeProgress = Shader.PropertyToID("_FadeProgress");

        private Vector3 initialPosition; // ��ʼλ��

        // ��ά��������ӳ�� count_heng �1�7?count_zong �1�7?count_num
        private int[,] circleNum = {
            { 0, 3, 6 },
            { 1, 4, 7 },
            { 2, 5, 8 }
        };

        private int[] answerNum = { 5, 1, 3, 9, 9, 6, 4, 5, 8 };

        public GameObject block; // Block ����

        private int previousCountNum = -1; // ��һ�1�7?count_num �Ą1�7?

        void Start()
        {
            // ��¼��ʼλ��
            initialPosition = transform.position;
            // ��ʼ�1�7?count_num
            UpdateCountNum();
            // ��ʼ�1�7?Answer ����ʾ״�1�7?
            InitializeAnswers();


            // ȷ��SpriteRenderer����
            if (blackboardFade == null)
                blackboardFade = GetComponent<SpriteRenderer>();

            // ��������ʵ��
            fadeMaterial = new Material(blackboardFade.material);
            blackboardFade.material = fadeMaterial;

            // ��ʼ͸������1�7?
            SetFadeProgress(0f);
        }

        void Update()
        {
            // ����������
            getNumandKeyDown();

            // �����ǰ�����1�7?
            //Debug.Log($"Count Heng: {count_heng}, Count Zong: {count_zong}, Count Num: {count_num}");
        }

        private void getNumandKeyDown()
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                MoveRight();
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                MoveLeft();
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                MoveUp();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                MoveDown();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                AudioClipHelper.Instance.Play_ChalkWriting();
                ShowAnswer(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                AudioClipHelper.Instance.Play_ChalkWriting();
                ShowAnswer(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                AudioClipHelper.Instance.Play_ChalkWriting();
                ShowAnswer(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                AudioClipHelper.Instance.Play_ChalkWriting();
                ShowAnswer(4);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                AudioClipHelper.Instance.Play_ChalkWriting();
                ShowAnswer(5);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                AudioClipHelper.Instance.Play_ChalkWriting();
                ShowAnswer(6);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                AudioClipHelper.Instance.Play_ChalkWriting();
                ShowAnswer(7);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                AudioClipHelper.Instance.Play_ChalkWriting();
                ShowAnswer(8);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                AudioClipHelper.Instance.Play_ChalkWriting();
                ShowAnswer(9);
            }
        }

        #region  Move Control
        void MoveRight()
        {
            transform.position += Vector3.right * moveDistance;
            count_heng = (count_heng + 1) % 3;
            UpdateCountNum();
        }

        void MoveLeft()
        {
            if (!(count_zong == 0 && count_heng == 0))
            {
                transform.position += Vector3.left * moveDistance;
                count_heng = (count_heng - 1 + 3) % 3; // ʹ��ģ����ȷ������Ǆ1�7?
                UpdateCountNum();
            }
            else
            {
                Debug.Log("Cannot move left when count_zong == 0 && count_heng == 0");
            }
        }

        void MoveUp()
        {
            if (!(count_zong == 0 && count_heng == 0))
            {
                transform.position += Vector3.up * moveDistance;
                count_zong = (count_zong - 1 + 3) % 3; // ʹ��ģ����ȷ������Ǆ1�7?
                UpdateCountNum();
            }
            else
            {
                Debug.Log("Cannot move up when count_zong == 0 && count_heng == 0");
            }
        }

        void MoveDown()
        {
            transform.position += Vector3.down * moveDistance;
            count_zong = (count_zong + 1) % 3;
            UpdateCountNum();
        }

        #endregion

        void UpdateCountNum()
        {
            if (circleNum != null) count_num = circleNum[count_heng, count_zong];
        }

        void InitializeAnswers()
        {
            for (int i = 0; i < 9; i++)
            {
                GameObject answerGroup = GetAnswerGroup(i);
                if (answerGroup != null)
                {
                    foreach (Transform child in answerGroup.transform)
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }
        }

        void ShowAnswer(int number)
        {
            GameObject answerGroup = GetAnswerGroup(count_num);
            if (answerGroup != null)
            {
                foreach (Transform child in answerGroup.transform)
                {
                    if (child.name == number.ToString())
                        child.gameObject.SetActive(true);
                    else
                        child.gameObject.SetActive(false);

                }
            }

            CheckAllAnswers();
        }

        void CheckAllAnswers()
        {
            isAnswer = true;
            for (int i = 0; i < 9; i++)
            {
                GameObject answerGroup = GetAnswerGroup(i);
                if (answerGroup != null)
                {
                    Transform correctAnswer = answerGroup.transform.Find(answerNum[i].ToString());
                    if (correctAnswer == null || !correctAnswer.gameObject.activeSelf)
                    {
                        isAnswer = false;
                        break;
                    }
                }
                else
                {
                    isAnswer = false;
                    break;
                }
            }

            if (isAnswer)
            {
                //Debug.Log("All right");
                Fade();
            }
        }

        GameObject GetAnswerGroup(int index)
        {
            Transform blockChild = block.transform.GetChild(index);
            if (blockChild != null)
            {
                return blockChild.Find("Answer")?.gameObject;
            }
            return null;
        }

        void LateUpdate()
        {
            // �1�7?count_heng == 0 ʱ��������ص���ʼλ�1�7?
            if (count_heng == 0)
            {
                transform.position = new Vector3(initialPosition.x, transform.position.y, transform.position.z);
            }

            // �1�7?count_zong == 0 ʱ��������ص���ʼλ�1�7?
            if (count_zong == 0)
            {
                transform.position = new Vector3(transform.position.x, initialPosition.y, transform.position.z);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene("C1CJB");
            }
        }





        ///
        public void Fade()
        {
            StartCoroutine(FadeCoroutine());
        }

        private IEnumerator FadeCoroutine()
        {
            float elapsedTime = 0f;
            SetFadeProgress(0f);

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / fadeDuration);
                SetFadeProgress(progress);
                yield return null;
            }

            SetFadeProgress(1f); // ȷ��������ȫ�Ԅ1�7?

            SetFadeProgress(1f); // �0�2�1�7�1�7�1�7�1�7�1�7�1�7�1�7�1�7�0�0�1�7�1�7�0�5
            progress = 4;
            PlayerPrefs.SetInt("BoardKey_Prefab", progress);
        }

        private void SetFadeProgress(float progress)
        {
            if (fadeMaterial != null)
                fadeMaterial.SetFloat(FadeProgress, progress);
        }

        // �����������ɴ��ⲿ���1�7?
        public void StartFade(float duration = 0f)
        {
            if (duration > 0)
                fadeDuration = duration;

            Fade();
        }

        void OnDestroy()
        {
            // �������Ĳ���ʵ�1�7?
            if (fadeMaterial != null)
                DestroyImmediate(fadeMaterial);
        }
    }
}


