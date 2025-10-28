using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Interact
{
    [RequireComponent(typeof(Collider2D))]
    public class ScenePortal2D : MonoBehaviour
    {
        [Header("本场景中此传送门的唯一ID（必须在场景内唯一）")]
        public string portalId = "A";

        [Header("要去的目标场景 & 该场景中的目标传送门ID")]
        public string nextSceneName = "";
        public string targetPortalIdInNextScene = "B";

        [Header("当『别人传送到我』时的到达设置（相对本物体/锚点）")]
        public Vector2 arrivalOffset = Vector2.zero;
        [Tooltip("是否覆盖到达时的Z值（2D一般不用）")]
        public bool overrideArrivalZ = false;
        public float arrivalZ = 0f;

        [Header("可选：固定锚点（当父级做视差/相机跟随时建议设置）")]
        public Transform anchorOverride;

        
        [Tooltip("勾选后仅在检测到 Parallax 脚本时才启用锚点；未检测到则用门本体位置")]
        public bool autoUseAnchorOnParallax = true;
        

        [Header("提示文本（会显示在对话框）")]
        [TextArea] public string hint = "按 <b>E</b> 进入";

        [Header("过滤")]
        public string playerTag = "Player";

        [Header("显示在名字框（可留空）")]
        public string displayName = "";

        [Header("在暂停时禁止触发")]
        public bool blockWhenPaused = true;
        
        [Header("是否可进入（条件开关）")]
        public bool canEnter;

        private bool inside;
        private bool loading;
        private bool doorLockedHintShown = false; // 是否已经显示过锁住提示

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true; // 自动勾选触发
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            inside = true;

            if (InfoDialogUI.Instance)
            {
                if (!string.IsNullOrEmpty(displayName))
                    InfoDialogUI.Instance.SetNameText(displayName);

                InfoDialogUI.Instance.ShowMessage(hint);
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
            if (!inside || loading) return;
            if (blockWhenPaused && Time.timeScale == 0f) return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!canEnter)
                {
                    if (InfoDialogUI.Instance)
                    {
                        if (!doorLockedHintShown)
                        {
                            InfoDialogUI.Instance.ShowMessage("- 门被锁上了 -");
                            doorLockedHintShown = true;
                        }
                        else
                        {
                            InfoDialogUI.Instance.ShowMessage("");
                            doorLockedHintShown = false;
                        }
                    }
                    return;
                }

                StartCoroutine(LoadRoutine());
            }
                
        }


        private IEnumerator LoadRoutine()
        {
            loading = true;

            if (InfoDialogUI.Instance)
            {
                InfoDialogUI.Instance.HideArrow();
                InfoDialogUI.Instance.ShowMessage("正在进入…");
            }

            // 将“目标场景中的目标传送门ID”写入一次性缓冲（不立即清空）
            PortalSpawnBuffer.SetTargetPortal(targetPortalIdInNextScene);

            yield return null;
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
    }

    /// <summary>
    /// 跨场景一次性目标传送门缓冲（不走存档）
    /// 支持“窥视”(Peek) 和 “消费”(Consume)——只有成功定位玩家后再消费。
    /// </summary>
    public static class PortalSpawnBuffer
    {
        private static bool hasPending;
        private static string targetPortalId;

        public static void SetTargetPortal(string portalId)
        {
            targetPortalId = portalId?.Trim();
            hasPending = !string.IsNullOrEmpty(targetPortalId);
        }

        public static bool Peek(out string portalId)
        {
            portalId = hasPending ? targetPortalId : null;
            return hasPending;
        }

        public static void Consume()
        {
            hasPending = false;
            targetPortalId = null;
        }
    }
}