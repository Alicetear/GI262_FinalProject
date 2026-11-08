using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [Header("ตั้งค่าพื้นฐานศัตรู")]
    [SerializeField] protected int maxHealth = 30;
    protected int currentHealth;
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected int attackDamage = 10;

    [Header("ตั้งค่าการโจมตี")]
    [SerializeField] protected float attackCooldown = 1.5f; // ความเร็วในการตี (วินาที/ครั้ง)
    private float currentAttackTimer = 0f; // ตัวนับเวลา
    private Barricade targetBarricade = null; // เก็บเป้าหมายกำแพงที่กำลังตีอยู่

    [Header("รางวัลเมื่อกำจัดได้ (Rewards)")]
    [SerializeField] protected int rewardCash = 15;
    [SerializeField] protected int rewardScrap = 5;
    [SerializeField] protected int rewardScore = 100;

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

    protected virtual void Update()
    {
        if (hasReachedWall && targetBarricade != null)
        {
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
        if (collision.gameObject.CompareTag("Wall"))
        {
            hasReachedWall = true;
            targetBarricade = collision.gameObject.GetComponent<Barricade>();

            if (targetBarricade != null)
            {
                AttackBarricade();
                currentAttackTimer = attackCooldown;
            }
        }
    }

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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCurrency(CurrencyType.Cash, rewardCash);
            GameManager.Instance.AddCurrency(CurrencyType.Scrap, rewardScrap);
            GameManager.Instance.AddScore(rewardScore);
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyKilled(this);
        }

        Destroy(gameObject);
    }
}
