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
    [RequireComponent(typeof(Collider2D))]
    public class PickupItem2D : MonoBehaviour
    {
        [Header("拾取配置")]
        [Tooltip("ItemDB 里的物品 id")]
        public string itemId = "sparkler";
        public int amount = 1;
        public KeyCode pickupKey = KeyCode.E;
        [Tooltip("拾取后销毁该物体；否则仅 SetActive(false)（会在对话播放完后执行）")]
        public bool destroyOnPickup = true;

        [Header("提示UI（可选）")]
        [TextArea] public string promptString = "按 <b>E</b> 拾取";

        [Header("引用（可留空自动找）")]
        public InventoryLite inventory;   // 若不手动拖拽，会在场景中自动 Find
        public ItemDB itemDB;             // 若不设，则尝试用 inventory.itemDB

        [Header("Save")]
        [Tooltip("必须有稳定 id 才能跨读档保持隐藏")]
        public new SaveTag tag;
        [Tooltip("拾取后是否自动保存（推荐：勾选）")]
        public bool autoSaveOnPickup = true;

        private Player player;
        private PlayerMovement playerMovement;

        // ========== 拾取后自动对话 ==========
        [System.Serializable]
        public class DialogueLine
        {
            public string speaker;
            [TextArea(2, 3)] public string content;
        }

        [Header("拾取后自动对话")]
        [Tooltip("拾取完成后是否自动播放下面的对话")]
        public bool triggerDialogueOnPickup = true;

        [Tooltip("是否先弹一句『获得 XXX xN』，然后再进入下面的正式对话")]
        public bool showPickupToast = true;

        public List<DialogueLine> lines = new()
        {
            new DialogueLine{ speaker="旁白", content="你捡起了某样重要的东西……" },
            new DialogueLine{ speaker="？？？", content="这也许会派上用场。" },
        };
        

        // ===== 内部状态 =====
        private bool _playerInRange = false;
        private bool _consumed = false;           // 防止重复执行
        

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col) col.isTrigger = true;
            tag = GetComponent<SaveTag>(); // 方便自动挂上
        }

        private void Start()
        {
            // 自动补引用
            if (!inventory) inventory = FindObjectOfType<InventoryLite>();
            if (!itemDB && inventory) itemDB = inventory.itemDB;

            // 确保 GameState 可用
            if (GameState.Current == null)
                GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

            // 读档应用：若该对象已被禁用，直接隐藏并不再工作
            if (tag && !string.IsNullOrEmpty(tag.id) && GameState.IsObjectDisabled(tag.id))
            {
                gameObject.SetActive(false);
                _consumed = true;
            }
            if (!player)
                player = FindObjectOfType<Player>();
            if (!playerMovement)
                playerMovement = FindObjectOfType<PlayerMovement>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_consumed) return;
            if (other.CompareTag("Player"))
            {
                _playerInRange = true;
                if (InfoDialogUI.Instance)
                    InfoDialogUI.Instance.ShowMessage(promptString);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_consumed) return;
            if (other.CompareTag("Player"))
            {
                _playerInRange = false;
                if (InfoDialogUI.Instance)
                    InfoDialogUI.Instance.Clear();
            }
        }

        private void Update()
        {
            if (_consumed) return;

            if (_playerInRange && Input.GetKeyDown(pickupKey))
                TryPickup();
        }

        private void TryPickup()
        {
            if (_consumed) return;

            // 基础校验
            if (string.IsNullOrWhiteSpace(itemId) || amount == 0)
            {
                Debug.LogWarning($"[PickupItem2D] 无效的 itemId 或数量：{itemId}, {amount}", this);
                return;
            }
            if (!inventory)
            {
                Debug.LogWarning("[PickupItem2D] 未找到 InventoryLite，无法拾取。", this);
                return;
            }

            // 1) 更新背包
            int newCount = inventory.Add(itemId, amount);

            // 2) 计算展示名称
            string displayName = itemId;
            if (itemDB)
            {
                var def = itemDB.Get(itemId);
                if (def != null && !string.IsNullOrWhiteSpace(def.displayName))
                    displayName = def.displayName;
            }

            // 3) —— 写回 GameState —— 
            GameState.AddItem(itemId, amount);
            if (tag && !string.IsNullOrEmpty(tag.id))
                GameState.AddDisabledObject(tag.id);
            GameState.Current.lastScene = SceneManager.GetActiveScene().name;

            if (autoSaveOnPickup)
                GameState.SaveNow();

            // 4) 启动拾取后的演出：提示 + 对话（播放完再销毁/隐藏）
            StartCoroutine(PickupFlow(displayName));
        }

        private IEnumerator PickupFlow(string displayName)
        {
            _consumed = true; // 防止重复触发
            // 清掉交互提示
            InfoDialogUI.Instance?.Clear();

            // （1）提示获得物品
            if (showPickupToast && InfoDialogUI.Instance)
            {
                InfoDialogUI.Instance.ShowMessage($"获得 {displayName} x{amount}");
                yield return new WaitForSecondsRealtime(0.8f);
            }

            // （2）对白流程（新版 InfoDialogUI）
            if (triggerDialogueOnPickup && lines is { Count: > 0 } && InfoDialogUI.Instance)
            {
                var dialogueLines = new List<(string speaker, string content)>();
                foreach (var l in lines)
                    dialogueLines.Add((l.speaker, l.content));

                bool finished = false;
                InfoDialogUI.Instance.BeginDialogue(dialogueLines, () => finished = true);
                // 锁住玩家控制（防止对白期间移动或操作）
                if (player) player.LockControl();
                if (playerMovement) playerMovement.LockControl();

                // 等待 InfoDialogUI 播放完毕
                yield return new WaitUntil(() => finished);
                // 解锁玩家控制
                if (playerMovement) playerMovement.UnlockControl();
                if (player) player.UnlockControl();
            }

            // 最后处理自身
            if (destroyOnPickup) Destroy(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
