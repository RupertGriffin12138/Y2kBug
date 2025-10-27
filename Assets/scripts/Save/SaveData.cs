using System;
using System.Linq;

namespace Save
{
    /// <summary>
    /// Demo ��С�����д浵���ݣ��������ֶ������飬���� JsonUtility��
    /// ���˼򻯷�������ʹ�ó���λ�ñ��棩
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // ���� ȫ�ֽ��� ����  
        public bool backpackUnlocked = false;

        // ���� λ��/�������������ֶε���������ʹ�ã� ����  
        public string lastScene = "";
        public float playerX = 0f;
        public float playerY = 0f;

        // ���� �ѱ�������/���ء��Ķ��󼯺� ����  
        public string[] disabledObjectIds = Array.Empty<string>();

        // ���� �������������飺id/count һһ��Ӧ�� ����  
        public string[] inventoryIds = Array.Empty<string>();
        public int[] inventoryCounts = Array.Empty<int>();

        // ���� �ĵ����ѻ��/���Ķ��� ����  
        public string[] docCollectedIds = Array.Empty<string>();
        public string[] docReadIds = Array.Empty<string>();

        // ���� �Ի����ȣ�����ɵĶԻ�������ID�� ����  
        public string[] dialogueSeenIds = Array.Empty<string>();

        public void EnsureArraysNotNull()
        {
            disabledObjectIds ??= Array.Empty<string>();
            inventoryIds ??= Array.Empty<string>();
            inventoryCounts ??= Array.Empty<int>();
            docCollectedIds ??= Array.Empty<string>();
            docReadIds ??= Array.Empty<string>();
            dialogueSeenIds ??= Array.Empty<string>();
        }

        public bool HasSeenDialogue(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            EnsureArraysNotNull();
            return Array.IndexOf(dialogueSeenIds, id) >= 0;
        }

        public bool TryMarkDialogueSeen(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            EnsureArraysNotNull();
            if (HasSeenDialogue(id)) return false;

            var list = dialogueSeenIds.ToList();
            list.Add(id);
            dialogueSeenIds = list.ToArray();
            return true;
        }
    }
}
