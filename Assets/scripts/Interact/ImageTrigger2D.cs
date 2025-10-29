using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Audio;
using Characters.PLayer_25D;
using Characters.Player;
using Save;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Interact
{
    [RequireComponent(typeof(Collider2D))]
    public class ImageTrigger2D : MonoBehaviour
    {
        [Header("唯一ID（用于存档判重）")]
        public string dialogueId = "img_001";

        [Header("显示图片")]
        public Texture2D texture2D;
        public float fadeDuration = 1.0f;

        [Header("对话内容")]
        public List<DialogueTrigger2D_Save.DialogueLine> lines = new()
        {
            new DialogueTrigger2D_Save.DialogueLine { speaker = "旁白", content = "一幅奇怪的画映入眼帘……" },
            new DialogueTrigger2D_Save.DialogueLine { speaker = "姜宁", content = "这幅画，似乎在动……" },
            new DialogueTrigger2D_Save.DialogueLine { speaker = "祝榆", content = "……小心点。" }
        };

        [Header("过滤")]
        public string playerTag = "Player";

        [Header("行为")]
        public bool destroyAfterFinish = true;
        public bool repeatMode = false;
        public bool lockPlayerDuringDialogue = true;

        [Header("遮罩（外部设置好的UI物体）")]
        public GameObject mask;

        private bool inside;
        private bool talking;
        public static bool imageShown;
        private bool dialogueEnded;

        private Player player;
        private PlayerMovement playerMovement;

        private RawImage fullScreenImage;
        private CanvasGroup maskCanvasGroup;
        private CanvasGroup imageCanvasGroup;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void Start()
        {
            if (GameState.Current == null)
                GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

            if (!repeatMode && GameState.HasSeenDialogue(dialogueId))
            {
                Destroy(gameObject);
                return;
            }

            if (mask)
            {
                maskCanvasGroup = mask.GetComponent<CanvasGroup>();
                if (!maskCanvasGroup) maskCanvasGroup = mask.AddComponent<CanvasGroup>();
                maskCanvasGroup.alpha = 0f;
                mask.SetActive(false);
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

            // 用“旁白”的方式提示
            if (InfoDialogUI.Instance)
            {
                var hintLines = new List<(string speaker, string content)>
                {
                    ("旁白", "按 E 交互")
                };

                InfoDialogUI.Instance.BeginDialogue(hintLines);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            if (!repeatMode)
            {
                return;
            }
            inside = false;
            InfoDialogUI.Instance?.Clear();
            // === 清空当前对话 ===
            StopAllCoroutines(); // 停止打字机协程等
            if (InfoDialogUI.Instance)
            {
                InfoDialogUI.Instance.EndDialogue();
            }
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
                StartCoroutine(BeginSequence());
            }

            // ESC 退出全屏
            if (imageShown && dialogueEnded && Input.GetKeyDown(KeyCode.Escape))
            {
                StartCoroutine(FadeOutMaskAndImage());
            }
        }

        private IEnumerator BeginSequence()
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

            // 创建图片并附加到mask中
            CreateImageUnderMask();

            // 渐入遮罩 + 图片
            yield return StartCoroutine(FadeInMaskAndImage());

            imageShown = true;

            // 播放对白
            yield return StartCoroutine(StartDialogueFlow());

            // 对话结束后提示ESC
            InfoDialogUI.Instance?.ShowMessage("- 按 ESC 退出 -");
        }

        private IEnumerator StartDialogueFlow()
        {
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
        }

        private void HandleLineChange(int idx)
        {
            if (idx == 1)
            {
                // 暂停对白
                InfoDialogUI.Instance?.PauseDialogue();

                // 播放语音
                if (AudioClipHelper.Instance)
                    AudioClipHelper.Instance.Play_ManWhisper();

                // 两秒后继续对白
                StartCoroutine(WaitForWhisperThenContinue());
            }
        }

        private IEnumerator WaitForWhisperThenContinue()
        {
            yield return new WaitForSecondsRealtime(2f);
            InfoDialogUI.Instance?.ResumeDialogue();
        }
        
        /// <summary>
        /// 遮罩和图片渐入
        /// </summary>
        private IEnumerator FadeInMaskAndImage()
        {
            if (!maskCanvasGroup || !imageCanvasGroup) yield break;

            mask.SetActive(true);
            fullScreenImage.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                float t = elapsed / fadeDuration;
                maskCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                imageCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            maskCanvasGroup.alpha = 1f;
            imageCanvasGroup.alpha = 1f;
        }

        /// <summary>
        /// 遮罩和图片渐出
        /// </summary>
        private IEnumerator FadeOutMaskAndImage()
        {
            if (!maskCanvasGroup || !imageCanvasGroup) yield break;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                float t = elapsed / fadeDuration;
                maskCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                imageCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            maskCanvasGroup.alpha = 0f;
            imageCanvasGroup.alpha = 0f;
            fullScreenImage.gameObject.SetActive(false);
            mask.SetActive(false);
            imageShown = false;
            talking = false;

            // 解锁玩家
            if (lockPlayerDuringDialogue)
            {
                if (player) player.UnlockControl();
                if (playerMovement) playerMovement.UnlockControl();
            }

            HandleSaveAndDestroy();
        }

        /// <summary>
        /// 在mask下生成图片对象
        /// </summary>
        private void CreateImageUnderMask()
        {
            if (fullScreenImage) return;

            if (!mask)
            {
                Debug.LogError("[ImageTrigger2D] 未设置遮罩（mask）！");
                return;
            }

            GameObject imgObj = new GameObject("ImageInMask", typeof(RectTransform), typeof(RawImage), typeof(CanvasGroup));
            imgObj.transform.SetParent(mask.transform.GetChild(0).transform, false); // 成为遮罩子物体

            fullScreenImage = imgObj.GetComponent<RawImage>();
            fullScreenImage.texture = texture2D;
            fullScreenImage.color = Color.white;

            RectTransform rect = imgObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            imageCanvasGroup = imgObj.GetComponent<CanvasGroup>();
            imageCanvasGroup.alpha = 0f;
        }

        private void HandleSaveAndDestroy()
        {
            if (!repeatMode)
            {
                if (!GameState.HasSeenDialogue(dialogueId))
                {
                    var list = GameState.Current.dialogueSeenIds.ToList();
                    list.Add(dialogueId);
                    GameState.Current.dialogueSeenIds = list.ToArray();
                    GameState.SaveNow();
                }
            }

            if (destroyAfterFinish && !repeatMode)
                Destroy(gameObject);
        }
    }
}
