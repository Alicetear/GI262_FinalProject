using UnityEngine;

public class SpitterZombie : BaseEnemy
{
    [Header("สเตตัสเฉพาะตัว: Spitter (พ่นกรดระยะไกล)")]
    [Tooltip("ระยะห่างจาก Barricade ที่จะหยุดเดินเพื่อยืนพ่นกรด (เลยกลางจอเล็กน้อย)")]
    [SerializeField] private float attackRange = 5.0f;
    [Tooltip("Prefab ลูกกระสุนกรด (AcidProjectile หรือ AcidSpit)")]
    [SerializeField] private GameObject acidPrefab;
    [Tooltip("จุดปล่อยกรดที่ปากซอมบี้ (Transform ชื่อ point)")]
    [SerializeField] private Transform spitPoint;

    [Header("ตัวแปรสำรอง (Optional)")]
    [SerializeField] private GameObject acidProjectilePrefab;
    [SerializeField] private Transform mouthPoint;

    private Transform targetWall;
    private float attackTimer = 0f;

    protected override void Awake()
    {
        overrideMovement = true;
        attackType = EnemyAttackType.Range;
        base.Awake();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (spitPoint == null)
        {
            spitPoint = mouthPoint != null ? mouthPoint : FindChildRecursive(transform, "point");
        }
    }

    protected override void Start()
    {
        overrideMovement = true;
        attackType = EnemyAttackType.Range;
        moveSpeed = 2.0f;
        maxHealth = 25;
        attackDamage = 15;
        attackCooldown = 2.5f;

        base.Start();

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        if (spitPoint == null)
        {
            spitPoint = mouthPoint != null ? mouthPoint : FindChildRecursive(transform, "point");
        }

        FindTargetWall();

        attackTimer = 0.5f; // หน่วงเวลาก่อนพ่นนัดแรกเล็กน้อย
    }

    private void FindTargetWall()
    {
        // 1. ค้นหากำแพงหลักด้วย Tag "Wall" เป็นหลัก
        GameObject wallObj = GameObject.FindGameObjectWithTag("Wall");
        if (wallObj != null)
        {
            targetWall = wallObj.transform;
            if (targetBarricade == null)
            {
                targetBarricade = wallObj.GetComponent<Barricade>() ?? wallObj.GetComponentInParent<Barricade>();
            }
            return;
        }

        // 2. สำรองด้วย Barricade.Instance หรือ FindAnyObjectByType<Barricade>()
        if (targetBarricade == null)
        {
            targetBarricade = Barricade.Instance != null ? Barricade.Instance : FindAnyObjectByType<Barricade>();
        }

        if (targetBarricade != null)
        {
            targetWall = targetBarricade.transform;
        }
    }

    protected override void FixedUpdate()
    {
        if (isDead) return;

        if (targetWall == null)
        {
            FindTargetWall();
        }

        float wallX = targetWall != null ? targetWall.position.x : 6.46f;
        float distX = wallX - transform.position.x;

        if (distX > attackRange)
        {
            // เดินไปทางขวาตามปกติ
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
            }
        }
        else
        {
            // หยุดความเร็วฟิสิกส์เป็นศูนย์ทุกเฟรมเมื่อถึงระยะยิง (ห้ามสไลด์ต่อ)
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
    }

    protected override void Update()
    {
        if (isDead) return;

        if (targetWall == null)
        {
            FindTargetWall();
        }

        float wallX = targetWall != null ? targetWall.position.x : 6.46f;
        float distX = wallX - transform.position.x;

        if (distX > attackRange)
        {
            // เดินต่อด้วยแอนิเมชัน walk
            if (animator != null)
            {
                animator.SetBool("isAttacking", false);
            }
        }
        else
        {
            // หยุดความเร็วฟิสิกส์เป็นศูนย์ทุกเฟรม
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // เล่นแอนิเมชันท่ากรีดร้องพ่นกรดต่อเนื่อง
            if (animator != null)
            {
                animator.SetBool("isAttacking", true);
            }

            // นับถอยหลังพ่นกรดต่อเนื่องจนกว่าจะถูกยิงตาย
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                SpawnAcid();
                attackTimer = attackCooldown;
            }
        }
    }

    // ป้องกันการชนกำแพงแล้วตายหรือทำดาเมจชนกำแพง
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (IsWallObject(collision.gameObject))
        {
            StopAtWall(collision.gameObject);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (IsWallObject(other.gameObject))
        {
            StopAtWall(other.gameObject);
        }
    }

    private bool IsWallObject(GameObject obj)
    {
        return obj.CompareTag("Wall") || obj.GetComponent<Barricade>() != null || obj.GetComponentInParent<Barricade>() != null;
    }

    private void StopAtWall(GameObject wallObj)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (targetBarricade == null)
        {
            targetBarricade = wallObj.GetComponent<Barricade>() ?? wallObj.GetComponentInParent<Barricade>();
        }
        if (targetWall == null)
        {
            targetWall = wallObj.transform;
        }

        if (animator != null)
        {
            animator.SetBool("isAttacking", true);
        }
    }

    // เมธอดหลักสำหรับปล่อยกระสุนกรด (รองรับการเรียกผ่าน Animation Event ชื่อ SpawnAcid)
    public void SpawnAcid()
    {
        if (isDead) return;

        Transform activePoint = spitPoint != null ? spitPoint : mouthPoint;
        Vector3 spawnPos = activePoint != null ? activePoint.position : transform.position + new Vector3(0.5f, 0.8f, 0f);
        GameObject prefabToSpawn = acidPrefab != null ? acidPrefab : acidProjectilePrefab;

        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"[SpitterZombie] ลืมใส่ Acid Prefab บน {gameObject.name} หรือเปล่า!");
            return;
        }

        GameObject acidObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        // กำหนดทิศทางและดาเมจตาม attackDamage ของ SpitterZombie
        AcidProjectile projectile = acidObj.GetComponent<AcidProjectile>();
        AcidSpit acidSpit = acidObj.GetComponent<AcidSpit>();

        if (projectile != null)
        {
            projectile.SetDamage(attackDamage);
            if (targetBarricade != null)
                projectile.SetTarget(targetBarricade.transform);
            else
                projectile.SetDirection(Vector2.right);
        }
        else if (acidSpit != null)
        {
            acidSpit.SetDamage(attackDamage);
            if (targetBarricade != null)
                acidSpit.SetDirection((targetBarricade.transform.position - spawnPos).normalized);
            else
                acidSpit.SetDirection(Vector2.right);
        }
        else
        {
            projectile = acidObj.AddComponent<AcidProjectile>();
            projectile.SetDamage(attackDamage);
            if (targetBarricade != null)
                projectile.SetTarget(targetBarricade.transform);
            else
                projectile.SetDirection(Vector2.right);
        }

        Debug.Log($"🧪 {gameObject.name}: พ่นกรดไปที่ Barricade (Damage: {attackDamage})!");
    }

    // Alias สำหรับ Animation Event เดิมที่อาจผูกชื่อ SpitAcid ไว้
    public void SpitAcid()
    {
        SpawnAcid();
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName) return child;
            Transform found = FindChildRecursive(child, childName);
            if (found != null) return found;
        }
        return null;
    }
}
