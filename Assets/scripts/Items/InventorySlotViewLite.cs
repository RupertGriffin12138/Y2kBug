using System.Collections.Generic;
using Save;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
// 加这个

namespace Items
{
    public class InventorySlotViewLite : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,IPointerClickHandler
    {
        [Header("UI 引用")]
        public Image icon;
        public TMP_Text countText;

        // 运行时缓存
        private bool hasItem;
        private string displayNameCached;

        // —— 原有显示逻辑，增加 displayName 参数 —— //
        public void Set(Sprite sprite, int count, string displayName)
        {
            hasItem = (sprite && count > 0);
            displayNameCached = hasItem ? displayName : null;

            if (hasItem)
            {
                if (icon)
                {
                    icon.gameObject.SetActive(true);
                    icon.enabled = true;
                    icon.sprite = sprite;
                }

                if (countText)
                {
                    if (count > 1)
                    {
                        countText.gameObject.SetActive(true);
                        countText.text = count.ToString();
                    }
                    else
                    {
                        countText.gameObject.SetActive(false);
                        countText.text = "";
                    }
                }
            }
            else
            {
                Clear();
            }
        }

        public void Clear()
        {
            hasItem = false;
            displayNameCached = null;

            if (icon)
            {
                icon.sprite = null;
                icon.gameObject.SetActive(false);
                icon.enabled = false;
            }
            if (countText)
            {
                countText.text = "";
                countText.gameObject.SetActive(false);
            }
        }

        // —— 悬停提示 —— //
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hasItem && !string.IsNullOrEmpty(displayNameCached) && InfoDialogUI.Instance)
            {
                InfoDialogUI.Instance.ShowItem(displayNameCached, true); // 第二行显示“－点击调查/使用－”
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (InfoDialogUI.Instance)
            {
                InfoDialogUI.Instance.Clear();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!hasItem || string.IsNullOrEmpty(displayNameCached))
                return;

            Debug.Log($"点击了物品格子: {displayNameCached}");

            // 如果你有文档阅读器、道具使用逻辑，可以在这里触发
            /*if (InfoDialogUI.Instance)
            {
                InfoDialogUI.Instance.ShowMessage($"使用了 {displayNameCached}");
            }*/

            if (displayNameCached == "校服" && GameState.HasSeenDialogue("dlg_guard_1001"))
            {
                if (!GameState.HasItem("key_strange_door"))
                {
                    ItemGrantTool.GiveItem("key_strange_door",1,true,lines: new List<(string speaker, string content)>
                    {
                        ("旁白", "（获得 奇怪的钥匙）\n"),
                        ("姜宁", "（衣服里怎么真有东西,这是大门钥匙吗？）\n"),
                    });
                }
            }
        }
    }
}
