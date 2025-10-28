using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
                // 持续移动
                rect.anchoredPosition += direction * (speed * Time.unscaledDeltaTime);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            // 淡出再销毁
            Image img = GetComponent<Image>();
            if (img)
                img.CrossFadeAlpha(0f, 0.5f, false);

            yield return new WaitForSeconds(0.5f);
            Destroy(gameObject);
        }
    }
}