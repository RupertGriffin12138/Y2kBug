using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class ScenePortal2D : MonoBehaviour
{
    [Header("Ŀ�곡��")]
    public string sceneName;

    [Header("��ʾ�ı�������ʾ�ڶԻ���")]
    [TextArea] public string hint = "�� <b>E</b> ����";

    [Header("����")]
    public string playerTag = "Player";

    [Header("��ʾ�����ֿ򣨿����գ�")]
    public string displayName = "";

    [Header("����ͣʱ��ֹ����")]
    public bool blockWhenPaused = true;

    private bool inside;
    private bool loading;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true; // �Զ���ѡ����
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        inside = true;

        if (InfoDialogUI.Instance)
        {
            if (!string.IsNullOrEmpty(displayName))
                InfoDialogUI.Instance.SetNameText(displayName);

            InfoDialogUI.Instance.ShowMessage(hint);
            InfoDialogUI.Instance.ShowArrow(); // �������С��ͷ����
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        inside = false;

        if (InfoDialogUI.Instance)
            InfoDialogUI.Instance.Clear(); // �ָ�Ĭ����ʾ�����ؼ�ͷ
    }

    void Update()
    {
        if (!inside || loading) return;

        // �������ͣ�˵��� Time.timeScale ��Ϊ 0��������ֱ������
        if (blockWhenPaused && Time.timeScale == 0f) return;

        if (Input.GetKeyDown(KeyCode.E))
            StartCoroutine(LoadRoutine());
    }

    System.Collections.IEnumerator LoadRoutine()
    {
        loading = true;

        if (InfoDialogUI.Instance)
        {
            InfoDialogUI.Instance.HideArrow();
            InfoDialogUI.Instance.ShowMessage("���ڽ��롭");
        }

        // �����������/������������ yield �ȴ�
        yield return null;

        SceneManager.LoadScene(sceneName);
    }

    // ���ӻ�������Χ��ѡ��ʱ��ʾ��
    void OnDrawGizmosSelected()
    {
        var col = GetComponent<Collider2D>();
        if (!col) return;

        // ����ԭ����
        Matrix4x4 prev = Gizmos.matrix;
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);

        // ʹ������ı任�������
        Gizmos.matrix = transform.localToWorldMatrix;

        // BoxCollider2D
        var box = col as BoxCollider2D;
        if (box)
        {
            // ע�⣺offset/size �Ǿֲ��ռ�
            Gizmos.DrawCube((Vector3)box.offset, (Vector3)box.size);
            Gizmos.matrix = prev;
            return;
        }

        // CircleCollider2D����ѡ��
        //var circle = col as CircleCollider2D;
        //if (circle)
        //{
        //    // �������һ��Բ�������
        //    Gizmos.DrawSphere((Vector3)circle.offset, circle.radius);
        //    Gizmos.matrix = prev;
        //    return;
        //}

        // ���� 2D ��ײ����԰������...

        // ��ԭ����
        Gizmos.matrix = prev;
    }
}
