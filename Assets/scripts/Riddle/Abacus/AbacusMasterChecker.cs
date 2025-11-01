using Audio;
using Save;
using UnityEngine;

namespace Riddle.Abacus
{
    /// <summary>
    /// 检查四个算盘谜题是否全部解开；
    /// 若全解开，则激活目标物体。
    /// </summary>
    public class AbacusMasterChecker : MonoBehaviour
    {
        [Header("目标对象（默认关闭）")]
        public GameObject targetObject;

        [Header("检查频率（秒）")]
        public float checkInterval = 1f;

        private void Start()
        {
            if (targetObject)
                targetObject.SetActive(false); // 默认关闭

            // 立即检测一次
            CheckAllSolved();

            // 定期检测（防止其他脚本稍后才写入 PlayerPrefs）
            InvokeRepeating(nameof(CheckAllSolved), checkInterval, checkInterval);
        }

        private void CheckAllSolved()
        {
            // 从 PlayerPrefs 读取四个标志（默认为 0）
            int s1 = PlayerPrefs.GetInt("AbacusSolved_1", 0);
            int s2 = PlayerPrefs.GetInt("AbacusSolved_2", 0);
            int s3 = PlayerPrefs.GetInt("AbacusSolved_3", 0);
            int s4 = PlayerPrefs.GetInt("AbacusSolved_4", 0);

            bool allSolved = (s1 == 1 && s2 == 1 && s3 == 1 && s4 == 1);

            if (allSolved && targetObject && !targetObject.activeSelf && !GameState.HasReadDoc("teach"))
            {
                // --- 防止重复播放（永久） ---
                if (PlayerPrefs.GetInt("AbacusMaster_WoodenPlayed", 0) == 0)
                {
                    Debug.Log("[AbacusMasterChecker] 全部算盘谜题已解开，激活目标对象并播放音效！");
                    AudioClipHelper.Instance.Play_WoodenStructure();
                    PlayerPrefs.SetInt("AbacusMaster_WoodenPlayed", 1); // 标记已播放
                    PlayerPrefs.Save();
                }
                targetObject.SetActive(true);
            }

            if (GameState.HasReadDoc("teach"))
            {
                targetObject.SetActive(false);
            }
        }
    }
}