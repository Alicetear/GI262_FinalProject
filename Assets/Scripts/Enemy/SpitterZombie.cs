using UnityEngine;

public class SpitterZombie : BaseEnemy
{
    [Header("สเตตัสเฉพาะตัว: Spitter (พ่นกรดระยะไกล)")]
    [SerializeField] private GameObject acidPrefab;           // Prefab ลูกกระสุนกรด
    [SerializeField] private Transform spitPoint;             // จุดปล่อยกรด
    [SerializeField] private float stopDistanceToWall = 5.5f; // ระยะห่างจากกำแพงที่จะหยุดเดินเพื่อยืนยิง
    [SerializeField] private float spitCooldown = 2.5f;       // ยิงกรดทุกๆ กี่วินาที
    [SerializeField] private Sprite spitterSprite;

    private float currentSpitTimer = 0.5f;
    private bool isInSpitStance = false;
    private SpriteRenderer spriteRenderer;
    private Transform wallTransform;

    protected override void Start()
    {
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
        float wallX = wallTransform != null ? wallTransform.position.x : 6.46f;
        float distanceToWall = wallX - transform.position.x;

        if (distanceToWall <= stopDistanceToWall)
        {
            if (!isInSpitStance)
            {
                isInSpitStance = true;
                Debug.Log($"🎯 {gameObject.name}: เข้าสู่ระยะพ่นกรดแล้ว!");
            }
        }

        if (!isInSpitStance && !hasReachedWall)
        {
            base.FixedUpdate();
        }
    }

    protected override void Update()
    {
        base.Update();

        if (isInSpitStance && !hasReachedWall)
        {
            currentSpitTimer -= Time.deltaTime;
            if (currentSpitTimer <= 0f)
            {
                SpitAcid();
                currentSpitTimer = spitCooldown;
            }
        }
    }

    private void SpitAcid()
    {
        Vector3 spawnPos = spitPoint != null ? spitPoint.position : transform.position + new Vector3(0.5f, 0, 0);

        if (acidPrefab != null)
        {
            GameObject acidObj = Instantiate(acidPrefab, spawnPos, Quaternion.identity);

            Bullet playerBullet = acidObj.GetComponent<Bullet>();
            if (playerBullet != null)
            {
                Destroy(playerBullet);
            }

            AcidSpit spit = acidObj.GetComponent<AcidSpit>();
            if (spit == null)
            {
                spit = acidObj.AddComponent<AcidSpit>();
            }

            spit.SetDirection(Vector2.right);
            Debug.Log($"🔥 {gameObject.name} พ่นน้ำกรดข้ามกำแพงสำเร็จ!");
        }
        else
        {
            CreateFallbackAcidProjectile(spawnPos);
        }
    }

    private void CreateFallbackAcidProjectile(Vector3 spawnPos)
    {
        GameObject tempAcid = new GameObject("AcidSpit_Auto");
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

        AcidSpit spit = tempAcid.AddComponent<AcidSpit>();
        spit.SetDirection(Vector2.right);
    }
}

