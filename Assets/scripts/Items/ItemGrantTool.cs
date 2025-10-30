using System.Collections;
using System.Collections.Generic;
using Interact;
using Save;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Items
{
    public static class ItemGrantTool
    {
        /// <summary>
        /// ֱ�Ӹ���������Ʒ���ɸ����԰ף���������������
        /// </summary>
        /// <param name="itemId">��ƷID</param>
        /// <param name="amount">����</param>
        /// <param name="showToast">�Ƿ���ʾ�����ʾ</param>
        /// <param name="lines">�԰����ݣ�speaker, content��</param>
        public static void GiveItem(
            string itemId,
            int amount = 1,
            bool showToast = true,
            List<(string speaker, string content)> lines = null)
        {
            if (string.IsNullOrEmpty(itemId) || amount == 0)
            {
                Debug.LogWarning("[ItemGrantTool] ��Ч����Ʒ ID ��������");
                return;
            }

            // �ҵ����������ݿ�
            var inventory = Object.FindObjectOfType<InventoryLite>();
            var itemDB = inventory ? inventory.itemDB : Object.FindObjectOfType<ItemDB>();

            // === ��ӵ���ұ��� ===
            if (inventory)
                inventory.Add(itemId, amount);

            // === д��浵 ===
            GameState.AddItem(itemId, amount);
            GameState.Current.lastScene = SceneManager.GetActiveScene().name;
            GameState.SaveNow();

            // === ��ȡ��ʾ���� ===
            string displayName = itemId;
            if (itemDB)
            {
                var def = itemDB.Get(itemId);
                if (def != null && !string.IsNullOrWhiteSpace(def.displayName))
                    displayName = def.displayName;
            }

            // === ��ʾ�����Ʒ ===
            if (showToast && InfoDialogUI.Instance)
            {
                InfoDialogUI.Instance.ShowMessage($"��� {displayName} x{amount}");
            }

            // === ���ж԰����ݣ������԰�ϵͳ ===
            if (lines is { Count: > 0 } && InfoDialogUI.Instance)
            {
                bool finished = false;
                InfoDialogUI.Instance.BeginDialogue(lines, () => finished = true);
                // ����Э�̵ȴ��԰׽�������ҪMono֧�֣�
                InfoDialogUI.Instance.StartCoroutine(WaitAndUnlock(finished));
            }

            // === ֪ͨ ConditionalSpawner ���� ===
            foreach (var spawner in Object.FindObjectsOfType<ConditionalSpawner>())
                spawner.TryCheckNow();

            Debug.Log($"[ItemGrantTool] ��һ����Ʒ��{displayName} x{amount}");
        }

        private static IEnumerator WaitAndUnlock(bool finished)
        {
            yield return new WaitUntil(() => finished);
        }
    }
}
