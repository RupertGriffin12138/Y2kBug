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
    /// ���԰׵ı���ʰȡ����
    /// ������������ + �Զ���԰� + �Զ��浵 + �Զ����١�
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PickupBackpackDialogue2D : MonoBehaviour
    {
        [Header("��������")]
        public KeyCode pickupKey = KeyCode.E;
        public bool destroyOnPickup = true;

        [Header("��ʾ�ı�")]
        [TextArea] public string promptString = "�� <b>E</b> ʰȡ����";
        [TextArea] public string obtainedString = "��� �������ѽ���ȫ�����ӣ�";

        [Header("���ã��������Զ��ң�")]
        public InventoryLite inventory;
        public AudioSource sfxSource;
        public AudioClip pickupSfx;

        [Header("�԰�����")]
        [Tooltip("ʰȡ��ɺ��Ƿ��Զ��������¶԰�")]
        public bool triggerDialogueOnPickup = true;

        [System.Serializable]
        public class DialogueLine
        {
            public string speaker;
            [TextArea(2, 3)] public string content;
        }

        [Tooltip("�Զ���ʰȡ��԰ף��ɶ��У�")]
        public List<DialogueLine> lines = new()
        {
            new DialogueLine{ speaker="�԰�", content="�����˱�����" },
            new DialogueLine{ speaker="�԰�", content="�����ڿ���Я��������Ʒ�ˡ�" },
        };

        [Header("�浵����")]
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

            // ��������ѽ�����ֱ������
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

            // �Ѿ�������
            if (GameState.BackpackUnlocked)
            {
                InfoDialogUI.Instance?.ShowMessage("�����ѽ���");
                StartCoroutine(ClearMessageAfter(1.2f));
                return;
            }

            // 1) �����߼�
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

            // 2) ������
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

            // 3) ��������
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
