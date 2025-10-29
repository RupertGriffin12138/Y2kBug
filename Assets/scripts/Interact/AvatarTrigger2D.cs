using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Characters.PLayer_25D;
using Characters.Player;
using Save;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Interact
{
    /// <summary>
    /// ��Ұ� E ������ 2D �Ի�����֧�ִ浵���أ�
    /// - ����������ʾ��ʾ��
    /// - δ�� E ������ң�
    /// - �� E ��ʼ�Ի���������ң���
    /// - �Ի��������Զ��������浵ɾ����
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class AvatarTrigger2D : MonoBehaviour
    {
        [Header("ΨһID�����ڴ浵���أ�")]
        public string dialogueId = "dlg_1001";

        [System.Serializable]
        public class DialogueLine
        {
            public string speaker;
            [TextArea(2, 3)] public string content;
        }

        [Header("����ʵ��")]
        public GameObject avatar;
        
        [Header("�Ի�����")]
        public List<DialogueLine> lines = new()
        {
            new DialogueLine { speaker = "�԰�", content = "��ǰվ��һλİ�����ˡ���" },
            new DialogueLine { speaker = "����", content = "��ã����������ǡ�����" },
            new DialogueLine { speaker = "???", content = "�����������ˡ�" }
        };

        [Header("��ҹ���")]
        public string playerTag = "Player";

        [Header("��Ϊѡ��")]
        public bool destroyAfterFinish = true;
        public bool lockPlayerDuringDialogue = true;
        [Tooltip("�Ƿ��ظ��Ի���Ĭ�Ϲر� = �Ի�������ɾ����")]
        public bool repeatMode = false;

        [Header("��ʾ�ı�")]
        public string interactHint = "�� <b>E</b> �Ի�";

        private bool inside;
        private bool talking;
        private bool dialogueEnded;

        private Player player;
        private PlayerMovement playerMovement;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void Start()
        {
            // === �浵��� ===
            if (GameState.Current == null)
                GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

            if (!repeatMode && GameState.HasSeenDialogue(dialogueId))
            {
                Destroy(gameObject);
                return;
            }

            player = FindObjectOfType<Player>();
            playerMovement = FindObjectOfType<PlayerMovement>();
            
            // ע���б���¼�
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.OnLineChanged += HandleLineChange;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            inside = true;

            if (!talking && InfoDialogUI.Instance)
                InfoDialogUI.Instance.ShowMessage(interactHint);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            inside = false;

            // �뿪�������ʾ
            if (!talking)
                InfoDialogUI.Instance?.Clear();
        }
        
        private void OnDisable()
        {
            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.OnLineChanged -= HandleLineChange;
        }

        private void Update()
        {
            if (inside && !talking && Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(BeginDialogueFlow());
            }
        }
        
        private void HandleLineChange(int idx)
        {
            // ���ⳡ�������߼�
            if (SceneManager.GetActiveScene().name == "C1S1 campus" && avatar.activeSelf)
            {
                ControlGif(idx);
            }
        }

        private IEnumerator BeginDialogueFlow()
        {
            talking = true;
            inside = false;
            InfoDialogUI.Instance?.Clear();

            // �������
            if (lockPlayerDuringDialogue)
            {
                if (player) player.LockControl();
                if (playerMovement) playerMovement.LockControl();
            }

            // === �����԰� ===
            var lineData = new List<(string speaker, string content)>();
            foreach (var l in lines)
            {
                string fullSpeaker = l.speaker?.Trim() ?? "";
                string content = l.content?.Trim() ?? "";
                string displaySpeaker = Regex.Replace(fullSpeaker, "��.*?��", "").Trim();
                lineData.Add((displaySpeaker, content));
            }

            dialogueEnded = false;
            InfoDialogUI.Instance.BeginDialogue(lineData, () => dialogueEnded = true);

            yield return new WaitUntil(() => dialogueEnded);

            // === ������� ===
            if (lockPlayerDuringDialogue)
            {
                if (player) player.UnlockControl();
                if (playerMovement) playerMovement.UnlockControl();
            }

            talking = false;

            // === �浵 ===
            if (!repeatMode)
            {
                if (GameState.Current != null && !GameState.HasSeenDialogue(dialogueId))
                {
                    var list = GameState.Current.dialogueSeenIds.ToList();
                    list.Add(dialogueId);
                    GameState.Current.dialogueSeenIds = list.ToArray();
                    GameState.SaveNow();
                }
            }

            // === ֪ͨ����ˢ�� ===
            foreach (var spawner in FindObjectsOfType<ConditionalSpawner>())
                spawner.TryCheckNow();

            // === �������� ===
            if (destroyAfterFinish && !repeatMode)
                Destroy(gameObject);
        }
        
        /// <summary>
        /// ���� GIF ��������(�������ҳ���ʹ��)
        /// </summary>
        private void ControlGif(int idx)
        {
            switch (idx)
            {
                case 2:
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/mouth1",new Vector2(536,385),new Vector2(475,329));
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                case 8:
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/heart1",new Vector2( -359,-31),new Vector2(265,357));
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                case 10:
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/eye1",new Vector2(-300,256),new Vector2(412,250));
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                case 17:
                    InfoDialogUI.Instance.HideAllGifs();
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/mouth2_2",new Vector2(108,155),new Vector2(504,311));
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                case 21:
                    InfoDialogUI.Instance.HideGif();
                    InfoDialogUI.Instance.SpawnMultiple(true);
                    break;
                case 22:
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    break;
                case 30:
                    // ��ͣ�԰�����
                    InfoDialogUI.Instance.PauseDialogue();
                    InfoDialogUI.Instance.ShowGif("Dialog/gif/prefab/bug",new Vector2(1,1),new Vector2(1,1),true);
                    
                    InfoDialogUI.Instance.SpawnMultiple(false);
                    // ����Э�̵ȴ� GIF ���� 1 ���ָ��԰�
                    StartCoroutine(ResumeDialogueAfterDelay(3f));
                    break;
            }
            
            
        }
        private IEnumerator ResumeDialogueAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.ResumeDialogue();
        }
    }
    
}
