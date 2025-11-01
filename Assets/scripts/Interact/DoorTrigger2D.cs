using System.Collections;
using System.Collections.Generic;
using Characters.Player;
using Scene;
using UI;
using UnityEngine;

namespace Interact
{
    public class DoorTrigger2D : MonoBehaviour
    {
        [Header("唯一ID（用于防止重复触发）")]
        public string triggerId = "door_final";

        [Header("对白内容")]
        public List<DialogueTrigger2D_Save.DialogueLine> lines = new()
        {
            new DialogueTrigger2D_Save.DialogueLine { speaker = "姜宁", content = "终于……结束了吗？" },
            new DialogueTrigger2D_Save.DialogueLine { speaker = "祝榆", content = "暂时而已，我们才刚刚开始。" },
            new DialogueTrigger2D_Save.DialogueLine { speaker = "旁白", content = "两人对视一眼，大门缓缓开启。" }
        };

        public ToggleObjectTrigger toggleObjectTrigger;

        [Header("获得文档 ID（会自动打开阅读界面）")]
        public string docId = "doc_final";

        public GameObject trigger;

        private Player player;

        private void Start()
        {
            player = FindObjectOfType<Player>();
        }

        public void GateInteract()
        {
            StartCoroutine(MainFlow());
        }

        private IEnumerator MainFlow()
        {
            
            Destroy(trigger);
            
            if (player)
            {
                player.LockControl();
            }
            
            toggleObjectTrigger.gameObject.SetActive(false);
            
            InfoDialogUI.Instance?.HideArrow();

            // === 播放对白 ===
            var dialogueLines = new List<(string speaker, string content)>();
            foreach (var l in lines)
                dialogueLines.Add((l.speaker, l.content));

            bool finished = false;
            InfoDialogUI.Instance.BeginDialogue(dialogueLines, () => finished = true);
            yield return new WaitUntil(() => finished);

            

          
            var fade = FindObjectOfType<SceneFadeEffect>();
            if (fade)
                fade.FadeOutAndLoad("Final",1f,1.5f);
            else
                Debug.LogWarning("[DoorTrigger2D] 未找到 SceneFadeEffect 实例");
            
        }
    }
}
