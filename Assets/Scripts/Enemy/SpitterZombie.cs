using UnityEngine;

public class SpitterZombie : BaseEnemy
{
    [Header("Spitter Ranged Attack")]
    [SerializeField] private float spitRange = 6f;
    [SerializeField] private float stopDistanceToWall = 5.5f; // Distance from the barricade at which the spitter stops to attack.
    [SerializeField] private GameObject acidProjectilePrefab;
    [SerializeField] private GameObject acidPrefab;           // Legacy acid projectile prefab fallback.
    [SerializeField] private Transform mouthPoint;
    [SerializeField] private Transform spitPoint;             // Projectile spawn point fallback.
    [SerializeField] private float spitCooldown = 2.5f;       // Seconds between acid shots.
    [SerializeField] private bool useAnimationEventOnly = false;
    [SerializeField] private Sprite spitterSprite;

    private float currentSpitTimer = 0.5f;
    private bool isInSpitStance = false;
    private SpriteRenderer spriteRenderer;
    private Transform wallTransform;

    protected override void Start()
    {
        attackType = EnemyAttackType.Range;
        moveSpeed = 2.0f;
        maxHealth = 25;
        currentHealth = maxHealth;
        attackDamage = 8;

        base.Start();

        GameObject wallObj = GameObject.FindWithTag("Wall");

        if (wallObj != null)
        {
            wallTransform = wallObj.transform;
        }
        else
        {
            Barricade barricade = FindAnyObjectByType<Barricade>();
            if (barricade != null)
            {
                wallTransform = barricade.transform;
            }
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spitterSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = spitterSprite;
        }

        currentSpitTimer = 0.5f;
    }

    protected override void FixedUpdate()
    {
        if (isDead) return;

        float wallX = wallTransform != null ? wallTransform.position.x : 6.46f;
        float distanceToWall = wallX - transform.position.x;
        float stopDistance = stopDistanceToWall > 0f ? stopDistanceToWall : spitRange;

        if (distanceToWall <= stopDistance)
        {
            if (!isInSpitStance)
            {
                isInSpitStance = true;
                Debug.Log($"{gameObject.name} is in spitting range.");
            }
        }

        if (!isInSpitStance && !hasReachedWall)
        {
            base.FixedUpdate();
        }
    }

    protected override void Update()
    {
        if (isDead) return;

        base.Update();

        float wallX = wallTransform != null ? wallTransform.position.x : 6.46f;
        float distanceToWall = Mathf.Abs(wallX - transform.position.x);

        if (distanceToWall <= spitRange)
        {
            if (!isInSpitStance)
            {
                isInSpitStance = true;
                Debug.Log($"🎯 {gameObject.name}: เข้าสู่ระยะพ่นกรดแล้ว!");
            }

            if (animator != null)
            {
                animator.SetBool("isAttacking", true);
            }
        }

        // Fallback Timer กรณีแอนิเมชันยังไม่ได้ผูก Animation Event
        if (isInSpitStance && !hasReachedWall && !useAnimationEventOnly)
        {
            bool hasAnimatorController = animator != null && animator.runtimeAnimatorController != null;
            if (!hasAnimatorController)
            {
                currentSpitTimer -= Time.deltaTime;
                if (currentSpitTimer <= 0f)
                {
                    SpitAcid();
                    currentSpitTimer = spitCooldown;
                }
            }
        }
    }

    // ฟังก์ชัน public สำหรับผูก Animation Event บนไทม์ไลน์แอนิเมชันพ่นกรด
    public void SpitAcid()
    {
        if (isDead) return;

        GameObject prefabToSpawn = acidProjectilePrefab != null ? acidProjectilePrefab : acidPrefab;
        Transform spawnAnchor = mouthPoint != null ? mouthPoint : spitPoint;
        Vector3 spawnPos = spawnAnchor != null ? spawnAnchor.position : transform.position + new Vector3(0.5f, 0.5f, 0f);
        Vector2 directionToWall = wallTransform != null
            ? ((Vector2)wallTransform.position - (Vector2)spawnPos).normalized
            : Vector2.right;

        if (prefabToSpawn != null)
        {
            GameObject acidObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

            Bullet playerBullet = acidObj.GetComponent<Bullet>();
            if (playerBullet != null) Destroy(playerBullet);

            AcidProjectile projectile = acidObj.GetComponent<AcidProjectile>();
            if (projectile != null)
            {
                projectile.SetDirection(directionToWall);
            }
            else
            {
                AcidSpit legacySpit = acidObj.GetComponent<AcidSpit>();
                if (legacySpit == null) legacySpit = acidObj.AddComponent<AcidSpit>();
                legacySpit.SetDirection(directionToWall);
            }

            Debug.Log($"🔥 {gameObject.name} fired an acid projectile toward Barricade.");
        }
        else
        {
            CreateFallbackAcidProjectile(spawnPos, directionToWall);
        }
    }

    private void CreateFallbackAcidProjectile(Vector3 spawnPos, Vector2 directionToWall)
    {
        GameObject tempAcid = new GameObject("AcidProjectile_Auto");
        tempAcid.transform.position = spawnPos;
        tempAcid.transform.localScale = new Vector3(0.35f, 0.35f, 1f);

        SpriteRenderer sr = tempAcid.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.2f, 1f, 0.2f, 1f);
        sr.sortingOrder = 5;

        Texture2D tex = Texture2D.whiteTexture;
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

        CircleCollider2D col = tempAcid.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        Rigidbody2D rb = tempAcid.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        AcidProjectile projectile = tempAcid.AddComponent<AcidProjectile>();
        projectile.SetDirection(directionToWall);
    }
}

