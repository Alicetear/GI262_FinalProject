using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [Header("ตั้งค่าพื้นฐานศัตรู")]
    [SerializeField] protected int maxHealth = 30;
    protected int currentHealth;
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected int attackDamage = 10;

    // --- เพิ่มตัวแปรสำหรับการโจมตีต่อเนื่อง ---
    [Header("ตั้งค่าการโจมตี")]
    [SerializeField] protected float attackCooldown = 1.5f; // ความเร็วในการตี (วินาที/ครั้ง)
    private float currentAttackTimer = 0f; // ตัวนับเวลา
    private Barricade targetBarricade = null; // เก็บเป้าหมายกำแพงที่กำลังตีอยู่
    // ------------------------------------

    protected bool hasReachedWall = false;
    private Rigidbody2D rb;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void FixedUpdate()
    {
        if (!hasReachedWall && rb != null)
        {
            Vector2 newPos = rb.position + Vector2.right * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
    }

    // เพิ่มฟังก์ชัน Update สำหรับนับเวลาตีตอนที่เกาะกำแพงอยู่
    protected virtual void Update()
    {
        // ถ้าชนกำแพงแล้ว และมีเป้าหมายเป็นกำแพง
        if (hasReachedWall && targetBarricade != null)
        {
            // เริ่มนับเวลาถอยหลัง
            currentAttackTimer -= Time.deltaTime;

            // ถ้าเวลาหมด (ถึงรอบตี)
            if (currentAttackTimer <= 0f)
            {
                AttackBarricade(); // สั่งโจมตี
                currentAttackTimer = attackCooldown; // รีเซ็ตเวลารอรอบต่อไป
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            hasReachedWall = true;

            // เก็บตัวแปร Barricade ไว้โจมตีต่อเนื่องใน Update
            targetBarricade = collision.gameObject.GetComponent<Barricade>();

            if (targetBarricade != null)
            {
                // ตีครั้งแรกทันทีที่ชน
                AttackBarricade();
                // เซ็ตเวลาให้รอตีรอบถัดไป
                currentAttackTimer = attackCooldown;
            }
        }
    }

    // ฟังก์ชันโจมตีกำแพง
    private void AttackBarricade()
    {
        if (targetBarricade != null)
        {
            targetBarricade.TakeDamage(attackDamage);
            Debug.Log($"{gameObject.name} กำลังตีกำแพง! ดาเมจ: {attackDamage}");
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