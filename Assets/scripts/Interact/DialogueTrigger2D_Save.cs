using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Characters.PLayer_25D;
using Characters.Player;
using Save;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        [Header("对话内容")]
        public List<DialogueLine> lines = new()
        {
            new DialogueLine { speaker = "旁白", content = "操场边上的男孩来回踱步……" },
            new DialogueLine { speaker = "姜宁", content = "十。" },
            new DialogueLine { speaker = "祝榆", content = "别急。" }
        };

        [Header("过滤")]
        public string playerTag = "Player";

        [Header("行为")]
        public bool destroyAfterFinish = true;
        
        [Header("控制选项")]
        [Tooltip("是否在对话期间锁住玩家的移动和操作（默认：是）")]
        public bool lockPlayerDuringDialogue = true;
        [Tooltip("是否允许对话在退出触发区后重置（再次进入可从头开始）")]
        public bool resetOnExit = false;

        private bool talking;
        private SaveData save;
        private Player player;
        private PlayerMovement playerMovement;

        private bool dialogueEnded;// 对话是否已经结束
        // 开启对话协程
        private Coroutine talkRoutine;

        private void OnDisable()
        {
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.OnLineChanged -= HandleLineChange;
        }
        
        private List<(string speaker, string content)> parsedLines;
        
        private void HandleLineChange(int idx)
        {
            // 特殊场景动画逻辑（保留你原来的）
            if (SceneManager.GetActiveScene().name == "C1S3 guard")
            {
                ControlGif(idx);
            }

            if (parsedLines == null || idx < 0 || idx >= parsedLines.Count)
                return;

            var (speaker, _) = parsedLines[idx];

            if (string.IsNullOrEmpty(speaker) || speaker == "旁白")
                return;

            // 去括号：只用于显示名字
            string displaySpeaker = Regex.Replace(speaker, "（.*?）", "").Trim();

            // 显示名字用去括号的版本
            if (!string.IsNullOrEmpty(displaySpeaker))
                InfoDialogUI.Instance?.SetNameText(displaySpeaker);

            // 背景用完整 speaker（包含括号）
            InfoDialogUI.Instance?.EnableCharacterBackground(speaker);
        }

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void Start()
        {
            // 确保 GameState 存档已载入
            if (GameState.Current == null)
                GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);
            
            // 用全局 GameState 判断是否看过该对话
            if (!resetOnExit && GameState.HasSeenDialogue(dialogueId))
            {
                Destroy(gameObject);
                return;
            }
            if (!player)
                player = FindObjectOfType<Player>();
            if (!playerMovement)
                playerMovement = FindObjectOfType<PlayerMovement>();
            
            // 注册行变更事件
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.OnLineChanged += HandleLineChange;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            if (!resetOnExit && save != null && save.HasSeenDialogue(dialogueId)) return;
            if (talking) return;
            if (talkRoutine != null) StopCoroutine(talkRoutine);
            talkRoutine = StartCoroutine(BeginTalkFlow());
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (!resetOnExit) return;
            if (!other.CompareTag(playerTag)) return;
            if (!talking) return;
            talking = false;
            // === 清空当前对话 ===
            StopAllCoroutines(); // 停止打字机协程等
            // 解锁玩家
            if (lockPlayerDuringDialogue)
            {
                if (player) player.UnlockControl();
                if (playerMovement) playerMovement.UnlockControl();
            }

            // 清空UI
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.EndDialogue();
        }
        
        /// <summary>
        /// 主流程：触发 → 对话 → 教学或存档 → 结束
        /// </summary>
        private IEnumerator BeginTalkFlow()
        {
            talking = true;
            // 锁定玩家
            if (lockPlayerDuringDialogue)
            {
                if (player) player.LockControl();
                if (playerMovement) playerMovement.LockControl();
            }

            // 准备对白数据（去掉显示用名字中的括号）
            var lineData = new List<(string speaker, string content)>();
            parsedLines = new List<(string speaker, string content)>();

            foreach (var l in lines)
            {
                string fullSpeaker = l.speaker?.Trim() ?? "";
                string content = l.content?.Trim() ?? "";

                // 去掉显示用括号内容
                string displaySpeaker = Regex.Replace(fullSpeaker, "（.*?）", "").Trim();

                // 用 displaySpeaker 传给 InfoDialogUI（用于显示）
                lineData.Add((displaySpeaker, content));

                // 保存完整 speaker 以便 HandleLineChange 用于背景加载
                parsedLines.Add((fullSpeaker, content));
            }

            dialogueEnded = false;

            if (!InfoDialogUI.Instance)
            {
                Debug.LogWarning("[DialogueTrigger2D_Save] InfoDialogUI 未实例化");
                yield break;
            }

            // 调用 InfoDialogUI 开始对白
            InfoDialogUI.Instance.BeginDialogue(lineData, OnDialogueEnd);


            // 等待对白结束
            yield return new WaitUntil(() => dialogueEnded);
            talking = false;
        }
        
        private void OnDialogueEnd()
        {
            if (!this) return; // 防止对象已销毁还回调
            dialogueEnded = true;
            // 解锁玩家
            if (lockPlayerDuringDialogue)
            {
                if (player) player.UnlockControl();
                if (playerMovement) playerMovement.UnlockControl();
            }

            // 存档、教学提示、销毁物体等
            HandleSaveAndDestroy();
        }
        
        private void HandleSaveAndDestroy()
        {
            // === 存档标记（仅当非 reset 模式时）===
            if (!resetOnExit)
            {
                //  统一保存到 GameState
                if (!GameState.HasSeenDialogue(dialogueId))
                {
                    var list = GameState.Current.dialogueSeenIds.ToList();
                    list.Add(dialogueId);
                    GameState.Current.dialogueSeenIds = list.ToArray();
                    GameState.SaveNow();
                }
            }

            // 特殊场景教学提示
            if (SceneManager.GetActiveScene().name == "C1S1 campus" && !resetOnExit)
            {
                InfoDialogUI.Instance.ShowMessage("-按“A”“D”进行移动-");
                StartCoroutine(WaitForMoveInputToHideHint());
            }

            // 销毁自身
            if (destroyAfterFinish && !resetOnExit)
                Destroy(gameObject);
        }
        
        /// <summary>
        /// 教学场景监听移动键
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForMoveInputToHideHint()
        {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D));
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.ShowMessage(""); // 清空提示
        }
        
        /// <summary>
        /// 选中时可视化触发范围
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            var col = GetComponent<Collider2D>();
            if (!col) return;

            var prev = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1f, 1f, 0f, 0.25f);

            switch (col)
            {
                case BoxCollider2D b:
                    Gizmos.DrawCube((Vector3)b.offset, (Vector3)b.size);
                    break;
                case CircleCollider2D c:
                    Gizmos.DrawSphere((Vector3)c.offset, c.radius);
                    break;
            }

            Gizmos.matrix = prev;
        }

        /// <summary>
        /// 调用 GIF 动画控制(仅门卫室场景使用)
        /// </summary>
        private void ControlGif(int idx)
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
    }
}
