using UnityEngine;

public class OneShotDialogueGate : MonoBehaviour
{
    public SaveTag tag;
    public Transform player;
    public float triggerRadius = 1.5f;
    public bool autoTrigger = true;      // 进入范围自动触发，或按键触发
    public KeyCode key = KeyCode.E;

    bool done = false;

    void Reset()
    {
        tag = GetComponent<SaveTag>();
    }

    void Start()
    {
        // 读档应用阶段 SceneSaveApplier 已经会把“禁用对象”隐藏。
        // 这里再加一层保护：如果已经禁用，直接关闭
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
        // TODO: 在这里调用你现有的对话系统播放这段对话
        // 比如：DialogueSystem.Instance.Play("dlg.town.intro", onComplete: OnDialogueFinish);

        // Demo：直接完成
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
