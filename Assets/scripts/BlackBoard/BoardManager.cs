using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlackBoard
{
    public class BoardManager : MonoBehaviour
    {
        private const string BoardKey = "BoardKey_Prefab";
        private int boardProgress;
        private bool canCa=false;

        [SerializeField] private GameObject[] gameObjects;
        private readonly Dictionary<int,GameObject> boards = new ();
        [SerializeField] private GameObject boardErase;


        [SerializeField] private SpriteRenderer blackboardFade;
        [SerializeField] private float fadeDuration = 2.0f;

        private Material fadeMaterial;
        private static readonly int FadeProgress = Shader.PropertyToID("_FadeProgress");
        
        private bool hasLoadedScene = false;

        private void LoadSceneOnce(string name)
        {
            if (hasLoadedScene) return;
            hasLoadedScene = true;
            SceneManager.LoadScene(name);
        }

        private void Start()
        {
            //PlayerPrefs.SetInt(BoardKey,4);

            boardErase.SetActive(false);
            boardProgress = PlayerPrefs.GetInt(BoardKey, 1);
            for (int i = 1; i <= gameObjects.Length; i++)
            {
                boards[i] = gameObjects[i - 1];
            }
        
            ShowUI();
            ShowBoard();
            // ȷ��SpriteRenderer����
            if (blackboardFade == null)
                blackboardFade = GetComponent<SpriteRenderer>();

            // ��������ʵ��
            fadeMaterial = new Material(blackboardFade.material);
            blackboardFade.material = fadeMaterial;

            // ��ʼ͸������Ϊ0
            SetFadeProgress(0f);
        }

        private void ShowBoard()
        {
            if (boardProgress == 3)
            {
                LoadSceneOnce("Riddle blackboard");
                return;
            }

            for (int i = 1; i <= gameObjects.Length; i++)
            {
                bool active = i == boardProgress || (i == 3 && boardProgress == 4);
                if (boards.TryGetValue(i, out var board))
                    board.SetActive(active);
            }
        }

        private void ShowUI()
        {
            switch (boardProgress)
            {
                case 1:
                    if (InfoDialogUI.Instance)
                    {
                        InfoDialogUI.Instance.ShowMessage("��ֵ����������Ҳ��һ�����룬�������˹ֲ�����ģ����������ɡ���");
                        InfoDialogUI.Instance.EnableCharacterBackground("����");
                        Debug.Log("InfoDialogUI is null");
                    }
                    break;
                case 2:
                    LoadSceneOnce("Riddle blackboard");
                    SetFadeProgress(1f);
                    break;
                case 3:
                    LoadSceneOnce("Riddle blackboard");
                    break;
                case 4:
                    boards[3].gameObject.SetActive(true);
                    break;
            }
        }



        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E) && canCa)
            {
                Fade();
            }
            if (boardProgress == 1&&!canCa)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    canCa = true;
                    InfoDialogUI.Instance.ShowMessage("�� <b>E</b> ����");
                    InfoDialogUI.Instance.EnableCharacterBackground("�԰�");
                }     
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (boardProgress==2)
                    PlayerPrefs.SetInt(BoardKey, 3);
                SceneManager.LoadScene("C1CJB");
            }
        }


        ///Shader Effect
        public void Fade()
        {
            boards[2].gameObject.SetActive(true);
            StartCoroutine(FadeCoroutine());
        }

        private IEnumerator FadeCoroutine()
        {
            float elapsedTime = 0f;
            SetFadeProgress(0f);

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / fadeDuration);
                SetFadeProgress(progress);
                boardErase.SetActive(true);
                yield return null;
            }

            SetFadeProgress(1f); // ȷ��������ȫ��ʾ
            PlayerPrefs.SetInt(BoardKey,2);
            SceneManager.LoadScene("C1CJB");
        }

        private void SetFadeProgress(float progress)
        {
            if (fadeMaterial != null)
                fadeMaterial.SetFloat(FadeProgress, progress);
        }

        // �����������ɴ��ⲿ����
        public void StartFade(float duration = 0f)
        {
            if (duration > 0)
                fadeDuration = duration;

            Fade();
        }

        void OnDestroy()
        {
            // �������Ĳ���ʵ��
            if (fadeMaterial != null)
                DestroyImmediate(fadeMaterial);
        }
    }
}
