using UnityEngine;

public enum EnemyAttackType
{
    Melee, // โจมตีระยะประชิด (ต้องเดินถึงกำแพง) เช่น Walker, Runner, Fat
    Range  // โจมตีระยะไกล (หยุดยืนยิง/พ่นกรด) เช่น Spitter
}

public class BaseEnemy : MonoBehaviour
{
    [Header("ประเภทศัตรู (Data Classification: Melee vs Range)")]
    [SerializeField] protected EnemyAttackType attackType = EnemyAttackType.Melee;
    public EnemyAttackType AttackType => attackType;

    [Header("ตั้งค่าพื้นฐานศัตรู")]
    [SerializeField] protected int maxHealth = 30;
    protected int currentHealth;
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected int attackDamage = 10;

    [Header("ระบบแอนิเมชัน (Animator จาก Child เช่น Visual 3D)")]
    [SerializeField] protected Animator animator;

    [Header("เอฟเฟกต์เลือดสาด (Blood VFX)")]
    [SerializeField] protected GameObject bloodVfxPrefab;

    [Header("ตั้งค่าการโจมตี")]
    [SerializeField] protected float attackCooldown = 1.5f; // ความเร็วในการตี (วินาที/ครั้ง)
    protected float currentAttackTimer = 0f; // ตัวนับเวลา
    protected Barricade targetBarricade = null; // เก็บเป้าหมายกำแพงที่กำลังตีอยู่

    [Header("รางวัลเมื่อกำจัดได้ (Rewards)")]
    [SerializeField] protected int rewardCash = 15;
    [SerializeField] protected int rewardScrap = 5;
    [SerializeField] protected int rewardScore = 100;

    [Header("สถานะการมีชีวิตและหน่วงเวลาทำลาย")]
    protected bool overrideMovement = false;
    protected bool stopMovement = false;
    protected bool isMovementLocked = false;
    protected bool isStopped = false;
    protected bool canMove = true;
    protected bool hasReachedWall = false;
    protected bool isDead = false;
    [SerializeField] protected float deathDestroyDelay = 3.0f;
    protected Rigidbody2D rb;

    public bool IsDead => isDead;

