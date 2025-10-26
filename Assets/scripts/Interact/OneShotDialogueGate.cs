using UnityEngine;

public class OneShotDialogueGate : MonoBehaviour
{
    public SaveTag tag;
    public Transform player;
    public float triggerRadius = 1.5f;
    public bool autoTrigger = true;      // ���뷶Χ�Զ��������򰴼�����
    public KeyCode key = KeyCode.E;

    bool done = false;

    void Reset()
    {
        tag = GetComponent<SaveTag>();
    }

    void Start()
    {
        // ����Ӧ�ý׶� SceneSaveApplier �Ѿ���ѡ����ö������ء�
        // �����ټ�һ�㱣��������Ѿ����ã�ֱ�ӹر�
        if (tag && GameState.IsObjectDisabled(tag.id))
        {
            gameObject.SetActive(false);
            done = true;
        }
    }

    void Update()
    {
        if (done || !player) return;
        if (Vector2.Distance(player.position, transform.position) > triggerRadius) return;

        if ((autoTrigger) || Input.GetKeyDown(key))
        {
            PlayDialogueThenDisable();
        }
    }

    void PlayDialogueThenDisable()
    {
        // TODO: ��������������еĶԻ�ϵͳ������ζԻ�
        // ���磺DialogueSystem.Instance.Play("dlg.town.intro", onComplete: OnDialogueFinish);

        // Demo��ֱ�����
        OnDialogueFinish();
    }

    void OnDialogueFinish()
    {
        if (tag && !string.IsNullOrEmpty(tag.id))
            GameState.AddDisabledObject(tag.id);

        done = true;
        gameObject.SetActive(false);
        GameState.SaveNow();
    }
}
