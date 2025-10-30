using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardDialogTrigger : MonoBehaviour
{
    [SerializeField] private GameObject boardTrigger1;

    private void OnEnable()
    {
        if (boardTrigger1!=null&&PlayerPrefs.GetInt("BoardKey_Prefab",1)!=1)
        {
            boardTrigger1.SetActive(true);
        }
    }
    private void Update()
    {
        if(boardTrigger1==null) return;
        if (!boardTrigger1.activeSelf&& boardTrigger1 != null && PlayerPrefs.GetInt("BoardKey_Prefab", 1) != 1)
        {
            boardTrigger1.SetActive(true);
        }
    }
}