    protected virtual void Awake()
    {
        // ดึง Animator จาก Child Object เผื่อกรณีใช้โมเดล 3D ซ้อนอยู่ใน Child "Visual"
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        // ดึง Animator ซ้ำอีกครั้งใน Start เพื่อความแน่นอน
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // เมื่อเริ่มเดิน ให้เซ็ตแอนิเมชันเดิน (isAttacking = false)
        if (animator != null)
        {
            animator.SetBool("isAttacking", false);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (isDead || overrideMovement || stopMovement || isMovementLocked || isStopped || !canMove) return;

        // ถ้ายังไม่ถึงกำแพง ให้เดินหน้าไปทางขวาหา Barricade
        if (!hasReachedWall && rb != null)
        {
            Vector2 newPos = rb.position + Vector2.right * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
    }

    protected virtual void Update()
    {
        if (isDead || overrideMovement) return;

        // เมื่อซอมบี้ถึงกำแพงแล้ว จะเข้าสู่ลูปโจมตีกำแพงตามคูลดาวน์ (เฉพาะ Melee)
        if (attackType == EnemyAttackType.Melee && hasReachedWall && targetBarricade != null)
        {
            currentAttackTimer -= Time.deltaTime;

            if (currentAttackTimer <= 0f)
            {
                AttackBarricade();
                currentAttackTimer = attackCooldown;
            }
        }

        if (stopMovement || isMovementLocked || isStopped || !canMove) return;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        HandleWallContact(collision.gameObject);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        HandleWallContact(other.gameObject);
    }

    private void HandleWallContact(GameObject hitObj)
    {
        if (hitObj.CompareTag("Wall") || hitObj.GetComponent<Barricade>() != null || hitObj.GetComponentInParent<Barricade>() != null)
        {
            stopMovement = true;
            isMovementLocked = true;
            isStopped = true;
            canMove = false;
            hasReachedWall = true;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // ถ้าเป็นซอมบี้ตีไกล (Range / SpitterZombie) ห้ามทำดาเมจชนกำแพงและห้าม Die() ให้เป็นหน้าที่ของกระสุนกรดแทน
            if (attackType == EnemyAttackType.Range || this is SpitterZombie)
            {
                return;
            }

            targetBarricade = hitObj.GetComponent<Barricade>() ?? hitObj.GetComponentInParent<Barricade>();

            // สั่งหยุดเดินและเล่นแอนิเมชันโจมตี
            if (animator != null)
            {
                animator.SetBool("isAttacking", true);
            }

            if (targetBarricade != null)
            {
                AttackBarricade();
                currentAttackTimer = attackCooldown;
            }
        }
    }

    protected virtual void AttackBarricade()
    {
        if (targetBarricade != null)
        {
            targetBarricade.TakeDamage(attackDamage);
            Debug.Log($"{gameObject.name} กำลังตีกำแพง! ดาเมจ: {attackDamage}");
        }
    }

    // ฟังก์ชันรับดาเมจพร้อมตำแหน่งปะทะและทิศทางกระสุนสำหรับสร้าง Blood VFX
    public virtual void TakeDamage(int damageAmount, Vector2 hitPoint, Vector2 hitDirection)
    {
        if (isDead) return;

        // 1. เสกเอฟเฟกต์เลือดสาด (Blood VFX) พุ่งสวนทางกับกระสุน
        SpawnBloodVFX(hitPoint, hitDirection);

        // 2. ลดเลือด
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // โอเวอร์โหลดรับดาเมจแบบเดิมเพื่อความเข้ากันได้
    public virtual void TakeDamage(int damageAmount)
    {
        TakeDamage(damageAmount, transform.position, Vector2.left);
    }

    protected virtual void SpawnBloodVFX(Vector2 hitPoint, Vector2 hitDirection)
    {
        if (bloodVfxPrefab == null) return;

        // ทิศทางเลือดสาดให้พุ่งสวนทางกับกระสุน (-hitDirection)
        Vector2 sprayDir = -hitDirection.normalized;
        if (sprayDir == Vector2.zero) sprayDir = Vector2.right;

        float angle = Mathf.Atan2(sprayDir.y, sprayDir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        GameObject bloodObj = Instantiate(bloodVfxPrefab, hitPoint, rotation);
        Destroy(bloodObj, 1.5f);
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        canMove = false;

        // 1. หยุดฟิสิกส์และการตกพื้นทันที ป้องกันแรงโน้มถ่วงดึงซอมบี้ร่วงทะลุพื้นหลังปิด Collider
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; // ปิดการคำนวณฟิสิกส์
        }

        // 2. ปิด Collider2D ทั้งหมดบนตัวแม่และตัวลูก (Head_Hitbox, Body_Hitbox)
        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in allColliders)
        {
            col.enabled = false;
        }

        // 3. สุ่มขยับตำแหน่งแกน Z เล็กน้อย ป้องกันโมเดล 3D ซ้อนทับกันจนภาพกระพริบ (Z-Fighting)
        Vector3 pos = transform.position;
        pos.z += Random.Range(-0.25f, 0.25f);
        transform.position = pos;

        // 4. สั่ง Animator เปลี่ยนไปเล่นแอนิเมชันตาย (isAttacking = false, Die = true)
        if (animator != null)
        {
            animator.SetBool("isAttacking", false);
            animator.SetBool("Die", true);
        }

        // 5. แจ้งเตือน WaveManager และระบบเงิน/คะแนน ทันที
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCurrency(CurrencyType.Cash, rewardCash);
            GameManager.Instance.AddCurrency(CurrencyType.Scrap, rewardScrap);
            GameManager.Instance.AddScore(rewardScore);
        }

        if (WaveManager.Instance != null)
        {
            // ส่งศพไปลงทะเบียนเก็บไว้ในฉากระหว่าง Wave
            WaveManager.Instance.RegisterCorpse(gameObject);
            WaveManager.Instance.OnEnemyKilled(this);
        }
        else
        {
            Destroy(gameObject, deathDestroyDelay);
        }
    }
}
