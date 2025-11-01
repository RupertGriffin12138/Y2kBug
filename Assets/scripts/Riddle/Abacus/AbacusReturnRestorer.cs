using Characters.PLayer_25D;
using UnityEngine;
// 你的 PlayerMovement 在这里

namespace Riddle.Abacus
{
    public class AbacusReturnRestorer : MonoBehaviour
    {
        private void Start()
        {
            if (PlayerPrefs.GetInt("AbacusHasReturn", 0) == 1)
            {
                float x = PlayerPrefs.GetFloat("AbacusReturnX");
                float y = PlayerPrefs.GetFloat("AbacusReturnY");

                var player = FindObjectOfType<PlayerMovement>();
                if (player)
                    player.transform.position = new Vector3(x, y, player.transform.position.z);

                // 一次性用完清除标记
                PlayerPrefs.SetInt("AbacusHasReturn", 0);
                PlayerPrefs.Save();
            }
        }
    }
}