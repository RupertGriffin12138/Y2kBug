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

    [SerializeField] private GameObject[] gameObjects;
    [SerializeField] private Dictionary<int,GameObject> boards = new Dictionary<int,GameObject>();
    [SerializeField] private GameObject boardErase;


    [SerializeField] private SpriteRenderer blackboardFade;
    [SerializeField] private float fadeDuration = 2.0f;

    private Material fadeMaterial;
    private static readonly int FadeProgress = Shader.PropertyToID("_FadeProgress");

    private void Start()
    {
        //PlayerPrefs.SetInt(BoardKey,1);

        boardErase.SetActive(false);
        boardProgress = PlayerPrefs.GetInt(BoardKey, 1);
        ShowUI();
        // 确保SpriteRenderer存在
        if (blackboardFade == null)
            blackboardFade = GetComponent<SpriteRenderer>();

        // 创建材质实例
        fadeMaterial = new Material(blackboardFade.material);
        blackboardFade.material = fadeMaterial;

        // 初始透明度设为0
        SetFadeProgress(0f);


        
        
        for (int i = 1; i <= gameObjects.Length; i++)
        {
            boards[i] = gameObjects[i - 1];
        }
        ShowBoaord();
        


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
        }
    }

    void ShowUI()
    {
        switch (boardProgress)
        {
            case 1:
                if (InfoDialogUI.Instance)
                {
                    InfoDialogUI.Instance.ShowMessage("按 <b>E</b> 交互");
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
        if (boardProgress == 1)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Fade();
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
