using System.Collections;
using System.Collections.Generic;
using Characters.PLayer_25D;
using Characters.Player;
using Items;
using Save;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Interact
{
    /// <summary>
    /// 带对白的背包拾取器：
    /// 解锁背包功能 + 自定义对白 + 自动存档 + 自动销毁。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PickupBackpackDialogue2D : MonoBehaviour
    {
        [Header("基础设置")]
        public KeyCode pickupKey = KeyCode.E;
        public bool destroyOnPickup = true;

        [Header("提示文本")]
        [TextArea] public string promptString = "按 <b>E</b> 拾取背包";
        [TextArea] public string obtainedString = "获得 背包（已解锁全部格子）";

        [Header("引用（可留空自动找）")]
        public InventoryLite inventory;
        public AudioSource sfxSource;
        public AudioClip pickupSfx;

        [Header("对白设置")]
        [Tooltip("拾取完成后是否自动播放以下对白")]
        public bool triggerDialogueOnPickup = true;

        [System.Serializable]
        public class DialogueLine
        {
            public string speaker;
            [TextArea(2, 3)] public string content;
        }

        [Tooltip("自定义拾取后对白（可多行）")]
        public List<DialogueLine> lines = new()
        {
            new DialogueLine{ speaker="旁白", content="你获得了背包！" },
            new DialogueLine{ speaker="旁白", content="你现在可以携带更多物品了。" },
        };

        [Header("存档设置")]
        public new SaveTag tag;
        public bool autoSaveOnPickup = true;

        private bool _inRange;
        private bool _pickedUp;
        private Player player;
        private PlayerMovement playerMovement;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col) col.isTrigger = true;
            tag = GetComponent<SaveTag>();
        }

        private void Start()
        {
            if (!inventory) inventory = FindObjectOfType<InventoryLite>();
            if (!player) player = FindObjectOfType<Player>();
            if (!playerMovement) playerMovement = FindObjectOfType<PlayerMovement>();

            if (GameState.Current == null)
                GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

            // 如果背包已解锁，直接隐藏
            if (GameState.BackpackUnlocked)
            {
                gameObject.SetActive(false);
                _pickedUp = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_pickedUp) return;
            if (!other.CompareTag("Player")) return;

            _inRange = true;
            InfoDialogUI.Instance?.ShowMessage(promptString);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _inRange = false;
            if (!_pickedUp)
                InfoDialogUI.Instance?.Clear();
        }

        private void Update()
        {
            if (_pickedUp) return;
            if (_inRange && Input.GetKeyDown(pickupKey))
                TryPickup();
        }

        private void TryPickup()
        {
            if (_pickedUp) return;

            // 已经解锁过
            if (GameState.BackpackUnlocked)
            {
                InfoDialogUI.Instance?.ShowMessage("背包已解锁");
                StartCoroutine(ClearMessageAfter(1.2f));
                return;
            }

            // 1) 解锁逻辑
            GameState.UnlockBackpack(autoSaveOnPickup);
            inventory?.NotifyCapacityChanged();

            if (sfxSource && pickupSfx)
                sfxSource.PlayOneShot(pickupSfx);

            _pickedUp = true;
            InfoDialogUI.Instance?.ShowMessage(obtainedString);
            StartCoroutine(PickupFlow());
        }

        private IEnumerator PickupFlow()
        {
            yield return new WaitForSecondsRealtime(1.0f);

            // 2) 白流程
            if (triggerDialogueOnPickup && lines.Count > 0 && InfoDialogUI.Instance)
            {
                var dialogueLines = new List<(string speaker, string content)>();
                foreach (var l in lines)
                    dialogueLines.Add((l.speaker, l.content));

                bool finished = false;
                InfoDialogUI.Instance.BeginDialogue(dialogueLines, () => finished = true);

                if (player) player.LockControl();
                if (playerMovement) playerMovement.LockControl();

                yield return new WaitUntil(() => finished);

                if (playerMovement) playerMovement.UnlockControl();
                if (player) player.UnlockControl();
            }

            // 3) 结束处理
            InfoDialogUI.Instance?.Clear();
            if (destroyOnPickup) Destroy(gameObject);
            else gameObject.SetActive(false);
        }

        private IEnumerator ClearMessageAfter(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            InfoDialogUI.Instance?.Clear();
        }
    }
}
