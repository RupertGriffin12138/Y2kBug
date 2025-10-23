using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class DocSlotViewLite : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text titleText;  // 按钮上的文字

    // 数据
    private string _docId;
    private string _displayName;

    // 事件：外部（UI控制器）订阅，用 docId 打开阅读面板
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
