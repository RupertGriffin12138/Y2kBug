using UnityEngine;
using UnityEngine.SceneManagement;

namespace Save
{
    public class PlayerSaveRestore : MonoBehaviour
    {
        [Tooltip("如果无效坐标则使用默认出生点")]
        public Transform fallbackSpawn;

        private void Start()
        {
            if (GameState.Current == null)
            {
                GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);
            }

            var data = GameState.Current;
            if (data == null) return;

            // 如果 lastScene 匹配当前场景，恢复位置
            if (data.lastScene == SceneManager.GetActiveScene().name)
            {
                Vector3 pos = new Vector3(data.playerX, data.playerY, transform.position.z);

                // 判断是否是有效坐标（排除 0,0）
                if (Mathf.Abs(pos.x) > 0.01f || Mathf.Abs(pos.y) > 0.01f)
                {
                    transform.position = pos;
                    Debug.Log($"[PlayerSaveRestore] 从存档恢复位置 {pos}");
                }
                else if (fallbackSpawn)
                {
                    transform.position = fallbackSpawn.position;
                    Debug.Log("[PlayerSaveRestore] 存档坐标无效，使用默认出生点");
                }
            }
            else if (fallbackSpawn)
            {
                transform.position = fallbackSpawn.position;
            }
        }
    }
}