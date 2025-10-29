using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CJBUI : MonoBehaviour
{
    [SerializeField] private GameObject[] gameObjects;
    void Start()
    {
        int a = PlayerPrefs.GetInt("BoardKey_Prefab",1);
        gameObjects[0].gameObject.SetActive(a!=4);
        gameObjects[1].gameObject.SetActive(a==4);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
