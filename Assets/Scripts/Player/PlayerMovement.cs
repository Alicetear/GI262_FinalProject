using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Stats")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Boundary Limits (หลังบังเกอร์)")]
    [SerializeField] private float minX = 6f;  // ขอบเขตซ้ายสุดหลังกำแพง
    [SerializeField] private float maxX = 9f;  // ขอบเขตขวาสุดหลังกำแพง
    [SerializeField] private float minY = -4f; // ขอบเขตด้านล่าง
    [SerializeField] private float maxY = 4f;  // ขอบเขตด้านบน

    private Rigidbody2D rb;
    private Vector2 movement;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // รับค่าปุ่มกด W, A, S, D
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        // คำนวณตำแหน่งใหม่
        Vector2 newPos = rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime;

        // บังคับให้อยู่ในขอบเขตหลังบังเกอร์เท่านั้น (Clamp)
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        // ขยับตัวละคร
        rb.MovePosition(newPos);
    }
}