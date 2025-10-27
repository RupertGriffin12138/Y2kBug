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
        [Header("�ĵ�����")]
        public string docId = "note1";
        public bool openReaderOnPickup = true;
        [Tooltip("�Ķ�����ڶԻ�֮���ٴ򿪣����鿪����")]
        public bool openReaderAfterDialogue = true;
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

        public System.Collections.Generic.List<DialogueLine> lines =
            new System.Collections.Generic.List<DialogueLine>
            {
                new DialogueLine{ speaker="�԰�", content="����¼��һ����Ҫ���ļ�����" },
                new DialogueLine{ speaker="�԰�", content="Ҳ���ܴ��з���������" },
            };

        [Header("�Ի�����")]
        public KeyCode nextKey = KeyCode.Space;
        [Tooltip("���ֻ�ÿ�ַ���ʱ���룩��ʹ��ʵʱ��ʱ")]
        public float typeCharDelay = 0.04f;
        [Tooltip("���һ����ʾ����Ƿ��Զ��ر�")]
        public bool autoCloseOnLastLine = true;
        [Tooltip("�����Զ��ر���ʱ���룬ʵʱ��")]
        public float autoCloseDelay = 0.3f;
        [Tooltip("���һ���������ͣ��ʱ�����룬ʵʱ�����������ʾ�͹�")]
        public float lastLineMinHold = 0.7f;

        [Header("�ƽ�ȥ��")]
        [Tooltip("�����һ��ʱ���ڲ��ٽ�����һ�����룬����ͬһ���ո�ֱ�ӽ���")]
        public float advanceCooldown = 0.15f;

        [Header("����")]
        public bool logDebug = false;

        // ========= ״̬ =========
        bool _inRange;
        bool _consumed;
        bool _talking;
        int _idx;
        Coroutine _typeRoutine;
        bool _lineFullyShown;
        float _nextAcceptTime = 0f;     // ��һ�������ƽ���ʱ�䣨unscaled��

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
            // ֻ���ڡ������� �� ���ڶԻ��С�ʱ������ ���� ��֤�Ի��ﻹ�ܽ��հ���
            if (_consumed && !_talking) return;

            // �Ի��ڼ䲻�����ٴδ���ʰȡ
            if (_inRange && !_talking && IsKeyPressed(pickupKey))
                TryPickupDoc();

            // �ƽ��Ի�����ʱ���ż�ȥ����
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

            StartCoroutine(PickupFlow(def, display, isNew));
        }

        IEnumerator PickupFlow(DocDB.DocDef def, string display, bool isNew)
        {
            _consumed = true;

            InfoDialogUI.Instance?.Clear();

            if (showPickupToast && InfoDialogUI.Instance)
            {
                string msg = isNew ? $"��á�{display}��" : $"����¼��{display}��";
                InfoDialogUI.Instance.ShowMessage(msg);
                yield return new WaitForSecondsRealtime(0.5f);
            }

            if (triggerDialogueOnPickup && lines != null && lines.Count > 0 && InfoDialogUI.Instance)
            {
                _talking = true;
                _idx = 0;

                InfoDialogUI.Instance.StartDialogue();
                ShowCurrentLineTyped();

                // �ȴ��Ի�����
                while (_talking) yield return null;
            }

            // ���� �ؼ������е� FileSlot���ٴ� TextPage�����������һ�£�����
            if (openReaderOnPickup && def != null)
            {
                if (logDebug) Debug.Log("[PickupDoc2D] OpenReader begin");

                if (SlotUIController.Instance)
                {
                    SlotUIController.Instance.ShowFileSlotFromPickup();
                    yield return null; // ��һ֡ȷ����ҳ���
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

            // 1) ǿ�Ƽ�����ϲ� Canvas �ĸ���
            EnsureUIHierarchyActive(readerPanel.rootPanel ? readerPanel.rootPanel.transform
                : readerPanel.transform);

            // 2) �ָ����������� CanvasGroup �Ŀɼ�/����
            RestoreCanvasGroups(readerPanel.rootPanel ? readerPanel.rootPanel.transform
                : readerPanel.transform);

            // 3) ȷ�������ɼ�
            if (readerPanel.rootPanel && !readerPanel.rootPanel.activeSelf)
                readerPanel.rootPanel.SetActive(true);

            Canvas.ForceUpdateCanvases();
            yield return null;

            // 4) ����������
            readerPanel.Open(def);

            Canvas.ForceUpdateCanvases();
            yield return null;

            // 5) ������������ͷ
            var scrollRect = readerPanel.contentText ?
                readerPanel.contentText.GetComponentInParent<ScrollRect>() : null;
            if (scrollRect) scrollRect.normalizedPosition = new Vector2(0, 1);
        }

        // ========== �Ի����� ==========
        void ShowCurrentLineTyped()
        {
            if (!InfoDialogUI.Instance) { EndNow(); return; }

            if (_typeRoutine != null) { StopCoroutine(_typeRoutine); _typeRoutine = null; }

            var line = lines[_idx];

            InfoDialogUI.Instance.SetNameText(string.Equals(line.speaker, "�԰�") ? "" : line.speaker);
            // ��ѡ��InfoDialogUI.Instance.EnableCharacterBackground(line.speaker);

            InfoDialogUI.Instance.textBoxText.text = "";
            InfoDialogUI.Instance.HideArrow();
            _lineFullyShown = false;

            _typeRoutine = StartCoroutine(Typewriter(line.content));
        }

        IEnumerator Typewriter(string content)
        {
            if (logDebug) Debug.Log($"[PickupDoc2D] Type start idx={_idx}, len={content.Length}");

            // �� for�������ڿ��ʱ�������ѭ��
            for (int i = 0; i < content.Length; i++)
            {
                // �����������ʾ��������
                if (Input.GetKeyDown(nextKey))
                {
                    InfoDialogUI.Instance.textBoxText.text = content;
                    _lineFullyShown = true;
                    if (logDebug) Debug.Log("[PickupDoc2D] Type fast-forward (outer)");
                    break;
                }

                InfoDialogUI.Instance.textBoxText.text += content[i];

                // ʵʱ��ʱ������ timeScale Ӱ�죻�ȴ���Ҳ������
                float t = 0f;
                while (t < typeCharDelay)
                {
                    if (Input.GetKeyDown(nextKey))
                    {
                        InfoDialogUI.Instance.textBoxText.text = content;
                        _lineFullyShown = true;
                        if (logDebug) Debug.Log("[PickupDoc2D] Type fast-forward (inner)");
                        i = content.Length; // ������� for
                        break;
                    }
                    t += Time.unscaledDeltaTime;
                    yield return null;
                }

                if (i == content.Length - 1)
                    _lineFullyShown = true;
            }

            // ȥ�����������ĩ��ʾ��󣬶�ʱ���ڲ����ܽ���/��һ��
            _nextAcceptTime = Time.unscaledTime + advanceCooldown;

            bool isLast = (_idx >= lines.Count - 1);

            if (isLast && autoCloseOnLastLine)
            {
                InfoDialogUI.Instance.HideArrow();

                // ���һ������ͣ�� lastLineMinHold���ٵ��� autoCloseDelay
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

            // ˲�Ժ�Ҳ����ȥ��
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

        // ���� �����⣨���� UI ����/��ͣ������
        bool IsKeyPressed(KeyCode key)
        {
            // ������ϵͳ
            bool legacy = Input.GetKeyDown(key) || Input.GetKeyUp(key);

            // Space ��Ϊ���ף�UI ����ʱҲ�ƽ�
            bool any = Input.anyKeyDown && key == KeyCode.Space;

            // �� UI ���㣬���� UI �̼�
            if (EventSystem.current && EventSystem.current.currentSelectedGameObject)
                EventSystem.current.SetSelectedGameObject(null);

            return legacy || any;
        }

        // ���� UI ���������� CanvasGroup �ָ� ���� 
        void EnsureUIHierarchyActive(Transform t)
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
