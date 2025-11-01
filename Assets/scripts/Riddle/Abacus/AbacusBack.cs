using Items;
using Scene;
using UnityEngine;

namespace Riddle.Abacus
{
    public class AbacusBack : MonoBehaviour
    {
        [Header("返回的目标场景")]
        public string backScene = "C1CJC";
        
        private SceneFadeEffect sceneFadeEffect;
        private DocReaderPanel docReaderPanel;

        private void Start()
        {
            sceneFadeEffect = FindObjectOfType<SceneFadeEffect>(true);
            docReaderPanel = FindObjectOfType<DocReaderPanel>(true);
        }

        private void Update()
        {
            // 按下 ESC 返回（关闭文档界面时）
            if (Input.GetKeyDown(KeyCode.Escape) && (!docReaderPanel || !docReaderPanel.gameObject.activeSelf))
            {
                // ✅ 不用 Inspector 里的 returnPosition，直接从缓存取
                ReturnToClassroom(AbacusBuffer.returnPosition);
            }
        }

        /// <summary>
        /// 公开方法：写入返回坐标并切回教室场景。
        /// 可供 BeadPuzzle 等脚本直接调用。
        /// </summary>
        public void ReturnToClassroom(Vector3 returnPosition)
        {
            // --- 写入返回信息到 PlayerPrefs（方便教室读取） ---
            PlayerPrefs.SetFloat("AbacusReturnX", returnPosition.x);
            PlayerPrefs.SetFloat("AbacusReturnY", returnPosition.y);
            PlayerPrefs.SetInt("AbacusHasReturn", 1);
            PlayerPrefs.Save();

            // --- 切换场景 ---
            if (sceneFadeEffect)
                sceneFadeEffect.FadeOutAndLoad(backScene, 0.5f, 1f);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(backScene);
        }
    }
}