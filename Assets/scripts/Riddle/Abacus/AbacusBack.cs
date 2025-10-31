using System;
using Items;
using Scene;
using UnityEngine;

namespace Riddle.Abacus
{
    public class AbacusBack : MonoBehaviour
    {
        private SceneFadeEffect sceneFadeEffect;

        private DocReaderPanel docReaderPanel;

        private void Start()
        {
            sceneFadeEffect = FindObjectOfType<SceneFadeEffect>(true);
            docReaderPanel = FindObjectOfType<DocReaderPanel>(true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !docReaderPanel.gameObject.activeSelf)
            {
                sceneFadeEffect.FadeOutAndLoad("C1CJC",0.5f,1f);
            }
        }
    }
}
