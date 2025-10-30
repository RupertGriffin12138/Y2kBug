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
using Audio;

namespace Interact
{
    /// <summary>
    /// 玩家按 E 触发的 2D 对话器（支持存档判重）
    /// - 进入区域显示提示；
    /// - 未按 E 不锁玩家；
    /// - 按 E 开始对话（锁定玩家）；
    /// - 对话结束后自动解锁并存档删除；
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class AvatarTrigger2D : MonoBehaviour
    {
        [Header("唯一ID（用于存档判重）")]
        public string dialogueId = "dlg_1001";

        [System.Serializable]
        public class DialogueLine
        {
            public string speaker;
            [TextArea(2, 3)] public string content;
        }

        [Header("人物实例")]
        public GameObject avatar;

        [Header("对话内容")]
        public List<DialogueLine> lines = new()
        {
            new DialogueLine { speaker = "旁白", content = "面前站着一位陌生的人……" },
            new DialogueLine { speaker = "姜宁", content = "你好，请问这里是……？" },
            new DialogueLine { speaker = "???", content = "……你来晚了。" }
        };

        [Header("玩家过滤")]
        public string playerTag = "Player";

        [Header("行为选项")]
        public bool destroyAfterFinish = true;
        public bool lockPlayerDuringDialogue = true;
        [Tooltip("是否重复对话（默认关闭 = 对话后永久删除）")]
        public bool repeatMode = false;

        [Header("提示文本")]
        public string interactHint = "按 <b>E</b> 对话";

        private bool inside;
        private bool talking;
        private bool dialogueEnded;

        private Player player;
        private PlayerMovement playerMovement;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void Start()
        {
            // === 存档检查 ===
            if (GameState.Current == null)
                GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

            if (!repeatMode && GameState.HasSeenDialogue(dialogueId))
            {
                Destroy(gameObject);
                return;
            }

            player = FindObjectOfType<Player>();
            playerMovement = FindObjectOfType<PlayerMovement>();

            // 注册行变更事件
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.OnLineChanged += HandleLineChange;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            inside = true;

            if (!talking && InfoDialogUI.Instance)
                InfoDialogUI.Instance.ShowMessage(interactHint);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            inside = false;

            // 离开后清除提示
            if (!talking)
                InfoDialogUI.Instance?.Clear();
        }

        private void OnDisable()
        {
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.OnLineChanged -= HandleLineChange;
        }

        private void Update()
        {
            if (inside && !talking && Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(BeginDialogueFlow());
            }
        }

        private void HandleLineChange(int idx)
        {
            // 特殊场景动画逻辑
            if (SceneManager.GetActiveScene().name == "C1S1 campus" && avatar.activeSelf)
            {
                ControlGif(idx);
            }
        }

        private IEnumerator BeginDialogueFlow()
        {
            talking = true;
            inside = false;
            InfoDialogUI.Instance?.Clear();

            // 锁定玩家
            if (lockPlayerDuringDialogue)
            {
                if (player) player.LockControl();
                if (playerMovement) playerMovement.LockControl();
            }

            // === 构建对白 ===
            var lineData = new List<(string speaker, string content)>();
            foreach (var l in lines)
            {
                string fullSpeaker = l.speaker?.Trim() ?? "";
                string content = l.content?.Trim() ?? "";
                string displaySpeaker = Regex.Replace(fullSpeaker, "（.*?）", "").Trim();
                lineData.Add((displaySpeaker, content));
            }

            dialogueEnded = false;
            InfoDialogUI.Instance.BeginDialogue(lineData, () => dialogueEnded = true);

            yield return new WaitUntil(() => dialogueEnded);

            // === 解锁玩家 ===
            if (lockPlayerDuringDialogue)
            {
                if (player) player.UnlockControl();
                if (playerMovement) playerMovement.UnlockControl();
            }

            talking = false;

            // === 存档 ===
            if (!repeatMode)
            {
                if (GameState.Current != null && !GameState.HasSeenDialogue(dialogueId))
                {
                    var list = GameState.Current.dialogueSeenIds.ToList();
                    list.Add(dialogueId);
                    GameState.Current.dialogueSeenIds = list.ToArray();
                    GameState.SaveNow();
                }
            }

            // === 通知条件刷新 ===
            foreach (var spawner in FindObjectsOfType<ConditionalSpawner>())
                spawner.TryCheckNow();

            // === 销毁自身 ===
            if (destroyAfterFinish && !repeatMode)
                Destroy(gameObject);
        }

        /// <summary>
        /// 调用 GIF 动画控制(仅门卫室场景使用)
        /// </summary>
        private void ControlGif(int idx)
        {
            switch (idx)
            {
                case 2:
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/mouth1", new Vector2(536, 385), new Vector2(475, 329));
                    AudioClipHelper.Instance.Play_Mouse1();
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                case 4:
                    AudioManager.Instance.StopLatelyAudio();
                    break;
                case 8:
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/heart1", new Vector2(-359, -31), new Vector2(265, 357));
                    AudioClipHelper.Instance.Play_hreat1();
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                case 10:
                    AudioManager.Instance.StopLatelyAudio();

                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/eye1", new Vector2(-300, 256), new Vector2(412, 250));
                    AudioClipHelper.Instance.Play_Eyes1();
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                    case 12:
                    AudioManager.Instance.StopLatelyAudio();
                    break;
                case 17:
                    InfoDialogUI.Instance.HideAllGifs();
                    AudioManager.Instance.StopAllAudio();
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/mouth2_2", new Vector2(108, 155), new Vector2(504, 311));
                    AudioClipHelper.Instance.Play_Mouse2();
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                case 19:
                    AudioManager.Instance.StopLatelyAudio();
                    break;
                case 21:
                    InfoDialogUI.Instance.HideGif();
                    //AudioManager.Instance.StopLatelyAudio();
                    InfoDialogUI.Instance.SpawnMultiple(true);
                    AudioClipHelper.Instance.Play_MutiImage();
                    break;
                case 22:
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    AudioClipHelper.Instance.Stop_MutiImage();
                    break;
                case 24:
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    InfoDialogUI.Instance.PauseDialogue();
                    StartCoroutine(FadeOutAvatar(avatar, 1f)); // 1f = 渐隐时间（秒）
                    // 启动协程等待 GIF 播放 1 秒后恢复对白
                    StartCoroutine(ResumeDialogueAfterDelay(1f));
                    break;
                case 30:
                    // 暂停对白输入
                    InfoDialogUI.Instance.PauseDialogue();
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/bug1", new Vector2(1, 1), new Vector2(1, 1), true);
                    AudioClipHelper.Instance.Play_Worm();
                    

                    InfoDialogUI.Instance.SpawnMultiple(false);
                    // 启动协程等待 GIF 播放 1 秒后恢复对白
                    StartCoroutine(ResumeDialogueAfterDelay1(4.5f));
                    
                    break;
            }


        }
        
        /// <summary>
        /// 渐隐角色并在结束后隐藏
        /// </summary>
        private IEnumerator FadeOutAvatar(GameObject obj, float duration)
        {
            if (!obj) yield break;

            // 尝试获取 SpriteRenderer 或 CanvasGroup（支持2D角色或UI角色）
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();

            float t = 0f;

            // 如果都没有，就尝试整个子层级
            if (!sr && !cg)
            {
                sr = obj.GetComponentInChildren<SpriteRenderer>();
                cg = obj.GetComponentInChildren<CanvasGroup>();
            }

            // 如果还没有，则直接关掉
            if (!sr && !cg)
            {
                Destroy(obj);
                yield break;
            }

            // 淡出逻辑
            if (sr)
            {
                Color startColor = sr.color;
                while (t < duration)
                {
                    float a = Mathf.Lerp(1f, 0f, t / duration);
                    sr.color = new Color(startColor.r, startColor.g, startColor.b, a);
                    t += Time.deltaTime;
                    yield return null;
                }
                sr.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
            }
            else if (cg)
            {
                while (t < duration)
                {
                    cg.alpha = Mathf.Lerp(1f, 0f, t / duration);
                    t += Time.deltaTime;
                    yield return null;
                }
                cg.alpha = 0f;
            }

            // 最后彻底关闭
            obj.SetActive(false);
        }
        private IEnumerator ResumeDialogueAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.ResumeDialogue();
        }
        
        private IEnumerator ResumeDialogueAfterDelay1(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.ResumeDialogue();
            InfoDialogUI.Instance.HideAllGifs();
        }
    }

}
