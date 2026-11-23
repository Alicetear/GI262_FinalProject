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

    [Header("Enemy Stats")]
    [SerializeField] protected int maxHealth = 30;
    protected int currentHealth;
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected int attackDamage = 10;

    [Header("Attack Settings")]
    [SerializeField] protected float attackCooldown = 1.5f; // Seconds between attacks.
    private float currentAttackTimer = 0f; // Time until the next attack.
    private Barricade targetBarricade = null; // Barricade currently under attack.

    [Header("Defeat Rewards")]
    [SerializeField] protected int rewardCash = 15;
    [SerializeField] protected int rewardScrap = 5;
    [SerializeField] protected int rewardScore = 100;

    [Header("เอฟเฟกต์เลือด (Blood VFX)")]
    [SerializeField] protected GameObject bloodVfxPrefab;

    [Header("ตั้งค่าการตาย (Death System)")]
    [SerializeField] protected float deathDestroyDelay = 3.0f;

    protected bool hasReachedWall = false;
    protected bool isDead = false;
    protected Rigidbody2D rb;
    protected Animator animator;

    public bool IsDead => isDead;

    protected virtual void Start()
    {
        isDead = false;
        hasReachedWall = false;
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
        }

        animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.SetBool("Die", false);
            animator.SetBool("isAttacking", false);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (isDead) return;

        if (!hasReachedWall)
        {
            if (rb != null)
            {
                Vector2 newPos = rb.position + Vector2.right * moveSpeed * Time.fixedDeltaTime;
                rb.MovePosition(newPos);
            }
            else
            {
                transform.Translate(Vector3.right * moveSpeed * Time.fixedDeltaTime);
            }

            if (animator != null)
            {
                animator.SetBool("isAttacking", false);
            }
        }
    }

    protected virtual void Update()
    {
        if (isDead) return;

        if (hasReachedWall && targetBarricade != null)
        {
            if (animator != null)
            {
                animator.SetBool("isAttacking", true);
            }

            currentAttackTimer -= Time.deltaTime;

            if (currentAttackTimer <= 0f)
            {
                AttackBarricade();
                currentAttackTimer = attackCooldown;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.GetComponent<Barricade>() != null)
        {
            hasReachedWall = true;
            targetBarricade = collision.gameObject.GetComponent<Barricade>();

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

    private void AttackBarricade()
    {
        if (isDead) return;

        if (targetBarricade != null)
        {
            targetBarricade.TakeDamage(attackDamage);
            Debug.Log($"{gameObject.name} hit the barricade for {attackDamage} damage.");
        }
    }

    public virtual void TakeDamage(int damageAmount)
    {
        TakeDamage((float)damageAmount, transform.position, Vector2.left);
    }

    public virtual void TakeDamage(float damage, Vector2 hitPoint, Vector2 hitDirection)
    {
        if (isDead) return;

        SpawnBloodVFX(hitPoint, hitDirection);

        currentHealth -= Mathf.RoundToInt(damage);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected void SpawnBloodVFX(Vector2 hitPoint, Vector2 hitDirection)
    {
        if (bloodVfxPrefab == null) return;

        Vector2 awayDirection = hitDirection.sqrMagnitude > 0.0001f ? -hitDirection.normalized : Vector2.right;
        float angle = Mathf.Atan2(awayDirection.y, awayDirection.x) * Mathf.Rad2Deg;
        GameObject vfx = Instantiate(bloodVfxPrefab, hitPoint, Quaternion.Euler(0f, 0f, angle));
        Destroy(vfx, 2.0f);
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        // ปิด Collider2D ทั้งหมด (ตัวแม่ + Hitbox ลูก) และหยุดการเคลื่อนที่ทันที
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (animator != null)
        {
            animator.SetBool("isAttacking", false);
            animator.SetBool("Die", true);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCurrency(CurrencyType.Cash, rewardCash);
            GameManager.Instance.AddCurrency(CurrencyType.Scrap, rewardScrap);
            GameManager.Instance.AddScore(rewardScore);
        }

        // แจ้ง WaveManager ทันทีที่ตาย เพื่อไม่ให้เวฟค้างระหว่างรอแอนิเมชันตาย
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyKilled(this);
        }

        Destroy(gameObject, deathDestroyDelay);
    }
}
