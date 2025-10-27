using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class ScenePortal2D : MonoBehaviour
{
    [Header("目标场景")]
    public string sceneName;

    [Header("提示文本（会显示在对话框）")]
    [TextArea] public string hint = "按 <b>E</b> 进入";

    [Header("过滤")]
    public string playerTag = "Player";

    [Header("显示在名字框（可留空）")]
    public string displayName = "";

    [Header("在暂停时禁止触发")]
    public bool blockWhenPaused = true;

    private bool inside;
    private bool loading;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true; // 自动勾选触发
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
            InfoDialogUI.Instance.ShowArrow(); // 播放你的小箭头动画
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        inside = false;

        if (InfoDialogUI.Instance)
            InfoDialogUI.Instance.Clear(); // 恢复默认提示并隐藏箭头
    }

    void Update()
    {
        if (!inside || loading) return;

        // 若你的暂停菜单把 Time.timeScale 设为 0，则这里直接拦截
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
            InfoDialogUI.Instance.ShowMessage("正在进入…");
        }

        // === 关键：进入下一场景前，记录当前场景的玩家坐标 ===
        StorePlayerPosOfCurrentScene();

        yield return null;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    void StorePlayerPosOfCurrentScene()
    {
        // 找到玩家
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (!player) return;

        Vector3 pos = player.transform.position;

        // 载入存档并写入
        var currentScene = SceneManager.GetActiveScene().name;
        var save = SaveManager.LoadOrDefault(currentScene);
        save.SetPlayerPos(currentScene, pos);
        SaveManager.Save(save);
    }

    // 可视化触发范围（选中时显示）
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
