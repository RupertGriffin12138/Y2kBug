using Characters.PLayer_25D;
using Characters.Player;
using Scene;
using UI;
using UnityEngine;

namespace Riddle.Abacus
{
    [RequireComponent(typeof(Collider2D))]
    public class AbacusTrigger : MonoBehaviour
    {
        [Header("目标场景名")]
        public string targetScene = "AbacusPuzzle";

        [Header("提示文本")]
        [TextArea] public string interactHint = "按 <b>E</b> 交互";

        [Header("过滤")]
        public string playerTag = "Player";

        private bool inside;
        private bool sceneLoaded;
        private PlayerMovement playerMovement;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void Start()
        {
            if (!InfoDialogUI.Instance)
                Debug.LogWarning("[AbacusTrigger2D] InfoDialogUI 未找到。");
            
            if (!playerMovement)
            {
                playerMovement = FindObjectOfType<PlayerMovement>();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            inside = true;
            sceneLoaded = false;

            // 播放对白
            if (InfoDialogUI.Instance)
            {
                InfoDialogUI.Instance.ShowMessage(interactHint);
            }
        }
        

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            inside = false;

            // 如果对白没结束就直接中断
            if (InfoDialogUI.Instance)
            {
                InfoDialogUI.Instance.Clear();
            }
        }

        private void Update()
        {
            if (!inside || sceneLoaded) return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                sceneLoaded = true;

                // 执行换场逻辑
                if (InfoDialogUI.Instance)
                {
                    InfoDialogUI.Instance.HideArrow();
                    InfoDialogUI.Instance.ShowMessage("正在进入…");
                }

                var fade = FindObjectOfType<SceneFadeEffect>();
                if (fade)
                    fade.FadeOutAndLoad(targetScene, 0.5f, 1f);
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
            }

            if (gameObject.name == "算盘1" && PlayerPrefs.GetInt("AbacusSolved1", 0) == 1)
            {
                gameObject.SetActive(false);
                playerMovement.transform.position = new Vector3(1.7f,2.62f);
            }
            if (gameObject.name == "算盘2" && PlayerPrefs.GetInt("AbacusSolved2", 0) == 1)
            {
                gameObject.SetActive(false);
                playerMovement.transform.position = new Vector3(4.22f,-1.3f);
            }
            if (gameObject.name == "算盘3" && PlayerPrefs.GetInt("AbacusSolved3", 0) == 1)
            {
                gameObject.SetActive(false);
                playerMovement.transform.position = new Vector3(9.38f,2.62f);
            }
            if (gameObject.name == "算盘4" && PlayerPrefs.GetInt("AbacusSolved4", 0) == 1)
            {
                gameObject.SetActive(false);
                playerMovement.transform.position = new Vector3(12.18f,0.54f);
            }
            
        }
    }
}
