using System.Collections;
using Interact;
using Scene;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Save
{
    public class PlayerSceneRestore : MonoBehaviour
    {
        [Tooltip("���û�л���Ŀ����Ҳ�����Ӧ�����ţ����������ѡ������")]
        public Transform fallbackSpawnPoint;

        [Header("��λ����")] [Tooltip("���ȴ�����֡�Եȳ��������ʼ����0 ��ʾ���ȴ���")]
        private const int maxWaitFrames = 90;

        [Tooltip("����δ�ҵ����ٶ���ȴ������루���� Addressables/�첽�����ʱ��")]
        public float extraWaitSeconds = 0f;

        private void Start()
        {
            StartCoroutine(PlaceAtTargetPortalCoroutine());
        }

        private IEnumerator PlaceAtTargetPortalCoroutine()
        {
            if (!PortalSpawnBuffer.Peek(out string targetPortalId) || string.IsNullOrEmpty(targetPortalId))
            {
                UseFallbackIfAny();
                yield break;
            }

            for (int i = 0; i < maxWaitFrames; i++)
            {
                if (TryPlaceAtPortal(targetPortalId))
                {
                    PortalSpawnBuffer.Consume();
                    yield break;
                }
                yield return null;
            }

            if (extraWaitSeconds > 0f)
            {
                float timer = 0f;
                while (timer < extraWaitSeconds)
                {
                    if (TryPlaceAtPortal(targetPortalId))
                    {
                        PortalSpawnBuffer.Consume();
                        yield break;
                    }
                    timer += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            UseFallbackIfAny();
        }

        private bool TryPlaceAtPortal(string targetPortalId)
        {
#if UNITY_2023_1_OR_NEWER
        var portals = FindObjectsByType<ScenePortal2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var portals = FindObjectsOfType<ScenePortal2D>(true);
#endif
            foreach (var p in portals)
            {
                if (!p) continue;
                if (!string.Equals(p.portalId?.Trim(), targetPortalId.Trim(), System.StringComparison.Ordinal))
                    continue;

                
                // ���� �Ƿ��⵽ Parallax������Ϊ ParallaxBackground �Ľű���������
                bool hasParallax =
                    p.GetComponent<ParallaxBackground>() ||
                    (p.transform.parent && p.transform.parent.GetComponent<ParallaxBackground>());

                // ���� ѡ���׼����ê�� &&��δҪ���Զ���� || ��⵽ Parallax������
                Transform basis = (p.anchorOverride && (hasParallax))
                    ? p.anchorOverride
                    : p.transform;

                // ���� ��������������㣺ʹ�� TransformPoint �Կ�����ת/���� ���� 
                Vector3 arrive = basis.TransformPoint((Vector3)p.arrivalOffset);

                if (TryGetComponent<Rigidbody2D>(out var rb2d))
                {
                    rb2d.position = new Vector2(arrive.x, arrive.y);
                    rb2d.velocity = Vector2.zero;
                    rb2d.angularVelocity = 0f;
                }
                else if (TryGetComponent<Rigidbody>(out var rb))
                {
                    float z = p.overrideArrivalZ ? p.arrivalZ : arrive.z;
                    rb.position = new Vector3(arrive.x, arrive.y, z);
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                else
                {
                    float z = p.overrideArrivalZ ? p.arrivalZ : transform.position.z;
                    transform.position = new Vector3(arrive.x, arrive.y, z);
                }
                return true;
            }
            return false;
        }

        private void UseFallbackIfAny()
        {
            if (fallbackSpawnPoint)
            {
                transform.position = fallbackSpawnPoint.position;
                return;
            }
                
            
            if (GameState.Current != null)
            {
                var data = GameState.Current;
                if (data.lastScene == SceneManager.GetActiveScene().name && data.playerX != 0 && data.playerY != 0)
                {
                    if (SceneManager.GetActiveScene().name != "C1CJB")
                    {
                        transform.position = new Vector3(data.playerX, data.playerY, transform.position.z);
                        Debug.Log($"[PlayerSceneRestore] �Ӵ浵�ָ����λ�� {transform.position}");
                    }
                }
            }
        }
    }
}
