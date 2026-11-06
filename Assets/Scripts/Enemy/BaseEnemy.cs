using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [Header("ตั้งค่าพื้นฐานศัตรู")]
    [SerializeField] protected int maxHealth = 30;
    protected int currentHealth;
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected int attackDamage = 10;

    protected bool hasReachedWall = false;
    private Rigidbody2D rb;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    // เปลี่ยนมาใช้ FixedUpdate สำหรับการเคลื่อนที่ที่ยุ่งเกี่ยวกับ Rigidbody 2D
    protected virtual void FixedUpdate()
    {
        if (!hasReachedWall && rb != null)
        {
            // ใช้ MovePosition ขยับตามฟิสิกส์ จะได้ไม่เดินทะลุกำแพง
            Vector2 newPos = rb.position + Vector2.right * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
    }

    // ฟังก์ชันตรวจจับการชนแบบ OnCollisionEnter2D
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // เช็กว่าชนกับวัตถุที่มี Tag เป็น "Wall" หรือไม่
        if (collision.gameObject.CompareTag("Wall"))
        {
            hasReachedWall = true; // สั่งให้หยุดเดินทันที

            Barricade barricade = collision.gameObject.GetComponent<Barricade>();
            if (barricade != null)
            {
                barricade.TakeDamage(attackDamage); // ทำดาเมจใส่กำแพง
            }
        }
    }

    public virtual void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        if (WaveManager.Instance != null) WaveManager.Instance.OnEnemyKilled();
        Destroy(gameObject);
    }
}