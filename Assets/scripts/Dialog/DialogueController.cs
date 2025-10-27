using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;

public class DialogueController : MonoBehaviour
{
    public InfoDialogUI infoDialogUI; // ����InfoDialogUIʵ��

    private string[] scriptLines; // �籾��������
    private int[] cartoonIndices; // ��Ӧÿ����ͨ�����̨������
    private int currentIndex = 0; // ��ǰ�Ի�����

    void Start()
    {
        if (infoDialogUI == null)
        {
            Debug.LogError("InfoDialogUI is not assigned.");
            return;
        }

        LoadScriptFromJson();
        infoDialogUI.StartDialogue();
        StartCoroutine(ShowDialogue());
    }

    void LoadScriptFromJson()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string filePath = $"Assets/Scripts/Dialog/{sceneName}.json";

        if (!File.Exists(filePath))
        {
            Debug.LogError($"JSON file for scene '{sceneName}' does not exist at path: {filePath}");
            return;
        }

        string jsonContent = File.ReadAllText(filePath);
        var scriptData = JsonUtility.FromJson<ScriptData>(jsonContent);

        scriptLines = scriptData.lines;
        cartoonIndices = scriptData.cartoonIndices;
    }

    IEnumerator ShowDialogue()
    {
        while (currentIndex < scriptLines.Length)
        {
            string line = scriptLines[currentIndex];
            string name = "";
            string dialogue = "";

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

            // ����������� "[�̻�������]"
            if (dialogue == "������ʮ��")
            {
                infoDialogUI.DisableAllCartoonsWithFadeOut();
                infoDialogUI.EnableCartoon(infoDialogUI.cartoonObjects.Length - 1); // �������һ����ͨ���� (T_cartoon_6)

                infoDialogUI.ShowMessage(dialogue);
                yield return new WaitForSeconds(2f); // �ȴ�һ��ʱ������
                currentIndex++;
                continue;
            }

            // ������������������Ӧ�ı���ͼ��
            infoDialogUI.EnableCharacterBackground(name);

            if (System.Array.IndexOf(cartoonIndices, currentIndex) >= 0)
            {
                int cartoonIndex = System.Array.IndexOf(cartoonIndices, currentIndex);
                infoDialogUI.EnableCartoon(cartoonIndex);
            }

            foreach (char c in dialogue.ToCharArray())
            {
                infoDialogUI.textBoxText.text += c;
                yield return new WaitForSeconds(0.05f);
            }

            infoDialogUI.ShowArrow();
            yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.E));
            infoDialogUI.HideArrow();

            currentIndex++;
        }

        infoDialogUI.EndDialogue();
    }

    [System.Serializable]
    private class ScriptData
    {
        public string[] lines;
        public int[] cartoonIndices;
    }
}


