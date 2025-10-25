using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PickupBackpack2D : MonoBehaviour
{
    [Header("拾取设置")]
    public KeyCode pickupKey = KeyCode.E;
    public bool destroyOnPickup = true;

    [Header("提示文本")]
    [TextArea] public string promptString = "按 <b>E</b> 拾取背包";
    [TextArea] public string obtainedString = "获得 背包（已解锁全部格子）";

    [Header("引用")]
    public GameProgress progress;
    public InventoryLite inventory;

    [Header("可选音效")]
    public AudioSource sfxSource;
    public AudioClip pickupSfx;

    bool _inRange;

    // --- 新增：用于抑制 OnTriggerExit 清空 ---
    bool _pickedUp = false;                      // [MOD]
    float _suppressExitClearUntil = 0f;          // [MOD]
    const float kMessageHold = 2f;               // [MOD] “获得背包”显示时长（与你的 Invoke 保持一致）

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void Start()
    {
        if (!inventory) inventory = FindObjectOfType<InventoryLite>();
        // （建议全局只在一个地方 Load，这里不再 Load）
        // if (progress) progress.Load();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _inRange = true;
        InfoDialogUI.Instance?.ShowMessage(promptString);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _inRange = false;

        // [MOD] 若刚刚完成拾取，且仍处于“提示展示期”，则不要清空
        if (_pickedUp && Time.time < _suppressExitClearUntil)
            return;

        InfoDialogUI.Instance?.Clear();
    }

    void Update()
    {
        if (_inRange && Input.GetKeyDown(pickupKey))
            TryPickup();
    }

    void TryPickup()
    {
        if (!progress)
        {
            Debug.LogWarning("[PickupBackpack2D] 未绑定 GameProgress", this);
            return;
        }

        if (!progress.backpackUnlocked)
        {
            progress.UnlockBackpack(true);

            // 显示“获得背包”，并安排自动清除
            InfoDialogUI.Instance?.ShowMessage(obtainedString);
            InfoDialogUI.Instance?.CancelInvoke(nameof(InfoDialogUI.Clear));
            InfoDialogUI.Instance?.Invoke(nameof(InfoDialogUI.Clear), kMessageHold);

            if (sfxSource && pickupSfx) sfxSource.PlayOneShot(pickupSfx);

            inventory?.NotifyCapacityChanged();

            // [MOD] 标记为已拾取 & 在展示期内屏蔽 Exit 清空
            _pickedUp = true;
            _suppressExitClearUntil = Time.time + kMessageHold;

            // [MOD] 轻微延迟后再销毁，确保不会同帧触发 Exit 清空
            if (destroyOnPickup)
                StartCoroutine(DestroyAfter(0.05f));
            else
                gameObject.SetActive(false);
        }
        else
        {
            InfoDialogUI.Instance?.ShowMessage("背包已解锁");
            InfoDialogUI.Instance?.CancelInvoke(nameof(InfoDialogUI.Clear));
            InfoDialogUI.Instance?.Invoke(nameof(InfoDialogUI.Clear), 1.5f);
        }
    }

    System.Collections.IEnumerator DestroyAfter(float delay)   // [MOD]
    {
        // 可选：先禁用碰撞，避免立刻触发 Exit
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
