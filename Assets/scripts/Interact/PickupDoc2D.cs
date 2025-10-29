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

// ScrollRect

namespace Interact
{
    [RequireComponent(typeof(Collider2D))]
    public class PickupDoc2D : MonoBehaviour
    {
        [Header("文档配置")]
        public string docId = "note1";
        public bool openReaderOnPickup = true;
        public bool destroyAfterPickup = false;

        [Header("输入")]
        public KeyCode pickupKey = KeyCode.E;

        [Header("提示UI（世界空间）")]
        [TextArea] public string promptString = "按 <b>E</b> 阅读/收录";

        [Header("引用（可留空自动找）")]
        public DocInventoryLite docInventory;
        public DocDB docDB;
        public DocReaderPanel readerPanel;

        [Header("可选音效")]
        public AudioSource sfxSource;
        public AudioClip pickupSfx;

        [Header("Save")]
        public new SaveTag tag;
        public bool autoSaveOnPickup = true;

        // ===== 对话配置 =====
        [System.Serializable]
        public class DialogueLine
        {
            public string speaker;
            [TextArea(2, 3)] public string content;
        }

        [Header("拾取后自动对话")]
        public bool triggerDialogueOnPickup = true;
        public bool showPickupToast = true;

        public List<DialogueLine> lines =
            new()
            {
                new DialogueLine() { speaker="旁白", content="你收录了一份重要的文件……" },
                new DialogueLine() { speaker="旁白", content="也许能从中发现线索。" },
            };
        
        [Header("调试")]
        public bool logDebug = true;

        // ========= 状态 =========
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
            // 只有在“已消费”时才早退 —— 保证对话里还能接收按键
            if (_consumed) return;

            if (_inRange && Input.GetKeyDown(pickupKey))
                TryPickupDoc();
        }

        private void TryPickupDoc()
        {
            if (_consumed) return;
            if (!docInventory)
            {
                Debug.LogWarning("[PickupDoc2D] 未找到 DocInventoryLite。", this);
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

            StartCoroutine(PickupFlow(def, display, isNew));
        }

        private IEnumerator PickupFlow(DocDB.DocDef def, string display, bool isNew)
        {
            _consumed = true;

            InfoDialogUI.Instance?.Clear();
            
            // ====== ① 先打开文档阅读器 ======
            if (openReaderOnPickup && def != null)
            {
                if (logDebug) Debug.Log("[PickupDoc2D] 打开文档阅读器");
       
                if (SlotUIController.Instance)
                {
                    SlotUIController.Instance.ShowFileSlotFromPickup();
                    yield return null;
                }
                else
                {
                    Debug.LogError("Slot控制器不存在");
                }
                
                // 锁住玩家移动
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
                    yield return null; // 确保界面切换完成
                }

                if (SlotUIController.Instance)
                    yield return SlotUIController.Instance.StartCoroutine(OpenReaderStable(def));
                else
                    yield return StartCoroutine(OpenReaderStable(def));

                // 等待玩家关闭阅读器（ DocReaderPanel 有一个 rootPanel）
                if (readerPanel && readerPanel.rootPanel)
                {
                    while (readerPanel.rootPanel.activeSelf)
                        yield return null;
                }

                if (logDebug) Debug.Log("[PickupDoc2D] 文档阅读器关闭");
            }
            
            // ====== ② 显示“获得文档”提示 ======
            if (showPickupToast && InfoDialogUI.Instance)
            {
                string msg = isNew ? $"获得《{display}》" : $"已收录《{display}》";
                InfoDialogUI.Instance.ShowMessage(msg);
                yield return new WaitForSecondsRealtime(1.5f);
            }
            
            // ====== ③ 开始对白流程 ======
            if (triggerDialogueOnPickup && lines is { Count: > 0 } && InfoDialogUI.Instance)
            {
                var dialogueLines = new List<(string speaker, string content)>();
                foreach (var l in lines)
                    dialogueLines.Add((l.speaker, l.content));
                bool finished = false;
                InfoDialogUI.Instance.BeginDialogue(dialogueLines, () => finished = true);
                
                yield return new WaitUntil(() => finished);

                // 解锁玩家
                if (playerMovement) playerMovement.UnlockControl();
                if (player) player.UnlockControl();
            }

            if (destroyAfterPickup) Destroy(gameObject);
        }

        /// <summary>
        /// 打开文档阅读器流程
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
                    // 这个 API 可以找到未激活的
                    readerPanel = r;
                    var go = r.gameObject;

                    //  如果未激活，强制激活父链
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

            // 1) 强制激活到最上层 Canvas 的父链
            EnsureUIHierarchyActive(readerPanel.rootPanel ? readerPanel.rootPanel.transform
                : readerPanel.transform);

            // 2) 恢复父链上所有 CanvasGroup 的可见/交互
            RestoreCanvasGroups(readerPanel.rootPanel ? readerPanel.rootPanel.transform
                : readerPanel.transform);
            
            if (readerPanel.rootPanel)
            {
                readerPanel.rootPanel.SetActive(false); // 先关一次，防止 Unity 忽略激活事件
                yield return null;
                readerPanel.rootPanel.SetActive(true);
            }

            Canvas.ForceUpdateCanvases();
            yield return null;

            // 4) 真正打开内容
            readerPanel.Open(def);

            Canvas.ForceUpdateCanvases();
            yield return null;

            // 5) 滚动条顶到开头
            var scrollRect = readerPanel.contentText ?
                readerPanel.contentText.GetComponentInParent<ScrollRect>() : null;
            if (scrollRect) scrollRect.normalizedPosition = new Vector2(0, 1);
        }

        // —— UI 父链激活与 CanvasGroup 恢复 —— 
        private void EnsureUIHierarchyActive(Transform t)
        {
            if (!t) return;

            // 找到最上层 Canvas
            Transform top = t;
            while (top.parent != null)
            {
                top = top.parent;
                if (top.GetComponent<Canvas>()) break;
            }

            // 从当前到 Canvas，把每一层都 SetActive(true)
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
            while (cur != null)
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
