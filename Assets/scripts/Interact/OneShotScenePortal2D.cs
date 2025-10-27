using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class OneShotScenePortal2D : MonoBehaviour
{
    [Header("ID and Save")]
    public SaveTag tag;                 // 需保证 id 唯一且稳定
    public bool autoSaveOnUse = true;   // 使用后自动保存

    [Header("Scene Jump")]
    public string nextSceneName = "";   // 目标场景名
    public string playerTag = "Player";
    public string interactHint = "按 E 进入";
    public KeyCode interactKey = KeyCode.E;
    public bool blockWhenPaused = true;

    bool inside;
    bool used;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
        tag = GetComponent<SaveTag>();
    }

    void Start()
    {
        // 若该传送门已被标记为禁用，则不再出现
        if (tag && !string.IsNullOrEmpty(tag.id) && GameState.IsObjectDisabled(tag.id))
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;
        if (!other.CompareTag(playerTag)) return;
        inside = true;

        if (InfoDialogUI.Instance)
        {
            InfoDialogUI.Instance.SetNameText("");
            InfoDialogUI.Instance.ShowMessage(interactHint);
            InfoDialogUI.Instance.ShowArrow();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        inside = false;

        if (InfoDialogUI.Instance)
            InfoDialogUI.Instance.Clear();
    }

    void Update()
    {
        if (used) return;
        if (!inside) return;
        if (blockWhenPaused && Time.timeScale == 0f) return;

        if (Input.GetKeyDown(interactKey))
        {
            UsePortalOnce();
        }
    }

    void UsePortalOnce()
    {
        if (used) return;
        used = true;

        // 1) 记录禁用自身，确保下次进场景不再出现
        if (tag && !string.IsNullOrEmpty(tag.id))
        {
            GameState.AddDisabledObject(tag.id);
        }

        // 2) 自动保存
        if (autoSaveOnUse)
        {
            GameState.SaveNow();
        }

        // 3) 销毁自身
        Destroy(gameObject);

        // 4) 切换场景
        if (!string.IsNullOrWhiteSpace(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
    }
}
