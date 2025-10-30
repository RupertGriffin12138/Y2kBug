using Core;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardManager : MonoBehaviour
{
    private static string BoardKey = "BoardKey_Prefab";
    private int boardProgress;
    private bool canCa=false;

    [SerializeField] private GameObject[] gameObjects;
    [SerializeField] private Dictionary<int,GameObject> boards = new Dictionary<int,GameObject>();
    [SerializeField] private GameObject boardErase;


    [SerializeField] private SpriteRenderer blackboardFade;
    [SerializeField] private float fadeDuration = 2.0f;

    private Material fadeMaterial;
    private static readonly int FadeProgress = Shader.PropertyToID("_FadeProgress");

    private void Start()
    {
        PlayerPrefs.SetInt(BoardKey,4);

        boardErase.SetActive(false);
        boardProgress = PlayerPrefs.GetInt(BoardKey, 1);
        for (int i = 1; i <= gameObjects.Length; i++)
        {
            boards[i] = gameObjects[i - 1];
        }
        
        ShowUI();
        ShowBoaord();
        // 确保SpriteRenderer存在
        if (blackboardFade == null)
            blackboardFade = GetComponent<SpriteRenderer>();

        // 创建材质实例
        fadeMaterial = new Material(blackboardFade.material);
        blackboardFade.material = fadeMaterial;

        // 初始透明度设为0
        SetFadeProgress(0f);


        
        
        
    }

    void ShowBoaord()
    {
        if (boardProgress==3)
        {
            SceneManager.LoadScene("Riddle blackboard");
            return;
        }
        for (int i = 1; i <= gameObjects.Length; i++)
        {
            boards[i].gameObject.SetActive(i== boardProgress);
            if (i == 3 && boardProgress == 4)
                boards[i].gameObject.SetActive(true);
            Debug.Log("i 是"+i);
        }
    }

    void ShowUI()
    {
        switch (boardProgress)
        {
            case 1:
                if (InfoDialogUI.Instance)
                {
                    InfoDialogUI.Instance.ShowMessage("（值日生的名字也是一团乱码，看得让人怪不舒服的，把它擦掉吧。）");
                    InfoDialogUI.Instance.EnableCharacterBackground("姜宁");
                    Debug.Log("InfoDialogUI is null");
                }
                break;
            case 2:
                SceneManager.LoadScene("Riddle blackboard");
                SetFadeProgress(1f);
                break;
            case 3:
                SceneManager.LoadScene("Riddle blackboard");
                break;
            case 4:
                boards[3].gameObject.SetActive(true);
                break;
        }
    }



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canCa)
        {
            Fade();
        }
        if (boardProgress == 1&&!canCa)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                canCa = true;
                InfoDialogUI.Instance.ShowMessage("按 <b>E</b> 交互");
                InfoDialogUI.Instance.EnableCharacterBackground("旁白");
            }     
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (boardProgress==2)
                PlayerPrefs.SetInt(BoardKey, 3);
            SceneManager.LoadScene("C1CJB");
        }
    }


    ///Shader Effect
    public void Fade()
    {
        boards[2].gameObject.SetActive(true);
        StartCoroutine(FadeCoroutine());
    }

    private IEnumerator FadeCoroutine()
    {
        float elapsedTime = 0f;
        SetFadeProgress(0f);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / fadeDuration);
            SetFadeProgress(progress);
            boardErase.SetActive(true);
            yield return null;
        }

        SetFadeProgress(1f); // 确保最终完全显示
        PlayerPrefs.SetInt(BoardKey,2);
        SceneManager.LoadScene("C1CJB");
    }

    private void SetFadeProgress(float progress)
    {
        if (fadeMaterial != null)
            fadeMaterial.SetFloat(FadeProgress, progress);
    }

    // 公开方法，可从外部调用
    public void StartFade(float duration = 0f)
    {
        if (duration > 0)
            fadeDuration = duration;

        Fade();
    }

    void OnDestroy()
    {
        // 清理创建的材质实例
        if (fadeMaterial != null)
            DestroyImmediate(fadeMaterial);
    }
}
