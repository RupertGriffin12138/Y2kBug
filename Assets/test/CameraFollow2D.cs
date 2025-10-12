using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{

    public Transform target;
    public float smoothTime = 0.15f;
    public Vector2 offset; // ��ѡ����ͷƫ��

    Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (!target) return;
        var targetPos = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z // �������ԭ��Z
        );
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
