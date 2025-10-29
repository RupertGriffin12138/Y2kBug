using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardTrigger : MonoBehaviour
{
    [SerializeField] private string nectScene;
    private BoxCollider2D boxCollider;
    private bool inside=false;

    private void Start()
    {
        boxCollider=this.transform.GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        inside=true;
        if (other.CompareTag("Player"))
        {

            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.ShowMessage("°´ <b>E</b> ½»»¥");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        inside=false;
        if (other.CompareTag("Player"))
        {

            if (InfoDialogUI.Instance)
                InfoDialogUI.Instance.Clear();
        }
    }

    private void Update()
    {
        if (!inside) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            SceneManager.LoadScene(nectScene);
        }

    }


}
