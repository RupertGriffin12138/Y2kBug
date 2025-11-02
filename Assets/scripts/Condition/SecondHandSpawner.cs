using Save;
using UnityEngine;

namespace Condition
{
    /// <summary>
    /// 秒针专用生成/销毁控制器：
    /// 当满足前置条件时生成秒针；
    /// 当满足任意销毁条件时永久删除秒针。
    /// </summary>
    public class SecondHandSpawner : MonoBehaviour
    {
        [Header("要控制的秒针物体")] public GameObject secondHandObject;

        [Header("前置激活条件")] [Tooltip("需要拥有的文档 teach")]
        public bool requireTeachDoc = true;

        [Tooltip("需要拥有的文档 diary2")] public bool requireDiary2Doc = true;
        [Tooltip("是否要求背包已解锁")] public bool requireBackpackUnlocked = true;

        [Header("销毁条件")] [Tooltip("对白 scene_dlg_001 已看过")]
        public bool destroyAfterDialogueSeen = true;

        [Tooltip("PlayerPrefs 条件：Clock_2_Seen")]
        public bool destroyIfClock2Seen = true;

        [Tooltip("PlayerPrefs 条件：Clock_ReturnSpawn_Valid")]
        public bool destroyIfReturnSpawnValid = true;

        [Header("高级选项")] [Tooltip("启用实时检测（条件变化时自动刷新）")]
        public bool enableRealtimeCheck = true;

        [Tooltip("检测间隔（秒）")] public float checkInterval = 1.0f;

        private bool hasSpawned;
        private bool hasDestroyed;

        private void Start()
        {
            if (!secondHandObject)
            {
                Debug.LogWarning("[SecondHandSpawner] 未绑定秒针物体引用！", this);
                return;
            }

            // 默认先隐藏
            secondHandObject.SetActive(false);

            // 初始检查
            CheckAndApply();

            if (enableRealtimeCheck)
                InvokeRepeating(nameof(CheckAndApply), checkInterval, checkInterval);
        }

        private void CheckAndApply()
        {
            if (!secondHandObject)
            {
                Debug.LogWarning("[SecondHandSpawner] 秒针物体引用已丢失。", this);
                CancelInvoke(nameof(CheckAndApply));
                Destroy(this);
                return;
            }

            // 如果已经销毁过，则跳过
            if (hasDestroyed)
                return;

            // 检查销毁条件（优先）
            if (ShouldDestroy())
            {
                Destroy(secondHandObject);
                hasDestroyed = true;
                Debug.Log("[SecondHandSpawner] 已满足销毁条件，秒针物体永久删除。");
                CancelInvoke(nameof(CheckAndApply));
                return;
            }

            // 检查生成条件
            if (!hasSpawned && ShouldSpawn())
            {
                secondHandObject.SetActive(true);
                hasSpawned = true;
                Debug.Log("[SecondHandSpawner] 秒针掉落物激活成功！");
            }
        }

        /// <summary>
        /// 是否满足生成条件（全部满足）
        /// </summary>
        private bool ShouldSpawn()
        {
            bool teachOk = !requireTeachDoc || GameState.HasCollectedDoc("teach");
            bool diaryOk = !requireDiary2Doc || GameState.HasCollectedDoc("diary2");
            bool backpackOk = !requireBackpackUnlocked || GameState.BackpackUnlocked;

            return teachOk && diaryOk && backpackOk;
        }

        /// <summary>
        /// 是否满足销毁条件（任意满足）
        /// </summary>
        private bool ShouldDestroy()
        {
            if (destroyAfterDialogueSeen && GameState.HasSeenDialogue("scene_dlg_001"))
                return true;
            if (destroyIfClock2Seen && PlayerPrefs.GetInt("Clock_2_Seen", 0) == 1)
                return true;
            if (destroyIfReturnSpawnValid && PlayerPrefs.GetInt("Clock_ReturnSpawn_Valid", 0) == 1)
                return true;

            return false;
        }
    }
}