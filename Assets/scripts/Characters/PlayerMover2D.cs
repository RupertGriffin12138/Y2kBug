using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover2D : MonoBehaviour
{

    [Header("Move Settings")]
    public float moveSpeed = 6f;   // 左右移动速度
    public float jumpForce = 12f;  // 跳跃力度

    [Header("Ground Check")]
    public Transform groundCheck;      // 用来检测是否落地的空物体
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;      // 指定地面图层（Ground

    Rigidbody2D rb;
    float inputX;
    bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // A/D 或 ←/→ 键，返回 -1/0/1
        inputX = Input.GetAxisRaw("Horizontal");

        // 2️⃣ 检测是否在地面上
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 3️⃣ 按 W 跳跃（仅限落地时）
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f); // 清空垂直速度
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    void FixedUpdate()
    {
        // 只改水平速度，保留当前竖直速度（用于受重力站稳、下坡等）
        rb.velocity = new Vector2(inputX * moveSpeed, rb.velocity.y);
    }

    // 方便调试，Scene 视图显示落地检测范围
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
