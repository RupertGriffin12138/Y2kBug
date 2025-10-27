using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
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

        [Header("��ɫ����ͼ��")]
        public GameObject[] characterBackgrounds; // Array of character background images

        [Header("GIF����Ч��")]
        public float moveSpeedMin = 60f;
        public float moveSpeedMax = 100f;
        public float lifetimeMin = 1f;
        public float lifetimeMax = 2f;
        public float spawnIntervalMin = 0.1f;
        public float spawnIntervalMax = 1f;
        
        private Canvas mainCanvas; // UI����canvas
        private Coroutine spawnLoopCoroutine; // �������浱ǰЭ������
        private GameObject activeGifObj; // ��Ҫ����Ķ�ͼ����
        private bool keepSpawning = false; // �����Ƿ��������
        
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
            DisableAllCharacterBackgrounds();
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
            DisableAllCharacterBackgrounds();
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
            if (arrowImage)
            {
                arrowImage.enabled = true;
            }
        }

        /// <summary>���ؼ�ͷ��</summary>
        public void HideArrow()
        {
            if (arrowImage)
            {
                arrowImage.enabled = false;
            }
        }

        /// <summary>�������п�ͨ����</summary>
        public void DisableCartoons()
        {
            foreach (GameObject obj in cartoonObjects)
            {
                if (obj)
                {
                    SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
                    if (renderer)
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
                if (obj)
                {
                    StartCoroutine(FadeIn(obj));
                }
            }
        }

        /// <summary>ʹ�����𽥱�ò�͸����</summary>
        IEnumerator FadeIn(GameObject obj)
        {
            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            if (renderer)
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

        /// <summary>ʹ���п�ͨ�����𽥱��͸����</summary>
        public void DisableAllCartoonsWithFadeOut()
        {
            foreach (GameObject obj in cartoonObjects)
            {
                if (obj)
                {
                    StartCoroutine(FadeOut(obj));
                }
            }
        }

        /// <summary>ʹ�����𽥱��͸����</summary>
        IEnumerator FadeOut(GameObject obj)
        {
            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            if (renderer)
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

        /// <summary>�������н�ɫ����ͼ��</summary>
        public void DisableAllCharacterBackgrounds()
        {
            foreach (GameObject bg in characterBackgrounds)
            {
                if (bg)
                {
                    bg.SetActive(false);
                }
            }
        }

        /// <summary>����ָ���Ľ�ɫ����ͼ��</summary>
        public void EnableCharacterBackground(string characterName)
        {
            Debug.Log("Enabling character background for: " + characterName);
            for (int i = 0; i < characterBackgrounds.Length; i++)
            {
                if (characterBackgrounds[i] && characterBackgrounds[i].name.Contains(characterName))
                {
                    characterBackgrounds[i].SetActive(true);
                }
                else
                {
                    characterBackgrounds[i].SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// �� prefab ʵ����һ�� GIF������
        /// </summary>
        /// <param name="resPath">��Դ·����������չ����</param>
        /// <param name="screenPos">��Ļ�ռ�λ��</param>
        public void ShowGif(string resPath, Vector2 screenPos,Vector2 textureSize,bool isFullscreen = false)
        {
            // �������� Canvas
            if (!mainCanvas)
                mainCanvas = FindObjectOfType<Canvas>();

            if (!mainCanvas)
            {
                Debug.LogError("[InfoDialogUI] ������δ������Canvas!");
                return;
            }

            // ����� GIF����ֹ�ظ���
            if (activeGifObj)
                Destroy(activeGifObj);
            
            // ���� prefab
            GameObject prefab = Resources.Load<GameObject>(resPath);
            if (!prefab)
            {
                Debug.LogWarning($"[InfoDialogUI] {resPath} δ����GIFԤ����");
                return;
            }

            // �����µ� Image
            activeGifObj = Instantiate(prefab, mainCanvas.transform);
            RectTransform rect = activeGifObj.GetComponent<RectTransform>();
            Animator animator = activeGifObj.GetComponent<Animator>();
            if (rect && !isFullscreen)
            {
                rect.anchoredPosition = screenPos;
                rect.sizeDelta = textureSize;
            }
            else if (isFullscreen)
            {
                // ����ê�������� Canvas ���ĸ���
                rect.anchorMin = Vector2.zero;     // ���½� (0, 0)
                rect.anchorMax = Vector2.one;      // ���Ͻ� (1, 1)
                rect.offsetMin = Vector2.zero;     // ���½�ƫ������
                rect.offsetMax = Vector2.zero;     // ���Ͻ�ƫ������

                // ȷ�������ϲ㣨�����ѹ����� UI��
                rect.SetAsLastSibling();
            }

            if (animator)
            {
                // ���̴�ͷ����
                animator.Play(0, 0, 0f);
                animator.Update(0f); // ����ˢ��һ֡����ֹ�ӳ���ʾ
                if (isFullscreen)
                {
                    // ȫ������������һ�κ�����
                    float animLength = 0f;
                    if (animator.runtimeAnimatorController && animator.runtimeAnimatorController.animationClips.Length > 0)
                    {
                        animLength = animator.runtimeAnimatorController.animationClips[0].length;
                    }

                    // ȷ����Ϊ0����ֹ�޶�������
                    if (animLength <= 0f) animLength = 3f;

                    StartCoroutine(DestroyAfter(animLength));
                }
            }
            Debug.Log($"[InfoDialogUI] չʾGif��ͼ��:{resPath} λ��: {screenPos}");
        }

        /// <summary>
        /// ���ص�ǰ GIF / ͼƬ
        /// </summary>
        public void HideGif()
        {
            if (activeGifObj)
            {
                Destroy(activeGifObj);
                activeGifObj = null;
            }
        }
        
        IEnumerator DestroyAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            HideGif();
        }
        
        /// <summary>
        /// �������һ�� GIF���Զ����� + ����ƶ� + �Զ����٣�
        /// </summary>
        private void SpawnRandomGif()
        {
            string[] gifNames = { "heart1", "heart2", "mouth1", "mouth2_2", "eye1", "eye2", "eye3" };
            // �������� Canvas
            if (!mainCanvas)
                mainCanvas = FindObjectOfType<Canvas>();

            if (!mainCanvas)
            {
                Debug.LogError("[InfoDialogUI] ������δ������Canvas!");
                return;
            }

            // ���ѡ�� prefab ����
            string name = gifNames[Random.Range(0, gifNames.Length)];
            string gifFolder = "Dialog/gif/prefab/";
            string resPath = gifFolder + name;

            // ���� prefab
            GameObject prefab = Resources.Load<GameObject>(resPath);
            if (!prefab)
            {
                Debug.LogWarning($"[GifSpawner] δ�ҵ� GIF prefab: {resPath}");
                return;
            }

            // ʵ������ Canvas
            GameObject obj = Instantiate(prefab, mainCanvas.transform);
            RectTransform rect = obj.GetComponent<RectTransform>();

            // �������λ�ã�UI ���꣩
            float x = Random.Range(-600f, 600f);
            float y = Random.Range(-300f, 300f);
            rect.anchoredPosition = new Vector2(x, y);

            // �����С
            float size = Random.Range(200f, 400f);
            rect.sizeDelta = new Vector2(size, size);

            // ��ʼ�ƶ�Э��
            float life = Random.Range(lifetimeMin, lifetimeMax);
            Vector2 moveDir = Random.insideUnitCircle.normalized; // �������
            float moveSpeed = Random.Range(moveSpeedMin, moveSpeedMax);

            obj.AddComponent<GifMover>().Init(moveDir, moveSpeed, life);
        }

        /// <summary>
        /// �������� N ����� GIF
        /// �������޸�����
        /// </summary>
        public void SpawnMultiple(int count)
        { 
            StartCoroutine(SpawnMultipleRoutine(count));
        }
        
        /// <summary>
        /// �������� N ����� GIF
        /// ������ true ʱ�������ɣ����� false ʱ����ֹͣ��
        /// </summary>
        public void SpawnMultiple(bool enable)
        {
            if (enable)
            {
                if (!keepSpawning)
                {
                    keepSpawning = true;
                    spawnLoopCoroutine = StartCoroutine(SpawnLoop());
                }
            }
            else
            {
                if (!keepSpawning) return; // ��ֹͣ�����
                keepSpawning = false;
                if (spawnLoopCoroutine != null)
                {
                    StopCoroutine(spawnLoopCoroutine);
                    spawnLoopCoroutine = null;
                }
            }
            
        }

        IEnumerator SpawnMultipleRoutine(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnRandomGif();
                yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));
            }
        }
        
        IEnumerator SpawnLoop()
        {
            while (keepSpawning)
            {
                SpawnRandomGif(); // ����һ����� GIF
                yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));
            }
        }
    }
}


