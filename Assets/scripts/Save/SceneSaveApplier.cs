using Interact;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Save
{
    public class SceneSaveApplier : MonoBehaviour
    {
        [Header("��ѡ��������λ���")]
        public Transform player;          // �����Ҷ���2D��
        public bool applyPlayerPos = false;

        void Start()
        {
            // 1) ȷ���ڴ�̬�����ݣ�û�浵ʱ���Ĭ�ϣ�
            if (GameState.Current == null)
                GameState.LoadGameOrNew(SceneManager.GetActiveScene().name);

            // 2) ��¼��ǰ�����������ڡ����桱ʱ�ص���һ�أ�
            var curScene = SceneManager.GetActiveScene().name;
            GameState.Current.lastScene = curScene;

            // 3) ����/���� ���ڴ浵�б�ǵ�һ���Զ���
            var tags = FindObjectsByType<SaveTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in tags)
            {
                if (!string.IsNullOrEmpty(t.id) && GameState.IsObjectDisabled(t.id))
                {
                    t.gameObject.SetActive(false);
                }
            }

            // 4) ��ѡ������ҷŵ��浵���¼��λ��
            if (applyPlayerPos && player != null)
            {
                player.position = new Vector3(GameState.Current.playerX, GameState.Current.playerY, player.position.z);
            }
        }
    }
}
