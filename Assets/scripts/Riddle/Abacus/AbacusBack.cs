using System;
using Items;
using Scene;
using UnityEngine;

namespace Riddle.Abacus
{
    public class AbacusBack : MonoBehaviour
    {
        [Header("返回的目标场景")]
        public string backScene = "C1CJC";

        [Header("回到教室后应出现的位置")]
        public Vector3 returnPosition = new Vector3(1.7f, 2.6f, 0f);
        
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
                ReturnToClassroom(returnPosition);
            }
        }

        /// <summary>
        /// 公开方法：写入返回坐标并切回教室场景。
        /// 可供 Bead_1 等脚本直接调用。
        /// </summary>
        public void ReturnToClassroom(Vector3 returnPosition)
        {
            // --- 写入返回信息 ---
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
