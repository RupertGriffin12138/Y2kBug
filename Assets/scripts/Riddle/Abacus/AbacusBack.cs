using System;
using Items;
using Scene;
using UnityEngine;

namespace Riddle.Abacus
{
    public class AbacusBack : MonoBehaviour
    {
        [Header("���ص�Ŀ�곡��")]
        public string backScene = "C1CJC";

        [Header("�ص����Һ�Ӧ���ֵ�λ��")]
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
            // ���� ESC ���أ��ر��ĵ�����ʱ��
            if (Input.GetKeyDown(KeyCode.Escape) && (!docReaderPanel || !docReaderPanel.gameObject.activeSelf))
            {
                ReturnToClassroom(returnPosition);
            }
        }

        /// <summary>
        /// ����������д�뷵�����겢�лؽ��ҳ�����
        /// �ɹ� Bead_1 �Ƚű�ֱ�ӵ��á�
        /// </summary>
        public void ReturnToClassroom(Vector3 returnPosition)
        {
            // --- д�뷵����Ϣ ---
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
