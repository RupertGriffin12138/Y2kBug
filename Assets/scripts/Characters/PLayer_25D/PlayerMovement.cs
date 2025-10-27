using UnityEngine;

namespace Characters.PLayer_25D
{
    public class PlayerMovement : MonoBehaviour
    {
        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        private static readonly int InputX = Animator.StringToHash("InputX");
        private static readonly int InputY = Animator.StringToHash("InputY");
        public float speed;

        private Rigidbody2D rb;
        private Animator animator;
        private float inputX;
        private float inputY;
        private float stopX, stopY;

        private Vector3 offset;
        private Camera _camera;

        void Start()
        {
            _camera = Camera.main;
            offset = Camera.main.transform.position - transform.position;
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponentInChildren<Animator>();
        }

    
        void Update()
        {
            inputX = Input.GetAxisRaw("Horizontal");
            inputY = Input.GetAxisRaw("Vertical");
            Vector2 input = new Vector2(inputX, inputY).normalized;
            rb.velocity = input * speed;

            if(input != Vector2.zero)
            {
                animator.SetBool(IsMoving, true);
                stopX = inputX;
                stopY = inputY;
            }
            else
            {
                animator.SetBool(IsMoving, false);
            }

            animator.SetFloat(InputX, stopX);
            animator.SetFloat(InputY, stopY);

            _camera.transform.position = transform.position + offset;
        }
    }
}
