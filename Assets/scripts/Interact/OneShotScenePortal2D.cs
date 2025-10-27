using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class OneShotScenePortal2D : MonoBehaviour
{
    [Header("ID and Save")]
    public SaveTag tag;                 // �豣֤ id Ψһ���ȶ�
    public bool autoSaveOnUse = true;   // ʹ�ú��Զ�����

    [Header("Scene Jump")]
    public string nextSceneName = "";   // Ŀ�곡����
    public string playerTag = "Player";
    public string interactHint = "�� E ����";
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
        // ���ô������ѱ����Ϊ���ã����ٳ���
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

        // 1) ��¼��������ȷ���´ν��������ٳ���
        if (tag && !string.IsNullOrEmpty(tag.id))
        {
            GameState.AddDisabledObject(tag.id);
        }

        // 2) �Զ�����
        if (autoSaveOnUse)
        {
            GameState.SaveNow();
        }

        // 3) ��������
        Destroy(gameObject);

        // 4) �л�����
        if (!string.IsNullOrWhiteSpace(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
    }
}
