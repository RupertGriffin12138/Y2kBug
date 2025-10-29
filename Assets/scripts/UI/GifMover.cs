using System.Collections;
using UnityEngine;

namespace UI
{
    public class GifMover : MonoBehaviour
    {
        private RectTransform rect;
        private Vector2 direction;
        private float speed;
        private float lifetime;

        public void Init(Vector2 dir, float moveSpeed, float life)
        {
            rect = GetComponent<RectTransform>();
            direction = dir;
            speed = moveSpeed;
            lifetime = life;

            StartCoroutine(MoveRoutine());
        }

        IEnumerator MoveRoutine()
        {
            float timer = 0f;

            while (timer < lifetime)
            {
                // ³ÖÐøÒÆ¶¯
                rect.anchoredPosition += direction * (speed * Time.unscaledDeltaTime);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            
            Destroy(gameObject);
        }
    }
}