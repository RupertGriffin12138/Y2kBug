using Audio;
using System.Collections;
using Mobile;
using UI;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable CS0414 // 字段已被赋值，但它的值从未被使用
using UnityEngine.SceneManagement;
#pragma warning disable CS0414 // ????????????????????????????????????????δ??????????

namespace Riddle.Blackboard
{
    public class SwitchCircleController : MonoBehaviour
    {
        [SerializeField] private bool isAnswer = false;
        [SerializeField] private int count_heng = 0; // 横向计数
        [SerializeField] private int count_zong = 0;  // 纵向计数
        [SerializeField] private int count_num = 0;   // 综合计数
        [SerializeField] private float moveDistance = 1f; // 移动距离
        
        [SerializeField] private Button btn1;
        [SerializeField] private Button btn2;
        [SerializeField] private Button btn3;
        [SerializeField] private Button btn4;
        [SerializeField] private Button btn5;
        [SerializeField] private Button btn6;
        [SerializeField] private Button btn7;
        [SerializeField] private Button btn8;
        [SerializeField] private Button btn9;

        private int progress = 0;

        [SerializeField] private SpriteRenderer blackboardFade;
        [SerializeField] private float fadeDuration = 2.0f;

        private Material fadeMaterial;
        private static readonly int FadeProgress = Shader.PropertyToID("_FadeProgress");

        private Vector3 initialPosition; // 初始位置

        // 二维数组用于映射 count_heng ???count_zong ???count_num
        private readonly int[,] circleNum = {
            { 0, 3, 6 },
            { 1, 4, 7 },
            { 2, 5, 8 }
        };

        private readonly int[] answerNum = { 5, 1, 3, 9, 9, 6, 4, 5, 8 };

        public GameObject block; // Block 对象
        
        private float xInput;
        private float yInput;
        private float moveTimer = 0f;
        private const float moveCooldown = 0.25f; // 防止连续移动太快

        private int previousCountNum = -1; // 上一???count_num 的???

        private void Start()
        {
            InfoDialogUI.Instance.ShowMessage("（这是，数独，让我想想）");
            InfoDialogUI.Instance.EnableCharacterBackground("姜宁");

            // 记录初始位置
            initialPosition = transform.position;
            // 初始???count_num
            UpdateCountNum();
            // 初始???Answer 组显示状???
            InitializeAnswers();


            // 确保SpriteRenderer存在
            if (blackboardFade == null)
                blackboardFade = GetComponent<SpriteRenderer>();

            // 创建材质实例
            fadeMaterial = new Material(blackboardFade.material);
            blackboardFade.material = fadeMaterial;

            // 初始透明度设???
            SetFadeProgress(0f);
            
            btn1.onClick.AddListener(() =>{
                AudioClipHelper.Instance.Play_ChalkWriting();
                ShowAnswer(1);
            });
            btn2.onClick.AddListener(() =>{
                AudioClipHelper.Instance.Play_ChalkWriting();
                ShowAnswer(2);
            });
            btn3.onClick.AddListener(() =>{
                AudioClipHelper.Instance.Play_ChalkWriting();
                ShowAnswer(3);
            });
            btn4.onClick.AddListener(() =>{
                AudioClipHelper.Instance.Play_ChalkWriting();
                ShowAnswer(4);
            });
            btn5.onClick.AddListener(() =>{
                AudioClipHelper.Instance.Play_ChalkWriting();
                ShowAnswer(5);
            });
            btn6.onClick.AddListener(() =>{
                AudioClipHelper.Instance.Play_ChalkWriting();
                ShowAnswer(6);
            });
            btn7.onClick.AddListener(() =>{
                AudioClipHelper.Instance.Play_ChalkWriting();
                ShowAnswer(7);
            });
            btn8.onClick.AddListener(() =>{
                AudioClipHelper.Instance.Play_ChalkWriting();
                ShowAnswer(8);
            });
            btn9.onClick.AddListener(() =>{
                AudioClipHelper.Instance.Play_ChalkWriting();
                ShowAnswer(9);
            });
            
    }

