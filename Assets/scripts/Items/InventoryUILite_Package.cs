using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryUILite_Package : MonoBehaviour
{
    [Header("����Դ")]
    public InventoryLite inventory;                   // ָ����ҵ� InventoryLite
    public ItemDB itemDB;                               // �����գ�Ĭ��ȡ inventory.itemDB

    [Header("���� (Button01��)")]
    public List<InventorySlotViewLite> slots = new List<InventorySlotViewLite>();

    [Header("��ʾ����")]
    [Tooltip("δ�����ĸ����Ƿ�ֱ�����أ�ȡ����ѡʱ�������� Prefab �����ûұ���")]
    public bool hideLockedSlots = true;            // [MOD] �򻯣�Ĭ������δ��������

    [Header("����")]
    public bool warnOnMissingItem = true;

    //[Header("Debug")]
    //public bool debugLog = true; // [DBG]

    void OnEnable()
    {
        if (!inventory) inventory = FindObjectOfType<InventoryLite>();
        if (!itemDB && inventory) itemDB = inventory.itemDB;

        if (inventory) inventory.OnChanged += Refresh;     // [MOD]

        //if (debugLog)
        //{
        //    Debug.Log($"[PackageUI.OnEnable] this={GetInstanceID()}", this);
        //    if (inventory)
        //    {
        //        Debug.Log($"[PackageUI.OnEnable] inv={inventory.GetInstanceID()}, unlocked={inventory.UnlockedSlotCount}, isUnlocked={inventory.IsBackpackUnlocked}", inventory);
        //        if (inventory.progress)
        //            Debug.Log($"[PackageUI.OnEnable] progress id={inventory.progress.GetInstanceID()}, unlocked={inventory.progress.backpackUnlocked}", inventory.progress);
        //        else
        //            Debug.LogWarning("[PackageUI.OnEnable] inventory.progress is NULL", this);
        //    }
        //    else
        //    {
        //        Debug.LogWarning("[PackageUI.OnEnable] inventory is NULL", this);
        //    }
        //}
        Refresh();
    }

    void OnDisable()
    {
        if (inventory) inventory.OnChanged -= Refresh;     // [MOD]
    }

    /// <summary>������ʱ��������Դʱ���á�</summary>
    public void Bind(InventoryLite inv)
    {
        if (inventory != null) inventory.OnChanged -= Refresh;
        inventory = inv;
        if (inventory != null) inventory.OnChanged += Refresh;
        Refresh();
    }

    /// <summary>�� InventoryLite.entries ӳ�䵽 UI ��λ��</summary>
    public void Refresh()
    {
        if (slots == null || slots.Count == 0) return;

        var inv = inventory;
        var db = itemDB ? itemDB : (inv ? inv.itemDB : null);
        var entries = inv ? inv.entries : new List<InventoryLite.Entry>();
        int unlocked = inv ? inv.UnlockedSlotCount : 1;     // [MOD]

        //if (debugLog)
        //    Debug.Log($"[PackageUI.Refresh] unlocked={unlocked}, entries={entries.Count}, slots={slots.Count}", this);


        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (!slot) continue;

            bool isUnlocked = (i < unlocked);

            // [MOD] δ�������Ӵ������ػ��������Զ�����ûұ���
            if (hideLockedSlots)
            {
                slot.gameObject.SetActive(isUnlocked);
                if (!isUnlocked) continue;
            }
            else
            {
                // ������ʱ�����ﲻ���û�������������е��Ӿ��������Ҫ�ûң����� Prefab �ϼ� CanvasGroup�������������� alpha/interactable
                slot.gameObject.SetActive(true);
            }

            // ��ʾ���ݣ��� entries ��˳�������
            if (i < entries.Count)
            {
                var e = entries[i];
                bool hasData = !string.IsNullOrWhiteSpace(e.id) && e.count > 0;

                if (hasData)
                {
                    Sprite icon = null;
                    string displayName = e.id;

                    if (db)
                    {
                        var def = db.Get(e.id);
                        if (def != null)
                        {
                            if (def.icon) icon = def.icon;
                            if (!string.IsNullOrWhiteSpace(def.displayName)) displayName = def.displayName;
                        }
                        else if (warnOnMissingItem)
                        {
                            Debug.LogWarning($"[InventoryUI] Missing def for id: '{e.id}'", this);
                        }
                    }

                    slot.Set(icon, e.count, displayName);   // [MOD] ������� Set(sprite,count,displayName)
                    continue;
                }
            }

            // �����ݻ򳬽磺���
            slot.Clear();
        }
    }
}