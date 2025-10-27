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

        // === �ؼ���������һ����ǰ����¼��ǰ������������� ===
        StorePlayerPosOfCurrentScene();

        yield return null;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    void StorePlayerPosOfCurrentScene()
    {
        // �ҵ����
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (!player) return;

        Vector3 pos = player.transform.position;

        // ����浵��д��
        var currentScene = SceneManager.GetActiveScene().name;
        var save = SaveManager.LoadOrDefault(currentScene);
        save.SetPlayerPos(currentScene, pos);
        SaveManager.Save(save);
    }

    // ���ӻ�������Χ��ѡ��ʱ��ʾ��
    void OnDrawGizmosSelected()
    {
        var col = GetComponent<Collider2D>();
        if (!col) return;

        var prev = Gizmos.matrix;
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider2D box)
            Gizmos.DrawCube((Vector3)box.offset, (Vector3)box.size);

        Gizmos.matrix = prev;
    }
}
