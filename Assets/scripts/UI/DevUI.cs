using System;
using Scene;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class DevUI : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
    }
}