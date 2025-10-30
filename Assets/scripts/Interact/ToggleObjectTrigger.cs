using Characters.Player;
using UI;
using UnityEngine;

namespace Interact
{
    /// <summary>
    /// 靠近显示提示，按 E 显隐目标物体。
    /// 可自由离开，不会强制触发。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ToggleObjectTrigger : MonoBehaviour
    {
        [Header("目标物体（要控制显隐的对象）")]
        public GameObject targetObject;

        [Header("提示文本")]
        [TextArea] public string interactHint = "按 <b>E</b> 交互";

        [Header("过滤")]
        public string playerTag = "Player";

        private bool inside;
        private bool toggled;

        private Player player;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void Start()
        {
            player = FindObjectOfType<Player>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            inside = true;

            if (InfoDialogUI.Instance)
            {
                InfoDialogUI.Instance.ShowMessage(interactHint);
                InfoDialogUI.Instance.ShowArrow();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            inside = false;

            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.Clear();
        }

        private void Update()
        {
            if (!inside) return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (targetObject)
                    targetObject.SetActive(true);
                player.LockControl();
            }

            if (!targetObject.activeSelf)
            {
                player.UnlockControl();
            }
        }
    }
}