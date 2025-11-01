using Characters.PLayer_25D;
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

            CheckSolvedStatus();
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
                
                //  根据触发器名字或自定义字段写入 ID
                string id = gameObject.name switch
                {
                    "算盘1" => "1",
                    "算盘2" => "2",
                    "算盘3" => "3",
                    "算盘4" => "4",
                    _ => "1"
                };
                
                // 对应回教室坐标（将来返回点）
                Vector3 backPos = id switch
                {
                    "1" => new Vector3(1.7f, 2.62f),
                    "2" => new Vector3(4.22f, -1.3f),
                    "3" => new Vector3(9.38f, 2.62f),
                    "4" => new Vector3(12.18f, 0.54f),
                    _ => Vector3.zero
                };
                
                AbacusBuffer.Set(id, backPos); // 写入缓冲器

                // 切换场景
                var fade = FindObjectOfType<SceneFadeEffect>();
                if (fade)
                    fade.FadeOutAndLoad(targetScene, 0.5f, 1f);
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
            }

            // 隐藏已完成的算盘
            CheckSolvedStatus();
        }
        
        private void CheckSolvedStatus()
        {
            string id = gameObject.name switch
            {
                "算盘1" => "1",
                "算盘2" => "2",
                "算盘3" => "3",
                "算盘4" => "4",
                _ => "1"
            };

            if (PlayerPrefs.GetInt($"AbacusSolved_{id}", 0) == 1)
            {
                Debug.Log($"[AbacusTrigger] 算盘{id} 已解锁，自动隐藏。");
                gameObject.SetActive(false);
            }
        }
    }
}
