using System.Collections;
using Items;
using Save;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
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
        [Tooltip("阅读面板在对话之后再打开（建议开启）")]
        public bool openReaderAfterDialogue = true;
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

        public System.Collections.Generic.List<DialogueLine> lines =
            new System.Collections.Generic.List<DialogueLine>
            {
                new DialogueLine{ speaker="旁白", content="你收录了一份重要的文件……" },
                new DialogueLine{ speaker="旁白", content="也许能从中发现线索。" },
            };

        [Header("对话参数")]
        public KeyCode nextKey = KeyCode.Space;
        [Tooltip("逐字机每字符延时（秒），使用实时计时")]
        public float typeCharDelay = 0.04f;
        [Tooltip("最后一句显示完后是否自动关闭")]
        public bool autoCloseOnLastLine = true;
        [Tooltip("基础自动关闭延时（秒，实时）")]
        public float autoCloseDelay = 0.3f;
        [Tooltip("最后一句额外最少停留时长（秒，实时），避免刚显示就关")]
        public float lastLineMinHold = 0.7f;

        [Header("推进去抖")]
        [Tooltip("快进后一段时间内不再接受下一句输入，避免同一击空格直接结束")]
        public float advanceCooldown = 0.15f;

        [Header("调试")]
        public bool logDebug = false;

        // ========= 状态 =========
        bool _inRange;
        bool _consumed;
        bool _talking;
        int _idx;
        Coroutine _typeRoutine;
        bool _lineFullyShown;
        float _nextAcceptTime = 0f;     // 下一次允许推进的时间（unscaled）

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col) col.isTrigger = true;
            tag = GetComponent<SaveTag>();
        }

        void Start()
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
                return;
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_consumed) return;
            if (other.CompareTag("Player"))
            {
                _inRange = true;
                InfoDialogUI.Instance?.ShowMessage(promptString);
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (_consumed) return;
            if (other.CompareTag("Player"))
            {
                _inRange = false;
                InfoDialogUI.Instance?.Clear();
            }
        }

        void Update()
        {
            // 只有在“已消费 且 不在对话中”时才早退 —— 保证对话里还能接收按键
            if (_consumed && !_talking) return;

            // 对话期间不允许再次触发拾取
            if (_inRange && !_talking && IsKeyPressed(pickupKey))
                TryPickupDoc();

            // 推进对话（加时间门槛去抖）
            if (_talking && Time.unscaledTime >= _nextAcceptTime && IsKeyPressed(nextKey))
            {
                if (logDebug) Debug.Log($"[PickupDoc2D] Detected key {nextKey}, fully={_lineFullyShown}, idx={_idx}");
                if (!_lineFullyShown) ShowLineInstant();
                else NextLine();
            }
        }

        void TryPickupDoc()
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

        IEnumerator PickupFlow(DocDB.DocDef def, string display, bool isNew)
        {
            _consumed = true;

            InfoDialogUI.Instance?.Clear();

            if (showPickupToast && InfoDialogUI.Instance)
            {
                string msg = isNew ? $"获得《{display}》" : $"已收录《{display}》";
                InfoDialogUI.Instance.ShowMessage(msg);
                yield return new WaitForSecondsRealtime(0.5f);
            }

            if (triggerDialogueOnPickup && lines != null && lines.Count > 0 && InfoDialogUI.Instance)
            {
                _talking = true;
                _idx = 0;

                InfoDialogUI.Instance.StartDialogue();
                ShowCurrentLineTyped();

                // 等待对话结束
                while (_talking) yield return null;
            }

            // —— 关键：先切到 FileSlot，再打开 TextPage（与最初流程一致）——
            if (openReaderOnPickup && def != null)
            {
                if (logDebug) Debug.Log("[PickupDoc2D] OpenReader begin");

                if (SlotUIController.Instance)
                {
                    SlotUIController.Instance.ShowFileSlotFromPickup();
                    yield return null; // 等一帧确保切页完成
                }

                if (openReaderAfterDialogue)
                {
                    if (SlotUIController.Instance)
                        yield return SlotUIController.Instance.StartCoroutine(OpenReaderStable(def));
                    else
                        yield return StartCoroutine(OpenReaderStable(def));
                }
                else
                {
                    if (SlotUIController.Instance)
                        yield return SlotUIController.Instance.StartCoroutine(OpenReaderStable(def));
                    else
                        yield return StartCoroutine(OpenReaderStable(def));
                }

                if (logDebug) Debug.Log("[PickupDoc2D] OpenReader end");
            }

            if (destroyAfterPickup) Destroy(gameObject);
        }

        IEnumerator OpenReaderStable(DocDB.DocDef def)
        {
            if (logDebug) Debug.Log("[PickupDoc2D] OpenReaderStable invoked");
            if (!readerPanel) yield break;

            // 1) 强制激活到最上层 Canvas 的父链
            EnsureUIHierarchyActive(readerPanel.rootPanel ? readerPanel.rootPanel.transform
                : readerPanel.transform);

            // 2) 恢复父链上所有 CanvasGroup 的可见/交互
            RestoreCanvasGroups(readerPanel.rootPanel ? readerPanel.rootPanel.transform
                : readerPanel.transform);

            // 3) 确保根面板可见
            if (readerPanel.rootPanel && !readerPanel.rootPanel.activeSelf)
                readerPanel.rootPanel.SetActive(true);

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

        // ========== 对话播放 ==========
        void ShowCurrentLineTyped()
        {
            if (!InfoDialogUI.Instance) { EndNow(); return; }

            if (_typeRoutine != null) { StopCoroutine(_typeRoutine); _typeRoutine = null; }

            var line = lines[_idx];

            InfoDialogUI.Instance.SetNameText(string.Equals(line.speaker, "旁白") ? "" : line.speaker);
            // 可选：InfoDialogUI.Instance.EnableCharacterBackground(line.speaker);

            InfoDialogUI.Instance.textBoxText.text = "";
            InfoDialogUI.Instance.HideArrow();
            _lineFullyShown = false;

            _typeRoutine = StartCoroutine(Typewriter(line.content));
        }

        IEnumerator Typewriter(string content)
        {
            if (logDebug) Debug.Log($"[PickupDoc2D] Type start idx={_idx}, len={content.Length}");

            // 用 for，便于在快进时跳出外层循环
            for (int i = 0; i < content.Length; i++)
            {
                // 快进：整行显示并标记完成
                if (Input.GetKeyDown(nextKey))
                {
                    InfoDialogUI.Instance.textBoxText.text = content;
                    _lineFullyShown = true;
                    if (logDebug) Debug.Log("[PickupDoc2D] Type fast-forward (outer)");
                    break;
                }

                InfoDialogUI.Instance.textBoxText.text += content[i];

                // 实时计时，不受 timeScale 影响；等待中也允许快进
                float t = 0f;
                while (t < typeCharDelay)
                {
                    if (Input.GetKeyDown(nextKey))
                    {
                        InfoDialogUI.Instance.textBoxText.text = content;
                        _lineFullyShown = true;
                        if (logDebug) Debug.Log("[PickupDoc2D] Type fast-forward (inner)");
                        i = content.Length; // 跳出外层 for
                        break;
                    }
                    t += Time.unscaledDeltaTime;
                    yield return null;
                }

                if (i == content.Length - 1)
                    _lineFullyShown = true;
            }

            // 去抖：快进或行末显示完后，短时间内不接受结束/下一句
            _nextAcceptTime = Time.unscaledTime + advanceCooldown;

            bool isLast = (_idx >= lines.Count - 1);

            if (isLast && autoCloseOnLastLine)
            {
                InfoDialogUI.Instance.HideArrow();

                // 最后一行至少停留 lastLineMinHold，再叠加 autoCloseDelay
                float finalDelay = Mathf.Max(0f, lastLineMinHold) + Mathf.Max(0f, autoCloseDelay);
                if (finalDelay > 0f)
                    yield return new WaitForSecondsRealtime(finalDelay);

                EndNow(); // EndDialogue()
            }
            else
            {
                InfoDialogUI.Instance.ShowArrow();
            }

            _typeRoutine = null;
        }

        void ShowLineInstant()
        {
            if (!InfoDialogUI.Instance) { EndNow(); return; }

            if (_typeRoutine != null) { StopCoroutine(_typeRoutine); _typeRoutine = null; }

            InfoDialogUI.Instance.textBoxText.text = lines[_idx].content;
            _lineFullyShown = true;

            // 瞬显后也开启去抖
            _nextAcceptTime = Time.unscaledTime + advanceCooldown;

            bool isLast = (_idx >= lines.Count - 1);
            if (isLast && autoCloseOnLastLine)
            {
                InfoDialogUI.Instance.HideArrow();

                float finalDelay = Mathf.Max(0f, lastLineMinHold) + Mathf.Max(0f, autoCloseDelay);
                if (finalDelay > 0f)
                    StartCoroutine(AutoCloseAfterDelay(finalDelay));
                else
                    EndNow();
            }
            else
            {
                InfoDialogUI.Instance.ShowArrow();
            }

            if (logDebug) Debug.Log($"[PickupDoc2D] ShowLineInstant idx={_idx}, isLast={isLast}");
        }

        IEnumerator AutoCloseAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            EndNow();
        }

        void NextLine()
        {
            _idx++;
            if (_idx < lines.Count)
            {
                if (logDebug) Debug.Log($"[PickupDoc2D] NextLine -> {_idx}");
                ShowCurrentLineTyped();
            }
            else
            {
                if (logDebug) Debug.Log("[PickupDoc2D] NextLine -> EndNow");
                EndNow();
            }
        }

        void EndNow()
        {
            _talking = false;
            InfoDialogUI.Instance?.EndDialogue();
            if (logDebug) Debug.Log("[PickupDoc2D] Dialogue ended.");
        }

        // —— 输入检测（兼容 UI 焦点/暂停）——
        bool IsKeyPressed(KeyCode key)
        {
            // 旧输入系统
            bool legacy = Input.GetKeyDown(key) || Input.GetKeyUp(key);

            // Space 作为兜底：UI 抢走时也推进
            bool any = Input.anyKeyDown && key == KeyCode.Space;

            // 清 UI 焦点，避免 UI 吞键
            if (EventSystem.current && EventSystem.current.currentSelectedGameObject)
                EventSystem.current.SetSelectedGameObject(null);

            return legacy || any;
        }

        // —— UI 父链激活与 CanvasGroup 恢复 —— 
        void EnsureUIHierarchyActive(Transform t)
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

        void RestoreCanvasGroups(Transform t)
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
