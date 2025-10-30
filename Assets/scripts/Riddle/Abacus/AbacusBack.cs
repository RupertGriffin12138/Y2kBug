using System;
using Scene;
using UnityEngine;

namespace Riddle.Abacus
{
    public class AbacusBack : MonoBehaviour
    {
        private SceneFadeEffect sceneFadeEffect;

        private void Start()
        {
            sceneFadeEffect = FindObjectOfType<SceneFadeEffect>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                sceneFadeEffect.FadeOutAndLoad("C1CJC",0.5f,1f);
            }
        }
    }
}
