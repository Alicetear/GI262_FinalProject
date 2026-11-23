using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
<<<<<<< Updated upstream

    [Header("Bullet Damage")]
=======
    [SerializeField] private int damage = 15;
    [Header("ตั้งค่าความแรงกระสุน")]
    // 💡 สิ่งที่ขาดไปคือบรรทัดนี้ครับ! ประกาศตัวแปรเก็บค่าดาเมจไว้ด้านบนสุด
>>>>>>> Stashed changes
    [SerializeField] private int bulletDamage = 10;

<<<<<<< HEAD
    [Header("พฤติกรรมกระสุน (Single vs AOE)")]
    [SerializeField] private WeaponBehaviorType behaviorType = WeaponBehaviorType.Single;
    [SerializeField] private float aoeRadius = 2.5f;

    [Header("อายุขัยกระสุน (วินาที)")]
=======
    [Header("Bullet Lifetime (Seconds)")]
>>>>>>> d8ee8a61bf1f885c5b6c1c0a93acacfb51fa0830
    [SerializeField] private float lifeTime = 4f;

    private TrailRenderer trailRenderer;
    private float lifeTimer;
    private bool hasHit = false;

    public int Damage => bulletDamage;
    public WeaponBehaviorType BehaviorType => behaviorType;
    public float AOERadius => aoeRadius;

    private void Awake()
    {
        trailRenderer = GetComponentInChildren<TrailRenderer>();
    }

    private void OnEnable()
    {
        hasHit = false;
        lifeTimer = lifeTime;

        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }

    private void Update()
    {
        // Bullets travel along the local left axis toward enemies.
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            DestroyOrReturnBullet();
        }
    }

    public void SetDamage(int dmg)
    {
        bulletDamage = dmg;
    }

    public void Configure(int dmg, WeaponBehaviorType behavior, float radius = 0f)
    {
        bulletDamage = dmg;
        behaviorType = behavior;
        if (radius > 0f) aoeRadius = radius;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
<<<<<<< HEAD
        if (hasHit) return;
=======
        // Only head and body hitboxes receive bullet damage.
        Hitbox targetHitbox = collision.GetComponent<Hitbox>();
>>>>>>> d8ee8a61bf1f885c5b6c1c0a93acacfb51fa0830

        // Only head and body hitboxes receive bullet damage.

        Hitbox targetHitbox = collision.GetComponent<Hitbox>();
        BaseEnemy directEnemy = collision.GetComponent<BaseEnemy>() ?? collision.GetComponentInParent<BaseEnemy>();

        if (targetHitbox != null || collision.CompareTag("Enemy") || directEnemy != null)
        {
            hasHit = true;

            Vector2 hitPoint = transform.position;
            Vector2 hitDirection = -transform.right; // ทิศทางกระสุนพุ่งตาม Vector3.left ของ Local Space

            if (behaviorType == WeaponBehaviorType.AOE)
            {
                ExplodeAOE(hitPoint, hitDirection);
            }
            else
            {
                // Single target hit (เป้าหมายเดี่ยว) พร้อมส่ง hitPoint และ hitDirection สำหรับ Blood VFX
                if (targetHitbox != null)
                {
                    targetHitbox.TakeHit(bulletDamage, hitPoint, hitDirection);
                }
                else if (directEnemy != null)
                {
                    directEnemy.TakeDamage(bulletDamage, hitPoint, hitDirection);
                }
            }

            DestroyOrReturnBullet();
        }
    }

    private void ExplodeAOE(Vector2 explosionCenter, Vector2 hitDirection)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(explosionCenter, aoeRadius);
        HashSet<BaseEnemy> hitEnemies = new HashSet<BaseEnemy>();

        foreach (Collider2D col in colliders)
        {
            Hitbox hb = col.GetComponent<Hitbox>();
            if (hb != null && hb.MainEnemy != null)
            {
                if (!hitEnemies.Contains(hb.MainEnemy))
                {
                    hb.TakeHit(bulletDamage, col.transform.position, hitDirection);
                    hitEnemies.Add(hb.MainEnemy);
                }
                continue;
            }

            BaseEnemy enemy = col.GetComponent<BaseEnemy>() ?? col.GetComponentInParent<BaseEnemy>();
            if (enemy != null && !hitEnemies.Contains(enemy))
            {
                enemy.TakeDamage(bulletDamage, col.transform.position, hitDirection);
                hitEnemies.Add(enemy);
            }
        }

        Debug.Log($"💥 [AOE Explosion] ระเบิดรัศมี {aoeRadius}! โดนศัตรู {hitEnemies.Count} ตัว (ดาเมจ: {bulletDamage})");
    }

    private void DestroyOrReturnBullet()
    {
        if (BulletPool.Instance != null)
        {
            if (trailRenderer != null)
            {
                trailRenderer.Clear();
            }
            BulletPool.Instance.ReturnBullet(gameObject);
        }
        else
        {
            if (trailRenderer != null)
            {
                // แยก TrailRenderer ออกให้ค่อยๆ จางหายไปเอง ไม่ขาดวิ่นตอน Destroy
                trailRenderer.transform.SetParent(null);
                trailRenderer.autodestruct = true;
            }
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (behaviorType == WeaponBehaviorType.AOE)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, aoeRadius);
        }
    }
}
