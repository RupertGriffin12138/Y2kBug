using System.Collections;
using System.Collections.Generic;
using Save;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Interact
{
    /// <summary>
    /// 2D �Ի����������Զ����� + ��˵���� + �����л� + ���ֻ���
    /// ���봥��������ʼ���ո��ƽ���ÿ����� speaker �� content������д��浵���Ƿ������� destroyAfterFinish ������
    /// ������InfoDialogUI(Instance/StartDialogue/EndDialogue/SetNameText/ShowMessage/textBoxText/EnableCharacterBackground/ShowArrow/HideArrow)��SaveManager��SaveData
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DialogueTrigger2D_Save : MonoBehaviour
    {
        [Header("ΨһID�����ڴ浵���أ�")]
        public string dialogueId = "dlg_001";

        [System.Serializable]
        public class DialogueLine
        {
            public string speaker;
            [TextArea(2, 3)] public string content;
        }

        [Header("�Ի����ݣ�ÿ���������+���ݣ�")]
        public List<DialogueLine> lines = new()
        {
            new DialogueLine { speaker = "�԰�", content = "�ٳ����ϵ��к������ⲽ����" },
            new DialogueLine { speaker = "����", content = "ʮ��" },
            new DialogueLine { speaker = "ף��", content = "�𼱡�" }
        };

        [Header("����")]
        public KeyCode nextKey = KeyCode.Space;

        [Header("����")]
        public string playerTag = "Player";

        [Header("��Ϊ")]
        public bool blockWhenPaused = true;
        public bool destroyAfterFinish = true;

        [Header("���ֻ�����")]
        [Tooltip("ÿ���ַ�����ʱ���룩")]
        public float typeCharDelay = 0.05f;

        private bool talking;
        private int idx;
        private SaveData save;

        // ���ֻ�״̬
        private Coroutine typeRoutine;
        private bool lineFullyShown;

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        void Start()
        {
            save = SaveManager.LoadOrDefault("Town");
            if (save.HasSeenDialogue(dialogueId))
            {
                Destroy(gameObject);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            if (save != null && save.HasSeenDialogue(dialogueId)) return;
            if (talking) return;

            BeginTalk();
        }

        void Update()
        {
            if (blockWhenPaused && Time.timeScale == 0f) return;
            if (!talking) return;

            if (Input.GetKeyDown(nextKey))
            {
                if (!lineFullyShown)
                {
                    // ��һ�ΰ��£����̲�ȫ��ǰ��
                    ShowLineInstant();
                }
                else
                {
                    if (SceneManager.GetActiveScene().name == "C1S3 guard")
                    {
                        // ÿһ�ΰ��°����ж�һ���Ƿ���Ҫ����Gif
                        ControlGif();
                    }
                    // �ڶ��ΰ��£�������һ��
                    NextLine();
                }
            }

           
        }

        void BeginTalk()
        {
            if (lines == null || lines.Count == 0) return;

            talking = true;
            idx = 0;

            if (InfoDialogUI.Instance)
            {
                InfoDialogUI.Instance.StartDialogue();
            }

            ShowCurrentLineTyped();
        }

        void NextLine()
        {
            idx++;
            if (idx < lines.Count)
            {
                ShowCurrentLineTyped();
            }
            else
            {
                EndTalk();
            }
        }

        void ShowCurrentLineTyped()
        {
            if (!InfoDialogUI.Instance) return;

            // ͣ����һ�εĴ���Э��
            if (typeRoutine != null)
            {
                StopCoroutine(typeRoutine);
                typeRoutine = null;
            }

            var line = lines[idx];

            // ���ֿ��԰ײ���ʾ����
            if (string.Equals(line.speaker, "�԰�"))
                InfoDialogUI.Instance.SetNameText("");
            else
                InfoDialogUI.Instance.SetNameText(line.speaker);

            // �����������л�����
            InfoDialogUI.Instance.EnableCharacterBackground(line.speaker);

            // ��ղ���ʼ����
            InfoDialogUI.Instance.textBoxText.text = "";
            lineFullyShown = false;

            // ��ȷ��ֱ�ӵ��÷���
            InfoDialogUI.Instance.HideArrow();

            typeRoutine = StartCoroutine(Typewriter(line.content));
        }

        IEnumerator Typewriter(string content)
        {
            // �������
            foreach (char c in content)
            {
                InfoDialogUI.Instance.textBoxText.text += c;
                yield return new WaitForSeconds(typeCharDelay);

                // ���ڴ����а��������� Update ���߼�������ȫ
                if (Input.GetKeyDown(nextKey))
                {
                    // ������ȫ
                    InfoDialogUI.Instance.textBoxText.text = content;
                    break;
                }
            }

            // �������
            lineFullyShown = true;

            // ��ȷ��ֱ�ӵ��÷���
            InfoDialogUI.Instance.ShowArrow();

            typeRoutine = null;
        }

        void ShowLineInstant()
        {
            if (!InfoDialogUI.Instance) return;
            if (idx < 0 || idx >= lines.Count) return;

            if (typeRoutine != null)
            {
                StopCoroutine(typeRoutine);
                typeRoutine = null;
            }

            InfoDialogUI.Instance.textBoxText.text = lines[idx].content;
            lineFullyShown = true;

            // ��ȷ��ֱ�ӵ��÷���
            InfoDialogUI.Instance.ShowArrow();
        }

        void EndTalk()
        {
            talking = false;

            // �浵���
            if (save == null) save = SaveManager.LoadOrDefault("Town");
            if (save.TryMarkDialogueSeen(dialogueId))
            {
                SaveManager.Save(save);
            }

            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.EndDialogue();

            if (destroyAfterFinish)
                Destroy(gameObject);
            // else �����ڳ����У��ٴν����� save ������Ϊδ��������ٴδ�����
        }

        // ѡ��ʱ���ӻ�������Χ
        void OnDrawGizmosSelected()
        {
            var col = GetComponent<Collider2D>();
            if (!col) return;

            var prev = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1f, 1f, 0f, 0.25f);

            if (col is BoxCollider2D b) Gizmos.DrawCube((Vector3)b.offset, (Vector3)b.size);
            if (col is CircleCollider2D c) Gizmos.DrawSphere((Vector3)c.offset, c.radius);

            Gizmos.matrix = prev;
        }

        void ControlGif()
        {
            switch (idx)
            {
                case 1:
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/mouth1",new Vector2(540,370),new Vector2(545,357));
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                case 7:
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/heart1",new Vector2(-333,0),new Vector2(266,361));
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                case 9:
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/eye1",new Vector2(-345,209),new Vector2(457,262));
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                case 16:
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/mouth2_2",new Vector2(108,155),new Vector2(504,311));
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                case 20:
                    InfoDialogUI.Instance.HideGif();
                    InfoDialogUI.Instance.SpawnMultiple(true);
                    break;
                case 21:
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                case 29:
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/bug",new Vector2(1,1),new Vector2(1,1),true);
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
            }
        }
    }
}
