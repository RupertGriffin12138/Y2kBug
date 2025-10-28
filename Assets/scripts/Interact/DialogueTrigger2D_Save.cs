using System.Collections;
using System.Collections.Generic;
using Characters.Player;
using Save;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Interact
{
    /// <summary>
    /// 2D 对话触发器（自动触发 + 多说话人 + 背景切换 + 打字机）
    /// 进入触发区即开始，空格推进；每句包含 speaker 与 content；结束写入存档；是否销毁由 destroyAfterFinish 决定。
    /// 依赖：InfoDialogUI(Instance/StartDialogue/EndDialogue/SetNameText/ShowMessage/textBoxText/EnableCharacterBackground/ShowArrow/HideArrow)、SaveManager、SaveData
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DialogueTrigger2D_Save : MonoBehaviour
    {
        [Header("唯一ID（用于存档判重）")]
        public string dialogueId = "dlg_001";

        [System.Serializable]
        public class DialogueLine
        {
            public string speaker;
            [TextArea(2, 3)] public string content;
        }

        [Header("对话内容（每句包含人物+内容）")]
        public List<DialogueLine> lines = new()
        {
            new DialogueLine { speaker = "旁白", content = "操场边上的男孩来回踱步……" },
            new DialogueLine { speaker = "姜宁", content = "十。" },
            new DialogueLine { speaker = "祝榆", content = "别急。" }
        };

        [Header("过滤")]
        public string playerTag = "Player";

        [Header("行为")]
        public bool blockWhenPaused = true;
        public bool destroyAfterFinish = true;

        [Header("打字机参数")]
        [Tooltip("每个字符的延时（秒）")]
        public float typeCharDelay = 0.05f;
        
        private const KeyCode nextKey = KeyCode.E;

        private bool talking;
        private int idx;
        private SaveData save;

        private Player player;

        // 打字机状态
        private Coroutine typeRoutine;
        private bool lineFullyShown;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void Start()
        {
            save = SaveManager.LoadOrDefault("Town");
            if (save.HasSeenDialogue(dialogueId))
            {
                Destroy(gameObject);
            }
            
            InitArrowBtn();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            if (save != null && save.HasSeenDialogue(dialogueId)) return;
            if (talking) return;

            BeginTalk();
        }

        private void Update()
        {
            if (blockWhenPaused && Time.timeScale == 0f) return;
            if (Input.GetKeyDown(nextKey))
            {
                HandleNext();
            }
        }
        
        /// <summary>
        /// 处理 推进对话
        /// </summary>
        private void HandleNext()
        {
            if (!talking) return;

            if (!lineFullyShown)
            {
                // 当前句还在打字 → 补全
                ShowLineInstant();
            }
            else
            {
                if (SceneManager.GetActiveScene().name == "C1S3 guard")
                    ControlGif();
                NextLine();
            }
        }
        
        public void InitArrowBtn()
        {
            if (!InfoDialogUI.Instance.arrowImage.TryGetComponent<Button>(out var image))
            {
                Button btn = InfoDialogUI.Instance.arrowImage.AddComponent<Button>();
                btn.onClick.AddListener(HandleNext);
            }
        }

        private void BeginTalk()
        {
            if (lines == null || lines.Count == 0) return;

            talking = true;
            idx = 0;

            // === 禁止玩家移动 ===
            if (player == null)
                player = FindObjectOfType<Player>();
            if (player != null)
                player.LockControl(); 
            
            

            if (InfoDialogUI.Instance)
            {
                InfoDialogUI.Instance.StartDialogue();
            }

            ShowCurrentLineTyped();
        }

        private void NextLine()
        {
            idx++;
            if (idx < lines.Count)
            {
                ShowCurrentLineTyped();
            }
            else
            {
                // 仅当不是教学场景时才立即 EndTalk()
                if (SceneManager.GetActiveScene().name == "C1S1 campus")
                {
                    // 显示教学提示（不要立刻 EndTalk）
                    if (InfoDialogUI.Instance)
                        InfoDialogUI.Instance.ShowMessage("按 “A” 或 “D” 进行移动");

                    // 启动监听协程
                    StartCoroutine(WaitForMoveInputToHideHint());
                }
                else
                {
                    // 正常对白 → 立即结束
                    EndTalk();
                }
            }
        }

        private void ShowCurrentLineTyped()
        {
            if (!InfoDialogUI.Instance) return;

            // 停掉上一次的打字协程
            if (typeRoutine != null)
            {
                StopCoroutine(typeRoutine);
                typeRoutine = null;
            }

            var line = lines[idx];

            // 名字框：旁白不显示名字
            if (string.Equals(line.speaker, "旁白"))
                InfoDialogUI.Instance.SetNameText("");
            else
                InfoDialogUI.Instance.SetNameText(line.speaker);

            // 按人物名称切换背景
            InfoDialogUI.Instance.EnableCharacterBackground(line.speaker);

            // 清空并开始打字
            InfoDialogUI.Instance.textBoxText.text = "";
            lineFullyShown = false;

            // 正确：直接调用方法
            InfoDialogUI.Instance.HideArrow();

            typeRoutine = StartCoroutine(Typewriter(line.content));
        }

        private IEnumerator Typewriter(string content)
        {
            // 逐字输出
            foreach (char c in content)
            {
                InfoDialogUI.Instance.textBoxText.text += c;
                yield return new WaitForSeconds(typeCharDelay);

                // 若在打字中按键，交由 Update 的逻辑立即补全
                if (Input.GetKeyDown(nextKey_1) || Input.GetKeyDown(nextKey_2))
                {
                    // 立即补全
                    InfoDialogUI.Instance.textBoxText.text = content;
                    break;
                }
            }

            // 打字完成
            lineFullyShown = true;

            // 正确：直接调用方法
            InfoDialogUI.Instance.ShowArrow();

            typeRoutine = null;
        }

        private void ShowLineInstant()
        {
            if (!InfoDialogUI.Instance) return;
            if (idx < 0 || idx >= lines.Count) return;

            if (typeRoutine != null)
            {
                StopCoroutine(typeRoutine);
                typeRoutine = null;
            }

            InfoDialogUI.Instance.textBoxText.text = lines[idx].content;
            lineFullyShown = true;

            // 正确：直接调用方法
            InfoDialogUI.Instance.ShowArrow();
        }

        private void EndTalk()
        {
            talking = false;

            // === 恢复玩家移动 ===
            if (!player)
                player = FindObjectOfType<Player>();
            if (player)
                player.isBusy = false; 

            // 存档标记
            if (save == null) save = SaveManager.LoadOrDefault("Town");
            if (save.TryMarkDialogueSeen(dialogueId))
            {
                SaveManager.Save(save);
            }

            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.EndDialogue();

            if (destroyAfterFinish)
                Destroy(gameObject);
            // else 保留在场景中（再次进入若 save 判重仍为未看过则可再次触发）
        }

        // 选中时可视化触发范围
        void OnDrawGizmosSelected()
        {
            var col = GetComponent<Collider2D>();
            if (!col) return;

            var prev = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1f, 1f, 0f, 0.25f);

            if (col is BoxCollider2D b) Gizmos.DrawCube((Vector3)b.offset, (Vector3)b.size);
            if (col is CircleCollider2D c) Gizmos.DrawSphere((Vector3)c.offset, c.radius);

            Gizmos.matrix = prev;
        }

        private void ControlGif()
        {
            switch (idx)
            {
                case 1:
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/mouth1",new Vector2(540,370),new Vector2(545,357));
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                case 7:
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/heart1",new Vector2(-333,0),new Vector2(266,361));
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                case 9:
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/eye1",new Vector2(-345,209),new Vector2(457,262));
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                case 16:
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/mouth2_2",new Vector2(108,155),new Vector2(504,311));
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                case 20:
                    InfoDialogUI.Instance.HideGif();
                    InfoDialogUI.Instance.SpawnMultiple(true);
                    break;
                case 21:
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                case 29:
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/bug",new Vector2(1,1),new Vector2(1,1),true);
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
            }
        }
        // <summary>
        /// 等待玩家按下移动键后隐藏提示
        /// </summary>
        private IEnumerator WaitForMoveInputToHideHint()
        {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D));

            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.ShowMessage(""); // 清空提示

            // 现在才真正结束对白
            EndTalk();
        }
    }
}
