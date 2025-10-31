using System;
using Characters.PLayer_25D;
using Characters.Player;
using TMPro;
using UI;
using UnityEngine;

namespace Items
{
    public class DocReaderPanel : MonoBehaviour
    {
        [Header("����")]
        public DocDB docDB;

        [Header("UI ����")]
        public GameObject rootPanel;     // ���� TextPage ��壨�� ScrollView��
        public TMP_Text contentText;     // ScrollView/Viewport/Content �ϵ� TMP_Text
        
        private Player player;
        private PlayerMovement playerMovement;
        private void Awake()
        {
            if (rootPanel) rootPanel.SetActive(false);
        }

        private void Start()
        {
            // �Զ��������
            player = FindObjectOfType<Player>();
            playerMovement = FindObjectOfType<PlayerMovement>();

            rootPanel = gameObject;
        }
        
        private void Update()
        {
            // ESC �Զ��ر�
            if (rootPanel && rootPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        private void OnEnable()
        {
            if (player) player.LockControl();
            if (playerMovement) playerMovement.LockControl();
        }

        private void OnDisable()
        {
          
            if (player) player.UnlockControl();
            if (playerMovement) playerMovement.UnlockControl(); 
        }

        /// <summary>ͨ�� id �򿪣��� DocUI��ʰȡ�ű��ȵ��ã�</summary>
        public void OpenById(string docId)
        {
            if (docDB == null || string.IsNullOrEmpty(docId))
            {
                Debug.LogWarning("[DocReaderPanel] ��Ч docId ��δ���� docDB��", this);
                return;
            }

            var def = docDB.Get(docId);
            if (def == null)
            {
                Debug.LogWarning($"[DocReaderPanel] docDB ��δ�ҵ� id: {docId}", this);
                return;
            }

            Open(def);
        }

        /// <summary>ͨ���ĵ�����ֱ�Ӵ򿪣���ʰȡ�ű����ã�</summary>
        public void Open(DocDB.DocDef def)
        {
            if (def == null)
            {
                Debug.LogWarning("[DocReaderPanel] ����� DocDef Ϊ�ա�", this);
                return;
            }

            // --- �޸��ص㣺ȷ�� UI �Ѽ�����ˢ�� ---
            if (rootPanel && !rootPanel.activeSelf)
                rootPanel.SetActive(true);

            // ����ˢ�� Canvas ���֣���ֹ���״μ����ʾ��
            Canvas.ForceUpdateCanvases();

            // ����ı�����
            if (contentText)
                contentText.text = def.content ?? "(��������)";

            // ��ˢ��һ�Σ�ȷ�����ֲ��ָ���
            Canvas.ForceUpdateCanvases();

            // �������ص�����
            var scrollRect = contentText?.GetComponentInParent<UnityEngine.UI.ScrollRect>();
            if (scrollRect)
                scrollRect.normalizedPosition = new Vector2(0, 1);
        }

        /// <summary>�ر��Ķ����</summary>
        public void Close()
        {
            if (rootPanel)
            {
                rootPanel.SetActive(false);
            }
        }
    }
}
