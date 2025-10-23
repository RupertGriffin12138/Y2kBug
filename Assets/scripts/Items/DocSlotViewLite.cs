using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class DocSlotViewLite : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text titleText;  // ��ť�ϵ�����

    // ����
    private string _docId;
    private string _displayName;

    // �¼����ⲿ��UI�����������ģ��� docId ���Ķ����
    public event Action<string> OnClicked;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(_docId))
                OnClicked?.Invoke(_docId);
        });
    }

    public void Set(string docId, string displayName)
    {
        _docId = docId;
        _displayName = displayName;
        if (titleText)
        {
            titleText.gameObject.SetActive(true);
            titleText.text = string.IsNullOrEmpty(displayName) ? docId : displayName;
        }
    }

    public void Clear()
    {
        _docId = null;
        _displayName = null;
        if (titleText)
        {
            titleText.text = "";
            titleText.gameObject.SetActive(false);
        }
    }
}
