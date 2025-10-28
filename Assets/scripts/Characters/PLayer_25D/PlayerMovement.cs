using System.Collections;
using UnityEngine;

namespace Characters.PLayer_25D
{
    public class PlayerMovement : MonoBehaviour
    {
        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        private static readonly int InputX = Animator.StringToHash("InputX");
        private static readonly int InputY = Animator.StringToHash("InputY");
        public float speed;

        // === 新增：控制锁定逻辑 ===
        public bool isBusy { get; private set; }

        
        private Rigidbody2D rb;
        private Animator animator;
        private float inputX;
        private float inputY;
        private float stopX, stopY;

        private Vector3 offset;
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
            offset = Camera.main.transform.position - transform.position;
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponentInChildren<Animator>();
        }


        public void Update()
        {
            if (isBusy)
            {
                // 锁定期间彻底停下动画与速度
                rb.velocity = Vector2.zero;
                animator.SetBool(IsMoving, false);
                return;
            }

            // === 正常输入逻辑 ===
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
        
        // === 新增：锁定与解锁控制 ===
        public void LockControl()
        {
            isBusy = true;
            rb.velocity = Vector2.zero;
            animator.SetBool(IsMoving, false);
        }

        public void UnlockControl()
        {
            isBusy = false;
        }
        
        public IEnumerator BusyFor(float seconds)
        {
            LockControl();
            yield return new WaitForSeconds(seconds);
            UnlockControl();
        }
    }
}
