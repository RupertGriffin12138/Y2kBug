using UnityEngine;

namespace Scene
{
    public class FacingCamera : MonoBehaviour
    {
        Transform[] childs;
        private Camera _camera;

        void Start()
        {
            _camera = Camera.main;
            childs = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                childs[i] = transform.GetChild(i);
            }
        }

    
        void Update()
        {
            if (childs == null) return;
            for(int i = 0; i < childs.Length; i++)
            {
                var t = childs[i];
                if (!t) continue; // 子物体已销毁，跳过
                t.rotation = _camera.transform.rotation;
            }
        }
    }
}
