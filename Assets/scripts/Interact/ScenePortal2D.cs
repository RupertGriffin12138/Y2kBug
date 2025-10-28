using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Interact
{
    [RequireComponent(typeof(Collider2D))]
    public class ScenePortal2D : MonoBehaviour
    {
        [Header("�������д˴����ŵ�ΨһID�������ڳ�����Ψһ��")]
        public string portalId = "A";

        [Header("Ҫȥ��Ŀ�곡�� & �ó����е�Ŀ�괫����ID")]
        public string nextSceneName = "";
        public string targetPortalIdInNextScene = "B";

        [Header("�������˴��͵��ҡ�ʱ�ĵ������ã���Ա�����/ê�㣩")]
        public Vector2 arrivalOffset = Vector2.zero;
        [Tooltip("�Ƿ񸲸ǵ���ʱ��Zֵ��2Dһ�㲻�ã�")]
        public bool overrideArrivalZ = false;
        public float arrivalZ = 0f;

        [Header("��ѡ���̶�ê�㣨���������Ӳ�/�������ʱ�������ã�")]
        public Transform anchorOverride;

        
        [Tooltip("��ѡ����ڼ�⵽ Parallax �ű�ʱ������ê�㣻δ��⵽�����ű���λ��")]
        public bool autoUseAnchorOnParallax = true;
        

        [Header("��ʾ�ı�������ʾ�ڶԻ���")]
        [TextArea] public string hint = "�� <b>E</b> ����";

        [Header("����")]
        public string playerTag = "Player";

        [Header("��ʾ�����ֿ򣨿����գ�")]
        public string displayName = "";

        [Header("����ͣʱ��ֹ����")]
        public bool blockWhenPaused = true;
        
        [Header("�Ƿ�ɽ��루�������أ�")]
        public bool canEnter;

        private bool inside;
        private bool loading;
        private bool doorLockedHintShown = false; // �Ƿ��Ѿ���ʾ����ס��ʾ

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true; // �Զ���ѡ����
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            inside = true;

            if (InfoDialogUI.Instance)
            {
                if (!string.IsNullOrEmpty(displayName))
                    InfoDialogUI.Instance.SetNameText(displayName);

                InfoDialogUI.Instance.ShowMessage(hint);
                InfoDialogUI.Instance.ShowArrow();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            inside = false;

            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.Clear();
        }


        private void Update()
        {
            if (!inside || loading) return;
            if (blockWhenPaused && Time.timeScale == 0f) return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!canEnter)
                {
                    if (InfoDialogUI.Instance)
                    {
                        if (!doorLockedHintShown)
                        {
                            InfoDialogUI.Instance.ShowMessage("- �ű������� -");
                            doorLockedHintShown = true;
                        }
                        else
                        {
                            InfoDialogUI.Instance.ShowMessage("");
                            doorLockedHintShown = false;
                        }
                    }
                    return;
                }

                StartCoroutine(LoadRoutine());
            }
                
        }


        private IEnumerator LoadRoutine()
        {
            loading = true;

            if (InfoDialogUI.Instance)
            {
                InfoDialogUI.Instance.HideArrow();
                InfoDialogUI.Instance.ShowMessage("���ڽ��롭");
            }

            // ����Ŀ�곡���е�Ŀ�괫����ID��д��һ���Ի��壨��������գ�
            PortalSpawnBuffer.SetTargetPortal(targetPortalIdInNextScene);

            yield return null;
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
    }

    /// <summary>
    /// �糡��һ����Ŀ�괫���Ż��壨���ߴ浵��
    /// ֧�֡����ӡ�(Peek) �� �����ѡ�(Consume)����ֻ�гɹ���λ��Һ������ѡ�
    /// </summary>
    public static class PortalSpawnBuffer
    {
        private static bool hasPending;
        private static string targetPortalId;

        public static void SetTargetPortal(string portalId)
        {
            targetPortalId = portalId?.Trim();
            hasPending = !string.IsNullOrEmpty(targetPortalId);
        }

        public static bool Peek(out string portalId)
        {
            portalId = hasPending ? targetPortalId : null;
            return hasPending;
        }

        public static void Consume()
        {
            hasPending = false;
            targetPortalId = null;
        }
    }
}