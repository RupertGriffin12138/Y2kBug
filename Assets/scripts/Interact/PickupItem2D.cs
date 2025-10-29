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
        [Header("ʰȡ����")]
        [Tooltip("ItemDB �����Ʒ id")]
        public string itemId = "sparkler";
        public int amount = 1;
        public KeyCode pickupKey = KeyCode.E;
        [Tooltip("ʰȡ�����ٸ����壻����� SetActive(false)�����ڶԻ��������ִ�У�")]
        public bool destroyOnPickup = true;

        [Header("��ʾUI����ѡ��")]
        [TextArea] public string promptString = "�� <b>E</b> ʰȡ";

        [Header("���ã��������Զ��ң�")]
        public InventoryLite inventory;   // �����ֶ���ק�����ڳ������Զ� Find
        public ItemDB itemDB;             // �����裬������ inventory.itemDB

        [Header("Save")]
        [Tooltip("�������ȶ� id ���ܿ������������")]
        public new SaveTag tag;
        [Tooltip("ʰȡ���Ƿ��Զ����棨�Ƽ�����ѡ��")]
        public bool autoSaveOnPickup = true;

        private Player player;
        private PlayerMovement playerMovement;

        // ========== ʰȡ���Զ��Ի� ==========
        [System.Serializable]
        public class DialogueLine
        {
            public string speaker;
            [TextArea(2, 3)] public string content;
        }

        [Header("ʰȡ���Զ��Ի�")]
        [Tooltip("ʰȡ��ɺ��Ƿ��Զ���������ĶԻ�")]
        public bool triggerDialogueOnPickup = true;

        [Tooltip("�Ƿ��ȵ�һ�䡺��� XXX xN����Ȼ���ٽ����������ʽ�Ի�")]
        public bool showPickupToast = true;

        public List<DialogueLine> lines = new()
        {
            new DialogueLine{ speaker="�԰�", content="�������ĳ����Ҫ�Ķ�������" },
            new DialogueLine{ speaker="������", content="��Ҳ��������ó���" },
        };
        

        // ===== �ڲ�״̬ =====
        private bool _playerInRange = false;
        private bool _consumed = false;           // ��ֹ�ظ�ִ��
        

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col) col.isTrigger = true;
            tag = GetComponent<SaveTag>(); // �����Զ�����
        }

        private void Start()
        {
            // �Զ�������
            if (!inventory) inventory = FindObjectOfType<InventoryLite>();
            if (!itemDB && inventory) itemDB = inventory.itemDB;

            // ȷ�� GameState ����
            if (GameState.Current == null)
                GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

            // ����Ӧ�ã����ö����ѱ����ã�ֱ�����ز����ٹ���
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

            // ����У��
            if (string.IsNullOrWhiteSpace(itemId) || amount == 0)
            {
                Debug.LogWarning($"[PickupItem2D] ��Ч�� itemId ��������{itemId}, {amount}", this);
                return;
            }
            if (!inventory)
            {
                Debug.LogWarning("[PickupItem2D] δ�ҵ� InventoryLite���޷�ʰȡ��", this);
                return;
            }

            // 1) ���±���
            int newCount = inventory.Add(itemId, amount);

            // 2) ����չʾ����
            string displayName = itemId;
            if (itemDB)
            {
                var def = itemDB.Get(itemId);
                if (def != null && !string.IsNullOrWhiteSpace(def.displayName))
                    displayName = def.displayName;
            }

            // 3) ���� д�� GameState ���� 
            GameState.AddItem(itemId, amount);
            if (tag && !string.IsNullOrEmpty(tag.id))
                GameState.AddDisabledObject(tag.id);
            GameState.Current.lastScene = SceneManager.GetActiveScene().name;

            if (autoSaveOnPickup)
                GameState.SaveNow();

            // 4) ����ʰȡ����ݳ�����ʾ + �Ի���������������/���أ�
            StartCoroutine(PickupFlow(displayName));
        }

        private IEnumerator PickupFlow(string displayName)
        {
            _consumed = true; // ��ֹ�ظ�����
            // ���������ʾ
            InfoDialogUI.Instance?.Clear();

            // ��1����ʾ�����Ʒ
            if (showPickupToast && InfoDialogUI.Instance)
            {
                InfoDialogUI.Instance.ShowMessage($"��� {displayName} x{amount}");
                yield return new WaitForSecondsRealtime(0.8f);
            }

            // ��2���԰����̣��°� InfoDialogUI��
            if (triggerDialogueOnPickup && lines is { Count: > 0 } && InfoDialogUI.Instance)
            {
                var dialogueLines = new List<(string speaker, string content)>();
                foreach (var l in lines)
                    dialogueLines.Add((l.speaker, l.content));

                bool finished = false;
                InfoDialogUI.Instance.BeginDialogue(dialogueLines, () => finished = true);
                // ��ס��ҿ��ƣ���ֹ�԰��ڼ��ƶ��������
                if (player) player.LockControl();
                if (playerMovement) playerMovement.LockControl();

                // �ȴ� InfoDialogUI �������
                yield return new WaitUntil(() => finished);
                // ������ҿ���
                if (playerMovement) playerMovement.UnlockControl();
                if (player) player.UnlockControl();
            }

            // ���������
            if (destroyOnPickup) Destroy(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
