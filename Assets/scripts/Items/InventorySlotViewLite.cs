using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;   // 加这个
using TMPro;

public class InventorySlotViewLite : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
        hasItem = (sprite != null && count > 0);
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
}
