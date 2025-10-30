using UnityEngine;
using UnityEngine.UI;

public class SetSortingOrder : MonoBehaviour
{
    public Image image1;
    public Image image2;

    void Start()
    {
        // …Ë÷√Sibling Index
        SetSiblingIndex(image1.rectTransform, 2);
        SetSiblingIndex(image2.rectTransform, 1);
    }

    void SetSiblingIndex(RectTransform rectTransform, int index)
    {
        if (rectTransform != null)
        {
            rectTransform.SetSiblingIndex(index);
        }
        else
        {
            Debug.LogError("RectTransform component is not assigned.");
        }
    }
}