using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class PickupBackpack2D : MonoBehaviour
{
    [Header("拾取设置")]
    public KeyCode pickupKey = KeyCode.E;
    public bool destroyOnPickup = true;

    [Header("提示文本")]
    [TextArea] public string promptString = "按 <b>E</b> 拾取背包";
    [TextArea] public string obtainedString = "获得 背包（已解锁全部格子）";

    public InventoryLite inventory;

    [Header("可选音效")]
    public AudioSource sfxSource;
    public AudioClip pickupSfx;

    bool _inRange;

    // --- 展示期抑制 Exit 清空 ---
    bool _pickedUp = false;
    float _suppressExitClearUntil = 0f;
    const float kMessageHold = 2f;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void Awake()
    {
        if (!inventory) inventory = FindObjectOfType<InventoryLite>();
        // 不再在这里做任何 Load（入口统一做了）
    }

    void OnEnable()
    {
        // 如果已经解锁，进入场景即隐藏
        if (GameState.BackpackUnlocked)
        {
            gameObject.SetActive(false);
            enabled = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _inRange = true;
        if (!GameState.BackpackUnlocked)
            InfoDialogUI.Instance?.ShowMessage(promptString);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _inRange = false;
        if (_pickedUp && Time.time < _suppressExitClearUntil) return;
        InfoDialogUI.Instance?.Clear();
    }

    void Update()
    {
        if (_inRange && Input.GetKeyDown(pickupKey))
            TryPickup();
    }

    void TryPickup()
    {
        if (GameState.BackpackUnlocked)
        {
            InfoDialogUI.Instance?.ShowMessage("背包已解锁");
            InfoDialogUI.Instance?.CancelInvoke(nameof(InfoDialogUI.Clear));
            InfoDialogUI.Instance?.Invoke(nameof(InfoDialogUI.Clear), 1.2f);
            return;
        }

        // 首次解锁 -> 写入存档
        GameState.UnlockBackpack(true);

        InfoDialogUI.Instance?.ShowMessage(obtainedString);
        InfoDialogUI.Instance?.CancelInvoke(nameof(InfoDialogUI.Clear));
        InfoDialogUI.Instance?.Invoke(nameof(InfoDialogUI.Clear), kMessageHold);

        if (sfxSource && pickupSfx) sfxSource.PlayOneShot(pickupSfx);
        inventory?.NotifyCapacityChanged();

        _pickedUp = true;
        _suppressExitClearUntil = Time.time + kMessageHold;

        if (destroyOnPickup) StartCoroutine(DestroyAfter(0.05f));
        else gameObject.SetActive(false);
    }

    IEnumerator DestroyAfter(float delay)
    {
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
