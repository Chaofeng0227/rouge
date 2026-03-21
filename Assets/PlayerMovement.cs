using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // 移动速度变量，可以在Unity编辑器里随时调整，非常方便
    public float moveSpeed = 5f;

    // 引用刚体组件，用于执行物理移动
    private Rigidbody2D rb;

    // 存储输入的方向
    private Vector2 moveDirection;

    void Start()
    {
        // 游戏开始时，获取挂载在同一个游戏对象上的 Rigidbody2D 组件
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 每一帧都处理输入：获取键盘 WASD 或方向键的输入
        moveDirection.x = Input.GetAxisRaw("Horizontal"); // 水平方向：A(-1), D(1)
        moveDirection.y = Input.GetAxisRaw("Vertical");   // 垂直方向：S(-1), W(1)
        moveDirection = moveDirection.normalized; // 标准化，防止斜向移动更快
    }

    void FixedUpdate()
    {
        // 在固定的物理时间步长中执行移动，这样移动更平滑、与帧率无关
        // 使用刚体的速度来控制移动，这是2D游戏常见的移动方式
        rb.velocity = moveDirection * moveSpeed;
    }
}