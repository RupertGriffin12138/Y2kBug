using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Demo ��С�����д浵���ݣ��������ֶ������飬���� JsonUtility��
/// ���ݣ����� lastScene/playerX/playerY����������������λ�õĽṹ�뷽����
/// </summary>
[Serializable]
public class SaveData
{
    // ���� ȫ�ֽ��� ����  
    public bool backpackUnlocked = false;

    // ���� λ��/���������¼��ݣ��������ֶΣ� ����  
    public string lastScene = "";   // ���� "Town"
    public float playerX = 0f;
    public float playerY = 0f;

    // ����������������¼���λ�ã����� JsonUtility�������� Dictionary��
    [Serializable]
    public class ScenePos
    {
        public string scene;
        public float x;
        public float y;

        public ScenePos() { }
        public ScenePos(string scene, Vector2 pos)
        {
            this.scene = scene;
            this.x = pos.x;
            this.y = pos.y;
        }

        public Vector2 ToVector2() => new Vector2(x, y);
    }

    public ScenePos[] scenePositions = Array.Empty<ScenePos>();

    // ���� �ѱ�������/���ء��Ķ��󼯺ϣ�һ����ʰȡ�һ���ԶԻ��������ȣ� ����  
    public string[] disabledObjectIds = Array.Empty<string>();

    // ���� �������������飺id/count һһ��Ӧ�� ����  
    public string[] inventoryIds = Array.Empty<string>();
    public int[] inventoryCounts = Array.Empty<int>();

    // ���� �ĵ����ѻ��/���Ķ��� ����  
    public string[] docCollectedIds = Array.Empty<string>();
    public string[] docReadIds = Array.Empty<string>();

    // ���� �Ի����ȣ�����ɵĶԻ�������ID�� ����  
    public string[] dialogueSeenIds = Array.Empty<string>();

    // =========================================================
    // ����/���߷���
    // =========================================================

    /// <summary>��֤����ǿգ��Ӿɰ汾/�մ浵�ָ�ʱ�ã���</summary>
    public void EnsureArraysNotNull()
    {
        scenePositions ??= Array.Empty<ScenePos>();
        disabledObjectIds ??= Array.Empty<string>();
        inventoryIds ??= Array.Empty<string>();
        inventoryCounts ??= Array.Empty<int>();
        docCollectedIds ??= Array.Empty<string>();
        docReadIds ??= Array.Empty<string>();
        dialogueSeenIds ??= Array.Empty<string>();
    }

    /// <summary>���ɰ�浵���� lastScene/x/y������ûд�� scenePositions������һ��Ǩ�ơ�</summary>
    public void MigrateLegacyPlayerPosIfNeeded()
    {
        EnsureArraysNotNull();
        if (!string.IsNullOrEmpty(lastScene))
        {
            bool exists = scenePositions.Any(p => p.scene == lastScene);
            if (!exists)
            {
                var list = scenePositions.ToList();
                list.Add(new ScenePos(lastScene, new Vector2(playerX, playerY)));
                scenePositions = list.ToArray();
            }
        }
    }

    /// <summary>����ĳ�������µ�������ꣻͬʱ���� lastScene �;ɰ� x/y �Ա��ּ��ݡ�</summary>
    public void SetPlayerPos(string scene, Vector2 position)
    {
        if (string.IsNullOrEmpty(scene)) return;
        EnsureArraysNotNull();

        int idx = Array.FindIndex(scenePositions, p => p.scene == scene);
        if (idx >= 0)
        {
            scenePositions[idx].x = position.x;
            scenePositions[idx].y = position.y;
        }
        else
        {
            var list = scenePositions.ToList();
            list.Add(new ScenePos(scene, position));
            scenePositions = list.ToArray();
        }

        // ͬ�����ֶΣ��������߼�������ã�
        lastScene = scene;
        playerX = position.x;
        playerY = position.y;
    }

    /// <summary>��ȡĳ��������������ꣻ��û�м�¼�������þ��ֶ���Ϊ���ˡ�</summary>
    public bool TryGetPlayerPos(string scene, out Vector2 position)
    {
        EnsureArraysNotNull();

        if (!string.IsNullOrEmpty(scene))
        {
            var rec = Array.Find(scenePositions, p => p.scene == scene);
            if (rec != null)
            {
                position = rec.ToVector2();
                return true;
            }
        }

        // ���ˣ����ֶ���Ч�ҳ�����ƥ��ʱʹ��
        if (!string.IsNullOrEmpty(lastScene) && lastScene == scene)
        {
            position = new Vector2(playerX, playerY);
            return true;
        }

        position = default;
        return false;
    }

    // ���� �Ի����� API��ԭ�������� ����  

    /// <summary>�Ƿ�����ɸöԻ���</summary>
    public bool HasSeenDialogue(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        EnsureArraysNotNull();
        return Array.IndexOf(dialogueSeenIds, id) >= 0;
    }

    /// <summary>���Ի����Ϊ��ɣ���δ��������ӣ���</summary>
    public bool TryMarkDialogueSeen(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        EnsureArraysNotNull();

        if (HasSeenDialogue(id))
            return false; // �Ѵ��ڣ����ظ����

        var list = dialogueSeenIds.ToList();
        list.Add(id);
        dialogueSeenIds = list.ToArray();
        return true;
    }
}
