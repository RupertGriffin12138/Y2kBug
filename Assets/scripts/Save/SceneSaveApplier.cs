using Interact;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Save
{
    public class SceneSaveApplier : MonoBehaviour
    {
        [Header("可选：读档后定位玩家")]
        public Transform player;          // 你的玩家对象（2D）
        public bool applyPlayerPos = false;

        void Start()
        {
            // 1) 确保内存态有数据（没存档时会给默认）
            if (GameState.Current == null)
                GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

            // 2) 记录当前场景名（用于“保存”时回到这一关）
            var curScene = SceneManager.GetActiveScene().name;
            GameState.Current.lastScene = curScene;

            // 3) 隐藏/禁用 已在存档中标记的一次性对象
            var tags = FindObjectsByType<SaveTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in tags)
            {
                if (!string.IsNullOrEmpty(t.id) && GameState.IsObjectDisabled(t.id))
                {
                    t.gameObject.SetActive(false);
                }
            }

            // 4) 可选：把玩家放到存档里记录的位置
            if (applyPlayerPos && player != null)
            {
                player.position = new Vector3(GameState.Current.playerX, GameState.Current.playerY, player.position.z);
            }
        }
    }
}