        private void Update()
        {
            moveTimer -= Time.deltaTime;
            
#if UNITY_ANDROID || UNITY_IOS
            if (GlobalInput.joystick)
            {
                xInput = GlobalInput.joystick.Horizontal;
                yInput = GlobalInput.joystick.Vertical;

                if (Mathf.Abs(xInput) < 0.3f) xInput = 0f;
                if (Mathf.Abs(yInput) < 0.3f) yInput = 0f;

                if (moveTimer <= 0f)
                {
                    if (xInput > 0.5f) { MoveRight(); moveTimer = moveCooldown; }
                    else if (xInput < -0.5f) { MoveLeft(); moveTimer = moveCooldown; }
                    else if (yInput > 0.5f) { MoveUp(); moveTimer = moveCooldown; }
                    else if (yInput < -0.5f) { MoveDown(); moveTimer = moveCooldown; }
                }
            }
#else
            // PC 键盘
            if (Input.GetKeyDown(KeyCode.D)) MoveRight();
            else if (Input.GetKeyDown(KeyCode.A)) MoveLeft();
            else if (Input.GetKeyDown(KeyCode.W)) MoveUp();
            else if (Input.GetKeyDown(KeyCode.S)) MoveDown();

            if (Input.GetKeyDown(KeyCode.Alpha1)) PlayAndShow(1);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) PlayAndShow(2);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) PlayAndShow(3);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) PlayAndShow(4);
            else if (Input.GetKeyDown(KeyCode.Alpha5)) PlayAndShow(5);
            else if (Input.GetKeyDown(KeyCode.Alpha6)) PlayAndShow(6);
            else if (Input.GetKeyDown(KeyCode.Alpha7)) PlayAndShow(7);
            else if (Input.GetKeyDown(KeyCode.Alpha8)) PlayAndShow(8);
            else if (Input.GetKeyDown(KeyCode.Alpha9)) PlayAndShow(9);
#endif
        }

        private void PlayAndShow(int num)
        {
            AudioClipHelper.Instance.Play_ChalkWriting();
            ShowAnswer(num);
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
                count_heng = (count_heng - 1 + 3) % 3; // 使用模运算确保结果非???
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
                count_zong = (count_zong - 1 + 3) % 3; // 使用模运算确保结果非???
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

        private void UpdateCountNum()
        {
            if (circleNum != null) count_num = circleNum[count_heng, count_zong];
        }

        private void InitializeAnswers()
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

        private void ShowAnswer(int number)
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

        private void CheckAllAnswers()
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
                InfoDialogUI.Instance.ShowMessage("（数字都...掉下来了...真的，这个地方...一定有什么出问题了，是“它”！)\r\n");
                InfoDialogUI.Instance.EnableCharacterBackground("姜宁");
                //Debug.Log("All right");
                Fade();
            }
        }

        private GameObject GetAnswerGroup(int index)
        {
            Transform blockChild = block.transform.GetChild(index);
            if (blockChild != null)
            {
                return blockChild.Find("Answer")?.gameObject;
            }
            return null;
        }

        private void LateUpdate()
        {
            // ???count_heng == 0 时，横坐标回到初始位???
            if (count_heng == 0)
            {
                transform.position = new Vector3(initialPosition.x, transform.position.y, transform.position.z);
            }

            // ???count_zong == 0 时，纵坐标回到初始位???
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

            SetFadeProgress(1f); // 确保最终完全显???

            SetFadeProgress(1f); // ??????????????????????????
            progress = 4;
            PlayerPrefs.SetInt("BoardKey_Prefab", progress);

            SceneManager.LoadScene("C1CJB");
        }

        private void SetFadeProgress(float progress)
        {
            if (fadeMaterial)
                fadeMaterial.SetFloat(FadeProgress, progress);
        }

        // 公开方法，可从外部调???
        public void StartFade(float duration = 0f)
        {
            if (duration > 0)
                fadeDuration = duration;

            Fade();
        }

        void OnDestroy()
        {
            // 清理创建的材质实???
            if (fadeMaterial != null)
                DestroyImmediate(fadeMaterial);
        }
    }
}


