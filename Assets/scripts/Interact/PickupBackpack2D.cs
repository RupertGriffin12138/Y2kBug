using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PickupBackpack2D : MonoBehaviour
{
    [Header("ʰȡ����")]
    public KeyCode pickupKey = KeyCode.E;
    public bool destroyOnPickup = true;

    [Header("��ʾ�ı�")]
    [TextArea] public string promptString = "�� <b>E</b> ʰȡ����";
    [TextArea] public string obtainedString = "��� �������ѽ���ȫ�����ӣ�";

    [Header("����")]
    public GameProgress progress;
    public InventoryLite inventory;

    [Header("��ѡ��Ч")]
    public AudioSource sfxSource;
    public AudioClip pickupSfx;

    bool _inRange;

    // --- �������������� OnTriggerExit ��� ---
    bool _pickedUp = false;                      // [MOD]
    float _suppressExitClearUntil = 0f;          // [MOD]
    const float kMessageHold = 2f;               // [MOD] ����ñ�������ʾʱ��������� Invoke ����һ�£�

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void Start()
    {
        if (!inventory) inventory = FindObjectOfType<InventoryLite>();
        // ������ȫ��ֻ��һ���ط� Load�����ﲻ�� Load��
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

        // [MOD] ���ո����ʰȡ�����Դ��ڡ���ʾչʾ�ڡ�����Ҫ���
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
            Debug.LogWarning("[PickupBackpack2D] δ�� GameProgress", this);
            return;
        }

        if (!progress.backpackUnlocked)
        {
            progress.UnlockBackpack(true);

            // ��ʾ����ñ��������������Զ����
            InfoDialogUI.Instance?.ShowMessage(obtainedString);
            InfoDialogUI.Instance?.CancelInvoke(nameof(InfoDialogUI.Clear));
            InfoDialogUI.Instance?.Invoke(nameof(InfoDialogUI.Clear), kMessageHold);

            if (sfxSource && pickupSfx) sfxSource.PlayOneShot(pickupSfx);

            inventory?.NotifyCapacityChanged();

            // [MOD] ���Ϊ��ʰȡ & ��չʾ�������� Exit ���
            _pickedUp = true;
            _suppressExitClearUntil = Time.time + kMessageHold;

            // [MOD] ��΢�ӳٺ������٣�ȷ������ͬ֡���� Exit ���
            if (destroyOnPickup)
                StartCoroutine(DestroyAfter(0.05f));
            else
                gameObject.SetActive(false);
        }
        else
        {
            InfoDialogUI.Instance?.ShowMessage("�����ѽ���");
            InfoDialogUI.Instance?.CancelInvoke(nameof(InfoDialogUI.Clear));
            InfoDialogUI.Instance?.Invoke(nameof(InfoDialogUI.Clear), 1.5f);
        }
    }

    System.Collections.IEnumerator DestroyAfter(float delay)   // [MOD]
    {
        // ��ѡ���Ƚ�����ײ���������̴��� Exit
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
