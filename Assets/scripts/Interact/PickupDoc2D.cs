using System.Collections;
using System.Collections.Generic;
using Characters.PLayer_25D;
using Characters.Player;
using Items;
using Save;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Audio;

// ScrollRect

namespace Interact
{
    [RequireComponent(typeof(Collider2D))]
    public class PickupDoc2D : MonoBehaviour
    {
        [Header("�ĵ�����")]
        public string docId = "note1";
        public bool openReaderOnPickup = true;
        public bool destroyAfterPickup = false;

        [Header("����")]
        public KeyCode pickupKey = KeyCode.E;

        [Header("��ʾUI������ռ䣩")]
        [TextArea] public string promptString = "�� <b>E</b> �Ķ�/��¼";

        [Header("���ã��������Զ��ң�")]
        public DocInventoryLite docInventory;
        public DocDB docDB;
        public DocReaderPanel readerPanel;

        [Header("��ѡ��Ч")]
        public AudioSource sfxSource;
        public AudioClip pickupSfx;

        [Header("Save")]
        public new SaveTag tag;
        public bool autoSaveOnPickup = true;

        // ===== �Ի����� =====
        [System.Serializable]
        public class DialogueLine
        {
            public string speaker;
            [TextArea(2, 3)] public string content;
        }

        [Header("ʰȡ���Զ��Ի�")]
        public bool triggerDialogueOnPickup = true;
        public bool showPickupToast = true;

        public List<DialogueLine> lines =
            new()
            {
                new DialogueLine() { speaker="�԰�", content="����¼��һ����Ҫ���ļ�����" },
                new DialogueLine() { speaker="�԰�", content="Ҳ���ܴ��з���������" },
            };
        
        [Header("����")]
        public bool logDebug = true;

        // ========= ״̬ =========
        private bool _inRange;
        private bool _consumed;
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
            if (!docInventory) docInventory = FindObjectOfType<DocInventoryLite>();
            if (!docDB && docInventory) docDB = docInventory.docDB;
            if (!readerPanel) readerPanel = FindObjectOfType<DocReaderPanel>(true);

            if (GameState.Current == null)
                GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

            if (tag && !string.IsNullOrEmpty(tag.id) && GameState.IsObjectDisabled(tag.id))
            {
                gameObject.SetActive(false);
                _consumed = true;
            }

