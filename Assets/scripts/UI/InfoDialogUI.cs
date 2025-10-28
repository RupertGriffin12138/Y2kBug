using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI
{
    public class InfoDialogUI : MonoBehaviour
    {
        public static InfoDialogUI Instance;  // 方便槽位直接调用（也可走 Inspector 引用）

        [Header("对话框文本（TMP）")]
        public TMP_Text textBoxText; // TextBoxText UI Text component

        [Header("名字框文本（TMP）")]
        public TMP_Text nameBoxText; // NameBoxText UI Text component

        [Header("箭头图像")]
        public Image arrowImage; // Arrow image to indicate pressing E key
        [Header("无悬停时的默认提示")]
        [TextArea]
        public string idleHint = "";

        [Header("卡通对象")]
        public GameObject[] cartoonObjects; // Array of cartoon objects (T_cartoon_1, T_cartoon_2, etc.)

        [Header("角色背景图像")]
        public GameObject[] characterBackgrounds; // Array of character background images

        [Header("GIF动画效果")]
        public float moveSpeedMin = 60f;
        public float moveSpeedMax = 100f;
        public float lifetimeMin = 1f;
        public float lifetimeMax = 2f;
        public float spawnIntervalMin = 0.1f;
        public float spawnIntervalMax = 1f;

        private Canvas mainCanvas; // UI的主canvas
        private Coroutine spawnLoopCoroutine; // 用来保存当前协程引用
        private GameObject activeGifObj; // 需要激活的动图对象
        private bool keepSpawning = false; // 控制是否持续生成
        
        private bool isShowingDialogue = false;
        private Coroutine _dialogueRoutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            Clear(); // 初始显示默认提示
        }

        /// <summary>显示物品名称 + 第二行提示。</summary>
        public void ShowItem(string displayName, bool showUseTip = true)
        {
            if (isShowingDialogue) return;
            if (showUseTip)
                textBoxText.text = $"{displayName}\n<size=90%>－点击调查/使用－</size>";
            else
                textBoxText.text = displayName;
        }

        /// <summary>
        /// 显示任意文本（可用于系统消息）
        /// </summary>
        public void ShowMessage(string message)
        {
            if (!textBoxText) return;

            // 显示消息
            textBoxText.text = message;
        }
        
        /// <summary>恢复默认提示。</summary>
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

        /// <summary>设置名字文本。</summary>
        public void SetNameText(string name)
        {
            if (!nameBoxText) return;
            nameBoxText.text = name;
        }

        /// <summary>开始显示对话。</summary>
        public void StartDialogue()
        {
            isShowingDialogue = true;
            textBoxText.text = ""; // 清除默认提示
            nameBoxText.text = ""; // 确保名字框为空
            HideArrow();
            DisableCartoons();
            DisableAllCharacterBackgrounds();
        }

        /// <summary>结束显示对话。</summary>
        public void EndDialogue()
        {
            isShowingDialogue = false;
            Clear();
        }

        /// <summary>显示箭头。</summary>
        public void ShowArrow()
        {
            if (arrowImage)
            {
                arrowImage.enabled = true;
            }
        }

        /// <summary>隐藏箭头。</summary>
        public void HideArrow()
        {
            if (arrowImage)
            {
                arrowImage.enabled = false;
            }
        }

        /// <summary>禁用所有卡通对象。</summary>
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

        /// <summary>启用指定的卡通对象并使其逐渐变得不透明。</summary>
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

        /// <summary>使物体逐渐变得不透明。</summary>
        private IEnumerator FadeIn(GameObject obj)
        {
            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            if (renderer)
            {
                const float duration = 1f; // 淡入持续时间
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

        /// <summary>使所有卡通对象逐渐变得透明。</summary>
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

        /// <summary>使物体逐渐变得透明。</summary>
        public IEnumerator FadeOut(GameObject obj)
        {
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer)
            {
                float duration = 1f; // 淡出持续时间
                float elapsedTime = 0f;
                while (elapsedTime < duration)
                {
                    float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
                    spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
            }
        }
        

        /// <summary>禁用所有角色背景图像。</summary>
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

        /// <summary>启用指定的角色背景图像。</summary>
        public void EnableCharacterBackground(string characterName)
        {
            Debug.Log("Enabling character background for: " + characterName);
            foreach (var background in characterBackgrounds)
            {
                if (background && background.name.Contains(characterName))
                {
                    background.SetActive(true);
                }
                else
                {
                    background.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// 从 prefab 实例化一个 GIF并播放
        /// </summary>
        /// <param name="resPath">资源路径（不带扩展名）</param>
        /// <param name="screenPos">屏幕空间位置</param>
        public void ShowGif(string resPath, Vector2 screenPos,Vector2 textureSize,bool isFullscreen = false)
        {
            // 缓存或查找 Canvas
            if (!mainCanvas)
                mainCanvas = FindObjectOfType<Canvas>();

            if (!mainCanvas)
            {
                Debug.LogError("[InfoDialogUI] 场景内未发现主Canvas!");
                return;
            }

            // 清除旧 GIF（防止重复）
            if (activeGifObj)
                Destroy(activeGifObj);
            
            // 加载 prefab
            GameObject prefab = Resources.Load<GameObject>(resPath);
            if (!prefab)
            {
                Debug.LogWarning($"[InfoDialogUI] {resPath} 未发现GIF预制体");
                return;
            }

            // 创建新的 Image
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
                // 让它锚定到父级 Canvas 的四个角
                rect.anchorMin = Vector2.zero;     // 左下角 (0, 0)
                rect.anchorMax = Vector2.one;      // 右上角 (1, 1)
                rect.offsetMin = Vector2.zero;     // 左下角偏移清零
                rect.offsetMax = Vector2.zero;     // 右上角偏移清零

                // 确保在最上层（如果想压过别的 UI）
                rect.SetAsLastSibling();
            }

            if (animator)
            {
                // 立刻从头播放
                animator.Play(0, 0, 0f);
                animator.Update(0f); // 立刻刷新一帧，防止延迟显示
                if (isFullscreen)
                {
                    // 全屏动画仅播放一次后销毁
                    float animLength = 0f;
                    if (animator.runtimeAnimatorController && animator.runtimeAnimatorController.animationClips.Length > 0)
                    {
                        animLength = animator.runtimeAnimatorController.animationClips[0].length;
                    }

                    // 确保不为0，防止无动画报错
                    if (animLength <= 0f) animLength = 3f;

                    StartCoroutine(DestroyAfter(animLength));
                }
            }
            Debug.Log($"[InfoDialogUI] 展示Gif动图中:{resPath} 位置: {screenPos}");
        }

        /// <summary>
        /// 隐藏当前 GIF / 图片
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
        /// 随机生成一个 GIF（自动播放 + 随机移动 + 自动销毁）
        /// </summary>
        private void SpawnRandomGif()
        {
            string[] gifNames = { "heart1", "heart2", "mouth1", "mouth2_2", "eye1", "eye2", "eye3" };
            // 缓存或查找 Canvas
            if (!mainCanvas)
                mainCanvas = FindObjectOfType<Canvas>();

            if (!mainCanvas)
            {
                Debug.LogError("[InfoDialogUI] 场景内未发现主Canvas!");
                return;
            }

            // 随机选择 prefab 名称
            string name = gifNames[Random.Range(0, gifNames.Length)];
            string gifFolder = "Dialog/gif/prefab/";
            string resPath = gifFolder + name;

            // 加载 prefab
            GameObject prefab = Resources.Load<GameObject>(resPath);
            if (!prefab)
            {
                Debug.LogWarning($"[GifSpawner] 未找到 GIF prefab: {resPath}");
                return;
            }

            // 实例化到 Canvas
            GameObject obj = Instantiate(prefab, mainCanvas.transform);
            RectTransform rect = obj.GetComponent<RectTransform>();

            // 随机生成位置（UI 坐标）
            float x = Random.Range(-600f, 600f);
            float y = Random.Range(-300f, 300f);
            rect.anchoredPosition = new Vector2(x, y);

            // 随机大小
            float size = Random.Range(200f, 400f);
            rect.sizeDelta = new Vector2(size, size);

            // 开始移动协程
            float life = Random.Range(lifetimeMin, lifetimeMax);
            Vector2 moveDir = Random.insideUnitCircle.normalized; // 随机方向
            float moveSpeed = Random.Range(moveSpeedMin, moveSpeedMax);

            obj.AddComponent<GifMover>().Init(moveDir, moveSpeed, life);
        }

        /// <summary>
        /// 连续生成 N 个随机 GIF
        /// 传入有限个数量
        /// </summary>
        public void SpawnMultiple(int count)
        { 
            StartCoroutine(SpawnMultipleRoutine(count));
        }
        
        /// <summary>
        /// 连续生成 N 个随机 GIF
        /// 当传入 true 时持续生成；传入 false 时立即停止。
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
                if (!keepSpawning) return; // 已停止则忽略
                keepSpawning = false;
                if (spawnLoopCoroutine != null)
                {
                    StopCoroutine(spawnLoopCoroutine);
                    spawnLoopCoroutine = null;
                }
            }
            
        }

        private IEnumerator SpawnMultipleRoutine(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnRandomGif();
                yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));
            }
        }

        private IEnumerator SpawnLoop()
        {
            while (keepSpawning)
            {
                SpawnRandomGif(); // 生成一个随机 GIF
                yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));
            }
        }
        
        
    }
}


