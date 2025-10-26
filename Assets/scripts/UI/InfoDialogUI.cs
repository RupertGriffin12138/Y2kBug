using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class InfoDialogUI : MonoBehaviour
{
    public static InfoDialogUI Instance;  // �����λֱ�ӵ��ã�Ҳ���� Inspector ���ã�

    [Header("�Ի����ı���TMP��")]
    public TMP_Text textBoxText; // TextBoxText UI Text component

    [Header("���ֿ��ı���TMP��")]
    public TMP_Text nameBoxText; // NameBoxText UI Text component

    [Header("��ͷͼ��")]
    public Image arrowImage; // Arrow image to indicate pressing E key

    [Header("����ͣʱ��Ĭ����ʾ")]
    [TextArea]
    public string idleHint = "������Ƶ���Ʒ�ϲ鿴��Ϣ";

    [Header("��ͨ����")]
    public GameObject[] cartoonObjects; // Array of cartoon objects (T_cartoon_1, T_cartoon_2, etc.)

    private bool isShowingDialogue = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    void Start()
    {
        Clear(); // ��ʼ��ʾĬ����ʾ
    }

    /// <summary>��ʾ��Ʒ���� + �ڶ�����ʾ��</summary>
    public void ShowItem(string displayName, bool showUseTip = true)
    {
        if (!textBoxText || !nameBoxText) return;
        if (showUseTip)
            textBoxText.text = $"{displayName}\n<size=90%>���������/ʹ�ã�</size>";
        else
            textBoxText.text = displayName;
    }

    /// <summary>��ʾ�����ı���������ϵͳ��Ϣ����</summary>
    public void ShowMessage(string message)
    {
        if (!textBoxText) return;
        textBoxText.text = message;
    }

    /// <summary>�ָ�Ĭ����ʾ��</summary>
    public void Clear()
    {
        if (isShowingDialogue) return;
        textBoxText.text = idleHint;
        nameBoxText.text = "";
        HideArrow();
        DisableCartoons();
        isShowingDialogue = false;
    }

    /// <summary>���������ı���</summary>
    public void SetNameText(string name)
    {
        if (!nameBoxText) return;
        nameBoxText.text = name;
    }

    /// <summary>��ʼ��ʾ�Ի���</summary>
    public void StartDialogue()
    {
        isShowingDialogue = true;
        textBoxText.text = ""; // ���Ĭ����ʾ
        nameBoxText.text = ""; // ȷ�����ֿ�Ϊ��
        HideArrow();
        DisableCartoons();
    }

    /// <summary>������ʾ�Ի���</summary>
    public void EndDialogue()
    {
        isShowingDialogue = false;
        Clear();
    }

    /// <summary>��ʾ��ͷ��</summary>
    public void ShowArrow()
    {
        if (arrowImage != null)
        {
            arrowImage.enabled = true;
        }
    }

    /// <summary>���ؼ�ͷ��</summary>
    public void HideArrow()
    {
        if (arrowImage != null)
        {
            arrowImage.enabled = false;
        }
    }

    /// <summary>�������п�ͨ����</summary>
    public void DisableCartoons()
    {
        foreach (GameObject obj in cartoonObjects)
        {
            if (obj != null)
            {
                SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0f);
                }
            }
        }
    }

    /// <summary>����ָ���Ŀ�ͨ����ʹ���𽥱�ò�͸����</summary>
    public void EnableCartoon(int index)
    {
        if (index >= 0 && index < cartoonObjects.Length)
        {
            GameObject obj = cartoonObjects[index];
            if (obj != null)
            {
                StartCoroutine(FadeIn(obj));
            }
         
        }
    }

    /// <summary>ʹ�����𽥱�ò�͸����</summary>
    IEnumerator FadeIn(GameObject obj)
    {
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            float duration = 1f; // �������ʱ��
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                float alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 1f);
        }
    }

    IEnumerator FadeOut(GameObject obj)
    {
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            float duration = 1f; // ��������ʱ��
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0f);
        }
    }

    /// <summary>ʹ���п�ͨ�����𽥱��͸����</summary>
    public void DisableAllCartoonsWithFadeOut()
    {
        foreach (GameObject obj in cartoonObjects)
        {
            if (obj != null)
            {
                StartCoroutine(FadeOut(obj));
            }
        }
    }
}


