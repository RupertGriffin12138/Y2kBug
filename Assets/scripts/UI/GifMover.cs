using System.Collections;
using UnityEngine;

namespace UI
{
    public class GifMover : MonoBehaviour
    {
        private float lifetime;

        public void Init(float life)
        {
            lifetime = life;
            StartCoroutine(MoveRoutine());
        }

        private IEnumerator MoveRoutine()
        {
            float timer = 0f;

            while (timer < lifetime)
            {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            
            Destroy(gameObject);
        }
    }
}