using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Audio;
using Characters.PLayer_25D;
using Characters.Player;
using Items;
using Save;
using Scene;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dialog
{
    /// <summary>
    /// 进入场景后自动开始对话的控制器（不依赖 Trigger）。
    /// 可在对白特定索引执行音效、动画或自定义事件。
    /// 依赖 InfoDialogUI、GameState。
    /// </summary>
    public class SceneDialogue : MonoBehaviour
    {
        [Header("唯一ID（用于存档判重）")]
        public string dialogueId = "scene_dlg_001";

        [Serializable]
        public class DialogueLine
        {
            public string speaker;
            [TextArea(2, 3)] public string content;
        }

        [Header("对话内容")]
        public List<DialogueLine> lines = new()
        {
            new DialogueLine { speaker = "旁白", content = "你走进教室，空气中弥漫着粉笔灰。" },
            new DialogueLine { speaker = "姜宁", content = "……终于到了。" },
            new DialogueLine { speaker = "祝榆", content = "这地方，好久没来了。" },
        };

        [Header("行为选项")]
        public bool playOnStart = true;
        public bool destroyAfterFinish = true;
        public bool skipIfSeen = true;
        public bool lockPlayerDuringDialogue = true;

        [Header("自定义事件（可选）")]
        public AudioSource audioSource;
        public AudioClip[] dialogueSfx;
        public Animation[] dialogueAnimations;

        private bool talking;
        private bool dialogueEnded;
        private Coroutine routine;
        private Player player;
        private PlayerMovement playerMovement;
        private List<(string speaker, string content)> parsedLines;

        public GameObject second;
        public ClockController clockController;

        private void Awake()
        {
            if (GameState.Current == null)
                GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);
        }

        private void Start()
        {
            if (skipIfSeen && GameState.HasSeenDialogue(dialogueId))
            {
                Destroy(gameObject);
                return;
            }
            second.SetActive(false);

            player = FindObjectOfType<Player>();
            playerMovement = FindObjectOfType<PlayerMovement>();

            if (playOnStart)
                routine = StartCoroutine(BeginDialogueFlow());

            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.OnLineChanged += HandleLineChanged;
        }

        private void OnDestroy()
        {
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.OnLineChanged -= HandleLineChanged;
        }

        /// <summary>
        /// 对话流程主协程
        /// </summary>
        private IEnumerator BeginDialogueFlow()
        {
            talking = true;

            if (lockPlayerDuringDialogue)
            {
                player?.LockControl();
                playerMovement?.LockControl();
            }

            // 准备对白行（去掉括号仅用于显示）
            parsedLines = new List<(string speaker, string content)>();
            foreach (var l in lines)
            {
                string fullSpeaker = l.speaker?.Trim() ?? "";
                string content = l.content?.Trim() ?? "";
                string displaySpeaker = Regex.Replace(fullSpeaker, "（.*?）", "").Trim();
                parsedLines.Add((displaySpeaker, content));
            }

            dialogueEnded = false;

            InfoDialogUI.Instance?.BeginDialogue(parsedLines, OnDialogueEnd);
            yield return new WaitUntil(() => dialogueEnded);

            talking = false;
        }

        /// <summary>
        /// 行索引变更回调：这里可以根据 index 自由插入逻辑。
        /// </summary>
        private void HandleLineChanged(int index)
        {
            // 示例：播放音效或触发动画
            if (dialogueSfx != null && index >= 0 && index < dialogueSfx.Length && dialogueSfx[index])
            {
                if (audioSource)
                    audioSource.PlayOneShot(dialogueSfx[index]);
            }

            if (dialogueAnimations != null && index >= 0 && index < dialogueAnimations.Length && dialogueAnimations[index])
            {
                dialogueAnimations[index].Play();
            }

            // 示例：根据索引执行自定义事件
            switch (index)
            {
                case 1:
                    if (GameState.HasItem("second_hand"))
                    {
                        var inv = FindObjectOfType<InventoryLite>();
                        inv.Remove("second_hand", 1);
                        // 删除完后，把当前背包同步回 GameState 并保存
                        inv.SnapshotToGameState();
                        GameState.SaveNow();
                    }
                    second.SetActive(true);
                    // 暂停对白
                    InfoDialogUI.Instance?.PauseDialogue();
                    // 播放语音
                    if (AudioClipHelper.Instance)
                    {
                        AudioClipHelper.Instance.Play_Clock1();
                    }
                    // 两秒后继续对白
                    StartCoroutine(WaitForWhisperThenContinue(2f));
                    break;
                case 2:
                    // 暂停对白
                    InfoDialogUI.Instance?.PauseDialogue();
                    clockController.ShowSecondAndStartRotation();
                    // 播放语音
                    if (AudioClipHelper.Instance)
                    {
                        AudioClipHelper.Instance.Play_Clock2();
                    }
                    // 两秒后继续对白
                    StartCoroutine(WaitForWhisperThenContinue(3f));
                    break;
                case 3:
                    // 暂停对白
                    InfoDialogUI.Instance?.PauseDialogue();
                    clockController.StopRotationAndPause();
                    clockController.SwitchYearDisplay();
                    // 播放语音
                    if (AudioClipHelper.Instance)
                    {
                        AudioClipHelper.Instance.Play_Clock3();
                    }
                    // 两秒后继续对白
                    StartCoroutine(WaitForWhisperThenContinue(3f));
                    break;
                case 4:
                    // 暂停对白
                    InfoDialogUI.Instance?.PauseDialogue();
                    clockController.ShowSecondAndStartRotation();
                    // 播放语音
                    if (AudioClipHelper.Instance)
                    {
                        AudioClipHelper.Instance.Play_Clock4(); 
                    }
                    // 两秒后继续对白
                    StartCoroutine(WaitForWhisperThenContinue(2f));
                    break;
                    
                    
            }
        }
        
        private IEnumerator WaitForWhisperThenContinue(float s)
        {
            yield return new WaitForSecondsRealtime(s);
            InfoDialogUI.Instance?.ResumeDialogue();
        }

        private void OnDialogueEnd()
        {
            dialogueEnded = true;

            if (lockPlayerDuringDialogue)
            {
                player?.UnlockControl();
                playerMovement?.UnlockControl();
            }

            // 存档标记
            if (!GameState.HasSeenDialogue(dialogueId))
            {
                var list = new List<string>(GameState.Current.dialogueSeenIds) { dialogueId };
                GameState.Current.dialogueSeenIds = list.ToArray();
                GameState.SaveNow();
            }

            SceneFadeEffect sceneFadeEffect = FindObjectOfType<SceneFadeEffect>();
            if (sceneFadeEffect)
            {
                sceneFadeEffect.FadeOutAndLoad("C1S1 campus",0.5f,1f);
            }

            if (destroyAfterFinish)
                Destroy(gameObject);
        }
    }
}