            player = FindObjectOfType<Player>();
            playerMovement = FindObjectOfType<PlayerMovement>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_consumed) return;
            if (other.CompareTag("Player"))
            {
                _inRange = true;
                InfoDialogUI.Instance?.ShowMessage(promptString);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_consumed) return;
            if (other.CompareTag("Player"))
            {
                _inRange = false;
                InfoDialogUI.Instance?.Clear();
            }
        }

        private void Update()
        {
            // ֻ���ڡ������ѡ�ʱ������ ���� ��֤�Ի��ﻹ�ܽ��հ���
            if (_consumed) return;

            if (_inRange && Input.GetKeyDown(pickupKey))
                TryPickupDoc();
        }

        private void TryPickupDoc()
        {
            if (_consumed) return;
            if (!docInventory)
            {
                Debug.LogWarning("[PickupDoc2D] δ�ҵ� DocInventoryLite��", this);
                return;
            }

            var def = docDB ? docDB.Get(docId) : null;
            string display = def != null && !string.IsNullOrWhiteSpace(def.displayName)
                ? def.displayName : docId;

            bool isNew = docInventory.AddOnce(docId);

            GameState.CollectDoc(docId);
            if (tag && !string.IsNullOrEmpty(tag.id))
                GameState.AddDisabledObject(tag.id);
            GameState.Current.lastScene = SceneManager.GetActiveScene().name;

            if (sfxSource && pickupSfx) sfxSource.PlayOneShot(pickupSfx);
            if (autoSaveOnPickup) GameState.SaveNow();

            AudioClipHelper.Instance.Play_PickUpPaper();

            StartCoroutine(PickupFlow(def, display, isNew));
        }

        private IEnumerator PickupFlow(DocDB.DocDef def, string display, bool isNew)
        {
            _consumed = true;

            InfoDialogUI.Instance?.Clear();
            
            foreach (var spawner in FindObjectsOfType<ConditionalSpawner>())
            {
                spawner.TryCheckNow();
            }
            
            // ====== �� �ȴ��ĵ��Ķ��� ======
            if (openReaderOnPickup && def != null)
            {
                if (logDebug) Debug.Log("[PickupDoc2D] ���ĵ��Ķ���");
       
                if (SlotUIController.Instance)
                {
                    SlotUIController.Instance.ShowFileSlotFromPickup();
                    yield return null;
                }
                else
                {
                    Debug.LogError("Slot������������");
                }
                
                // ��ס����ƶ�
                if (player)
                {
                    player.LockControl();
                }
                if (playerMovement)
                {
                    playerMovement.LockControl();
                }

                if (SlotUIController.Instance)
                {
                    SlotUIController.Instance.ShowFileSlotFromPickup();
                    yield return null; // ȷ�������л����
                }

                if (SlotUIController.Instance)
                    yield return SlotUIController.Instance.StartCoroutine(OpenReaderStable(def));
                else
                    yield return StartCoroutine(OpenReaderStable(def));

                // �ȴ���ҹر��Ķ����� DocReaderPanel ��һ�� rootPanel��
                if (readerPanel && readerPanel.rootPanel)
                {
                    while (readerPanel.rootPanel.activeSelf)
                        yield return null;
                }

                if (logDebug) Debug.Log("[PickupDoc2D] �ĵ��Ķ����ر�");
            }
            
            // ====== �� ��ʾ������ĵ�����ʾ ======
            if (showPickupToast && InfoDialogUI.Instance)
            {
                string msg = isNew ? $"��á�{display}��" : $"����¼��{display}��";
                InfoDialogUI.Instance.ShowMessage(msg);
                yield return new WaitForSecondsRealtime(1.5f);
            }
            
            // ====== �� ��ʼ�԰����� ======
            if (triggerDialogueOnPickup && lines is { Count: > 0 } && InfoDialogUI.Instance)
            {
                var dialogueLines = new List<(string speaker, string content)>();
                foreach (var l in lines)
                    dialogueLines.Add((l.speaker, l.content));
                bool finished = false;
                InfoDialogUI.Instance.BeginDialogue(dialogueLines, () => finished = true);
                
                yield return new WaitUntil(() => finished);

                // �������
                if (playerMovement) playerMovement.UnlockControl();
                if (player) player.UnlockControl();
            }

            if (destroyAfterPickup) Destroy(gameObject);
        }

        /// <summary>
        /// ���ĵ��Ķ�������
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        private IEnumerator OpenReaderStable(DocDB.DocDef def)
        {
            if (logDebug) Debug.Log("[PickupDoc2D] OpenReaderStable invoked");
            if (!readerPanel)
            {
                foreach (var r in Resources.FindObjectsOfTypeAll<DocReaderPanel>())
                {
                    readerPanel = r;
                    var go = r.gameObject;

                    if (!go.activeInHierarchy)
                    {
                        Transform cur = go.transform;
                        while (cur)
                        {
                            if (!cur.gameObject.activeSelf)
                                cur.gameObject.SetActive(true);
                            cur = cur.parent;
                        }
                    }
                    yield break;
                }
            }

            // ======= �ҵ� GameUI �㼶�����ĸ��� =======
            Transform slot = readerPanel.transform.parent;
            Transform gameUI = slot ? slot.parent : null;
            if (!gameUI)
            {
                Debug.LogWarning("[PickupDoc2D] δ�ҵ� GameUI �㼶���޷������㼶˳��");
            }

            int originalIndex = -1;
            if (gameUI && gameUI.parent)
            {
                originalIndex = gameUI.GetSiblingIndex();

                // �ؼ����� GameUI �� Canvas �±����ף������棩��λ��
                gameUI.SetAsLastSibling();
            }

            // ======= ԭ�߼������Ķ��� =======
            EnsureUIHierarchyActive(readerPanel.rootPanel ? readerPanel.rootPanel.transform : readerPanel.transform);
            RestoreCanvasGroups(readerPanel.rootPanel ? readerPanel.rootPanel.transform : readerPanel.transform);

            if (readerPanel.rootPanel)
            {
                readerPanel.rootPanel.SetActive(false);
                yield return null;
                readerPanel.rootPanel.SetActive(true);
            }

            Canvas.ForceUpdateCanvases();
            yield return null;

            readerPanel.Open(def);
            Canvas.ForceUpdateCanvases();
            yield return null;

            var scrollRect = readerPanel.contentText ?
                readerPanel.contentText.GetComponentInParent<ScrollRect>() : null;
            if (scrollRect) scrollRect.normalizedPosition = new Vector2(0, 1);

            // ======= �ȴ��ر� =======
            if (readerPanel.rootPanel)
                yield return new WaitUntil(() => !readerPanel.rootPanel.activeSelf);

            // ======= �ָ�ԭ���Ĳ㼶λ�� =======
            if (gameUI && gameUI.parent && originalIndex >= 0)
            {
                gameUI.SetSiblingIndex(originalIndex);
            }
        }



        // ���� UI ���������� CanvasGroup �ָ� ���� 
        private void EnsureUIHierarchyActive(Transform t)
        {
            if (!t) return;

            // �ҵ����ϲ� Canvas
            Transform top = t;
            while (top.parent != null)
            {
                top = top.parent;
                if (top.GetComponent<Canvas>()) break;
            }

            // �ӵ�ǰ�� Canvas����ÿһ�㶼 SetActive(true)
            var cur = t;
            while (cur != null)
            {
                if (!cur.gameObject.activeSelf) cur.gameObject.SetActive(true);
                if (cur == top) break;
                cur = cur.parent;
            }
        }

        private void RestoreCanvasGroups(Transform t)
        {
            Transform cur = t;
            while (cur)
            {
                var cg = cur.GetComponent<CanvasGroup>();
                if (cg)
                {
                    cg.alpha = 1f;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }
                if (cur.GetComponent<Canvas>()) break;
                cur = cur.parent;
            }
        }
    }
}
