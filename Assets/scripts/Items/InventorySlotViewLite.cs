using System.Collections.Generic;
using Save;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
// �����

namespace Items
{
    public class InventorySlotViewLite : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,IPointerClickHandler
    {
        [Header("UI ����")]
        public Image icon;
        public TMP_Text countText;

        // ����ʱ����
        private bool hasItem;
        private string displayNameCached;

        // ���� ԭ����ʾ�߼������� displayName ���� ���� //
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

        // ���� ��ͣ��ʾ ���� //
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hasItem && !string.IsNullOrEmpty(displayNameCached) && InfoDialogUI.Instance)
            {
                InfoDialogUI.Instance.ShowItem(displayNameCached, true); // �ڶ�����ʾ�����������/ʹ�ã���
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

            Debug.Log($"�������Ʒ����: {displayNameCached}");

            // ��������ĵ��Ķ���������ʹ���߼������������ﴥ��
            /*if (InfoDialogUI.Instance)
            {
                InfoDialogUI.Instance.ShowMessage($"ʹ���� {displayNameCached}");
            }*/

            if (displayNameCached == "У��" && GameState.HasSeenDialogue("dlg_guard_1001"))
            {
                if (!GameState.HasItem("key_strange_door"))
                {
                    ItemGrantTool.GiveItem("key_strange_door",1,true,lines: new List<(string speaker, string content)>
                    {
                        ("�԰�", "����� ��ֵ�Կ�ף�\n"),
                        ("����", "���·�����ô���ж���,���Ǵ���Կ���𣿣�\n"),
                    });
                }
            }
        }
    }
}
