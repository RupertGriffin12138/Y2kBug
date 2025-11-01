using Items;
using Scene;
using UnityEngine;

namespace Riddle.Abacus
{
    public class AbacusBack : MonoBehaviour
    {
        [Header("���ص�Ŀ�곡��")]
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
            // ���� ESC ���أ��ر��ĵ�����ʱ��
            if (Input.GetKeyDown(KeyCode.Escape) && (!docReaderPanel || !docReaderPanel.gameObject.activeSelf))
            {
                // �7�3 ���� Inspector ��� returnPosition��ֱ�Ӵӻ���ȡ
                ReturnToClassroom(AbacusBuffer.returnPosition);
            }
        }

        /// <summary>
        /// ����������д�뷵�����겢�лؽ��ҳ�����
        /// �ɹ� BeadPuzzle �Ƚű�ֱ�ӵ��á�
        /// </summary>
        public void ReturnToClassroom(Vector3 returnPosition)
        {
            // --- д�뷵����Ϣ�� PlayerPrefs��������Ҷ�ȡ�� ---
            PlayerPrefs.SetFloat("AbacusReturnX", returnPosition.x);
            PlayerPrefs.SetFloat("AbacusReturnY", returnPosition.y);
            PlayerPrefs.SetInt("AbacusHasReturn", 1);
            PlayerPrefs.Save();

            // --- �л����� ---
            if (sceneFadeEffect)
                sceneFadeEffect.FadeOutAndLoad(backScene, 0.5f, 1f);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(backScene);
        }
    }
}