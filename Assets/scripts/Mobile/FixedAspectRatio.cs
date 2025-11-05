using UnityEngine;

namespace Mobile
{
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(1000)] // 保证在 Cinemachine 之后执行
    public class FixedAspectRatio : MonoBehaviour
    {
        public float targetAspect = 1600f / 1200f;
        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void OnPreCull()
        {
            ApplyAspect();
            GL.Clear(true, true, Color.black);
        }

        private void ApplyAspect()
        {
            float windowAspect = (float)Screen.width / Screen.height;
            float scaleHeight = windowAspect / targetAspect;

            Rect rect = cam.rect;

            if (scaleHeight < 1.0f)
            {
                rect.width = 1.0f;
                rect.height = scaleHeight;
                rect.x = 0;
                rect.y = (1.0f - scaleHeight) / 2.0f;
            }
            else
            {
                float scaleWidth = 1.0f / scaleHeight;
                rect.width = scaleWidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scaleWidth) / 2.0f;
                rect.y = 0;
            }

            cam.rect = rect;
        }
    }
}