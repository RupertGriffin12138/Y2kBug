using System.Collections;
using UnityEngine;

namespace Save
{
    public class ReturnSpawnHandler : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(DelayedRestore());
        }

        private IEnumerator DelayedRestore()
        {
            // 等待 0.1 秒，保证其他脚本（比如玩家控制器、出生点恢复）先执行
            yield return new WaitForSeconds(0.1f);

            if (PlayerPrefs.GetInt("Clock_ReturnSpawn_Valid", 0) == 1)
            {
                float x = PlayerPrefs.GetFloat("Clock_ReturnSpawn_X", 0);
                float y = PlayerPrefs.GetFloat("Clock_ReturnSpawn_Y", 0);

                var player = GameObject.FindWithTag("Player");
                if (player)
                {
                    var parent = player.transform.parent;
                    if (parent)
                    {
                        // 有父物体 → 使用局部坐标（防止父层偏移叠加）
                        player.transform.localPosition = new Vector3(x, y, player.transform.localPosition.z);
                        Debug.Log($"[ReturnSpawnHandler] 玩家有父物体，已设置局部坐标 ({x}, {y})");
                    }
                    else
                    {
                        // 无父物体 → 使用世界坐标
                        player.transform.position = new Vector3(x, y, player.transform.position.z);
                        Debug.Log($"[ReturnSpawnHandler] 玩家无父物体，已设置世界坐标 ({x}, {y})");
                    }
                }

                // 用完即清除，防止影响后续进入
                PlayerPrefs.SetInt("Clock_ReturnSpawn_Valid", 0);
                PlayerPrefs.Save();
            }
        }
    }
}