using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class PickupBackpack2D : MonoBehaviour
{
    [Header("ʰȡ����")]
    public KeyCode pickupKey = KeyCode.E;
    public bool destroyOnPickup = true;

    [Header("��ʾ�ı�")]
    [TextArea] public string promptString = "�� <b>E</b> ʰȡ����";
    [TextArea] public string obtainedString = "��� �������ѽ���ȫ�����ӣ�";

    public InventoryLite inventory;

    [Header("��ѡ��Ч")]
    public AudioSource sfxSource;
    public AudioClip pickupSfx;

    bool _inRange;

    // --- չʾ������ Exit ��� ---
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
        // �������������κ� Load�����ͳһ���ˣ�
    }

    void OnEnable()
    {
        // ����Ѿ����������볡��������
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
            InfoDialogUI.Instance?.ShowMessage("�����ѽ���");
            InfoDialogUI.Instance?.CancelInvoke(nameof(InfoDialogUI.Clear));
            InfoDialogUI.Instance?.Invoke(nameof(InfoDialogUI.Clear), 1.2f);
            return;
        }

        // �״ν��� -> д��浵
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
