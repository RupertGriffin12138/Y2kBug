using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;   // ����

public class DialogueController : MonoBehaviour
{
    public InfoDialogUI infoDialogUI; // ����InfoDialogUIʵ��

    [Header("�԰���ɺ�Ҫ���صĳ���")]
    public string nextSceneName = "C1S1 campus";  // ���������Ŀ�곡����������� Build Settings��

    [Header("�Ի��������Ƿ��ȵȴ�/����")]
    public float afterDialogueDelay = 0.5f;       // Сͣ�٣���Ϊ0��
    public bool useFadeOut = false;               // ������ȫ��CanvasGroup���ڳ����ɹ���
    public CanvasGroup fadeCanvas;                // ����ѡ��ȫ����Ļ��CanvasGroup��alpha 0��1��

    // �籾�������飬��ʽΪ "�������ƣ�̨��"
    private string[] scriptLines = {
        "���ҿ������������ڣ�����û�������׻��ж�ð���",
        "�԰ף��ٳ����ϵ��к������ⲽ����ʱ�شյ�ͬ�����ԣ�Ŀ�������ǿ����ʽ�ֱ��ϡ�",
        "ף�ܣ����ҿ����յ����ھ������ӣ������˲���ʮ���ˣ��������ܻ����ģ�����ô�����",
        "���ҿ������ⲻ�Ǽ���������......",
        "��ѩӨ��С�����̻������Ƕ����������Ը��",
        "�԰ף���ϸ���۾���Ů������һ�ԣ����������һͬ�������˶��У����Ǹո�һֱԶ��������У��������Ǽ��˽��",
        "���������ı��飩���ǵ�Ȼ���Ҵ����أ����������ǣ�",
        "���ҿ����������ң�����һ��������˵�������Ը����׼�飡",
        "ף�ܣ����ҡ�",
        "�԰ף��̻��������ŵݵ�ÿһ˫���ҹ���£��ĸ�����������˫�ۣ���˼���������͵ĳ��������Σ������Ǹ�ע��������ʷ��ʱ�̡�",
        "�԰ף�......",
        "���ҿ��������Ը�ˣ����ۣ�ʱ�����ϵ��ˣ� ",
        "�԰ף����Ŷ̴ٵĻ����������һ������˫�ۣ�Ŀ���䴦��֪�ǵ��桢Զ�������ǽ�������¥���µ�ʱ������Ҫ�����ˡ�",
        "[�̻�������]",
        "������ʮ��",
        "���ҿ����ţ�",
        "ף�ܣ��ˡ�",
        "��ѩӨ���ߡ�",
        "�����������塣��..��...��...һ��21���Ϳ�......"
    };

    // ��Ӧÿ����ͨ�����̨������
    private int[] cartoonIndices = { 0, 4, 6, 9, 11, 13 }; // ������Ҫ��������

    private int currentIndex = 0; // ��ǰ�Ի�����

    void Start()
    {
        if (infoDialogUI == null)
        {
            Debug.LogError("InfoDialogUI is not assigned.");
            return;
        }

        infoDialogUI.StartDialogue();
        StartCoroutine(ShowDialogue());
    }

    IEnumerator ShowDialogue()
    {
        while (currentIndex < scriptLines.Length)
        {
            string line = scriptLines[currentIndex];
            string name = "";
            string dialogue = "";

            // �ָ�ÿһ���еĽ�ɫ���ƺͶԻ�����
            int colonIndex = line.IndexOf('��');
            if (colonIndex >= 0)
            {
                name = line.Substring(0, colonIndex).Trim();
                dialogue = line.Substring(colonIndex + 1).Trim();
            }
            else
            {
                dialogue = line.Trim();
            }

            // ����������� "[�̻�������]"
            if (dialogue == "[�̻�������]")
            {
                infoDialogUI.DisableAllCartoonsWithFadeOut();
                infoDialogUI.EnableCartoon(infoDialogUI.cartoonObjects.Length - 1); // �������һ����ͨ���� (T_cartoon_6)

                infoDialogUI.ShowMessage(dialogue);
                yield return new WaitForSeconds(2f); // �ȴ�һ��ʱ������
                currentIndex++;
                continue;
            }

            // ���������ı�
            if (name == "�԰�")
            {
                infoDialogUI.SetNameText("");
            }
            else if (name == "���������ı��飩" || name == "����" || name == "�������־���飩")
            {
                infoDialogUI.SetNameText("����");
            }
            else
            {
                infoDialogUI.SetNameText(name);
            }

            infoDialogUI.textBoxText.text = "";

            // ��鵱ǰ�����Ƿ��Ӧĳ����ͨ����
            if (System.Array.IndexOf(cartoonIndices, currentIndex) >= 0)
            {
                int cartoonIndex = System.Array.IndexOf(cartoonIndices, currentIndex);
                infoDialogUI.EnableCartoon(cartoonIndex);
            }

            // ������ʾ�Ի�����
            foreach (char c in dialogue.ToCharArray())
            {
                infoDialogUI.textBoxText.text += c;
                yield return new WaitForSeconds(0.05f); // ������ʾ�ٶ�
            }

            // ��ʾ��ͷ
            infoDialogUI.ShowArrow();

            // �ȴ���Ұ��� E �����㵱ǰ�߼����� E ������
            yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.E));

            // ���ؼ�ͷ
            infoDialogUI.HideArrow();

            currentIndex++; // �ƶ�����һ���Ի�
        }

        // �Ի����� �� �رնԻ�UI
        infoDialogUI.EndDialogue();

        // ��ѡ��С�ȴ� / ����
        if (afterDialogueDelay > 0f)
            yield return new WaitForSeconds(afterDialogueDelay);

        if (useFadeOut && fadeCanvas != null)
            yield return StartCoroutine(FadeToBlack(fadeCanvas, 0.35f));

        // ������һ������
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[DialogueController] nextSceneName Ϊ�գ�δ�л�������");
        }
    }

    // ����ѡ����������
    IEnumerator FadeToBlack(CanvasGroup cg, float duration)
    {
        cg.blocksRaycasts = true;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);
            cg.alpha = a;
            yield return null;
        }
        cg.alpha = 1f;
    }
}
