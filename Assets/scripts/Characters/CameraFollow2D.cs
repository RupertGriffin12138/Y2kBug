using UnityEngine;

namespace Characters
{
    public class CameraFollow2D : MonoBehaviour
    {
        public Transform target;
        public Vector2 offset;
        public float smoothTime = 0.2f;
        public float deadZone = 0.02f; // 世界单位

        Vector3 vel;

        void LateUpdate()
        {
            if (!target) return;


            Vector3 cur = transform.position;
            Vector3 dst = new(target.position.x + offset.x,
                target.position.y + offset.y,
                cur.z);

            Vector2 d = new(dst.x - cur.x, dst.y - cur.y);
            if (d.sqrMagnitude < deadZone * deadZone) return;

            transform.position = Vector3.SmoothDamp(cur, dst, ref vel, smoothTime);
        }
    }
}
