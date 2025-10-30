using Scene;
using UnityEngine;

namespace UI
{
    public class CreadtUI : MonoBehaviour
    {
 
    

        public void LoadToCreadt()
        {
            var scene = FindObjectOfType<SceneFadeEffect>();
            if (scene)
            {
                scene.FadeOutAndLoad("C0");
            }
        }
    }
}
