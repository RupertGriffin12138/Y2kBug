using System;
using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dialog
{
    public class DialogueController : MonoBehaviour
    {
        public InfoDialogUI infoDialogUI; // ����InfoDialogUIʵ��

        private string[] scriptLines; // �籾��������
        private int[] cartoonIndices; // ��Ӧÿ����ͨ�����̨������
        private int currentIndex = 0; // ��ǰ�Ի�����

        [Header("Scene Transition")]
        public bool loadNextSceneOnEnd = false;     // ���Ž������Ƿ���ת
        public string nextSceneName = "";           // ��һ�������������� Build Settings ����ӣ�
        public float nextSceneDelay = 0f;           // ��תǰ��ʱ���룩

        void Start()
        {
            if (infoDialogUI == null)
            {
                Debug.LogError("InfoDialogUI is not assigned.");
                return;
            }

            // ��Ϊ Resources ��ȡ����������·������ʧ��
            if (!LoadScriptFromResources())
            {
                // �������籾��ֱ�ӽ������������������
                infoDialogUI.EndDialogue();
                TryLoadNextSceneIfNeeded(); // ����û�жԻ�Ҳ������
                return;
            }

            infoDialogUI.StartDialogue();
            StartCoroutine(ShowDialogue());
        }

        // ================== �Ķ� 1��ʹ�� Resources ��ȡ ==================
        // ��Ҫ�� json �ŵ� Assets/Resources/Dialog/ �£��ļ��� = ������������ .json
        bool LoadScriptFromResources()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            string resPath = $"Dialog/{sceneName}"; // ��Ӧ Assets/Resources/Dialog/{sceneName}.json

            TextAsset jsonAsset = Resources.Load<TextAsset>(resPath);
            if (jsonAsset == null)
            {
                Debug.LogError($"[Dialogue] TextAsset not found at Resources/{resPath}.json (place JSON at Assets/Resources/Dialog/)");
                return false;
            }

            var scriptData = JsonUtility.FromJson<ScriptData>(jsonAsset.text);
            if (scriptData == null)
            {
                Debug.LogError("[Dialogue] JSON parse failed.");
                return false;
            }

            scriptLines = scriptData.lines ?? Array.Empty<string>();
            cartoonIndices = scriptData.cartoonIndices ?? Array.Empty<int>();

            if (scriptLines.Length == 0)
            {
                Debug.LogWarning("[Dialogue] lines is empty.");
            }
            return true;
        }
        // ===============================================================

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
                // ���ﱣ�����ԭ���жϣ����������߼�
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

                if (Array.IndexOf(cartoonIndices, currentIndex) >= 0)
                {
                    int cartoonIndex = Array.IndexOf(cartoonIndices, currentIndex);
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

            // ========= �Ķ� 2���������ѡ���е���һ������ =========
            TryLoadNextSceneIfNeeded();
        }

        void TryLoadNextSceneIfNeeded()
        {
            if (loadNextSceneOnEnd && !string.IsNullOrEmpty(nextSceneName))
            {
                StartCoroutine(LoadNextSceneCoroutine());
            }
        }

        IEnumerator LoadNextSceneCoroutine()
        {
            if (nextSceneDelay > 0f)
                yield return new WaitForSeconds(nextSceneDelay);

            // ȷ���� Build Settings ������� nextSceneName
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
        // =============================================================

        [Serializable]
        private class ScriptData
        {
            public string[] lines;
            public int[] cartoonIndices;
        }
    }
}
