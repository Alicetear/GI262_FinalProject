using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 15f;

    [Header("ตั้งค่าความแรงกระสุน")]
    [SerializeField] private int bulletDamage = 10;

    [Header("พฤติกรรมกระสุน (Single vs AOE)")]
    [SerializeField] private WeaponBehaviorType behaviorType = WeaponBehaviorType.Single;
    [SerializeField] private float aoeRadius = 2.5f;

    [Header("อายุขัยกระสุน (วินาที)")]
    [SerializeField] private float lifeTime = 4f;

    public int Damage => bulletDamage;
    public WeaponBehaviorType BehaviorType => behaviorType;
    public float AOERadius => aoeRadius;

    private void OnEnable()
    {
        // รีเซ็ต TrailRenderer เพื่อไม่ให้ลากเส้นค้างเมื่อดึงออกจาก Pool
        TrailRenderer trail = GetComponentInChildren<TrailRenderer>();
        if (trail != null)
        {
            trail.Clear();
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // กระสุนพุ่งออกไปทางซ้าย (หาเป้าหมายซอมบี้)
        transform.Translate(Vector3.left * speed * Time.deltaTime);
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
        Hitbox targetHitbox = collision.GetComponent<Hitbox>();

        if (targetHitbox != null || collision.CompareTag("Enemy"))
        {
            // คำนวณจุดปะทะและทิศทางกระสุนส่งไปสร้าง Blood VFX
            Vector2 hitPoint = collision.ClosestPoint(transform.position);
            if (hitPoint == Vector2.zero) hitPoint = transform.position;

            Vector2 bulletDir = Vector2.left; // ทิศทางกระสุนพุ่งไปทางซ้าย

            if (behaviorType == WeaponBehaviorType.AOE)
            {
                ExplodeAOE();
            }
            else
            {
                // Single target hit (เป้าหมายเดี่ยว)
                if (targetHitbox != null)
                {
                    targetHitbox.TakeHit(bulletDamage, hitPoint, bulletDir);
                }
                else
                {
                    BaseEnemy enemy = collision.GetComponent<BaseEnemy>() ?? collision.GetComponentInParent<BaseEnemy>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(bulletDamage, hitPoint, bulletDir);
                    }
                }
            }

            DestroyOrReturnBullet();
        }
    }

    private void ExplodeAOE()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
        HashSet<BaseEnemy> hitEnemies = new HashSet<BaseEnemy>();

        foreach (Collider2D col in colliders)
        {
            Vector2 hitPt = col.ClosestPoint(transform.position);
            Vector2 explodeDir = (hitPt - (Vector2)transform.position).normalized;
            if (explodeDir == Vector2.zero) explodeDir = Vector2.left;

            Hitbox hb = col.GetComponent<Hitbox>();
            if (hb != null && hb.MainEnemy != null)
            {
                if (!hitEnemies.Contains(hb.MainEnemy))
                {
                    hb.TakeHit(bulletDamage, hitPt, explodeDir);
                    hitEnemies.Add(hb.MainEnemy);
                }
                continue;
            }

            BaseEnemy enemy = col.GetComponent<BaseEnemy>();
            if (enemy != null && !hitEnemies.Contains(enemy))
            {
                enemy.TakeDamage(bulletDamage, hitPt, explodeDir);
                hitEnemies.Add(enemy);
            }
        }

        Debug.Log($"💥 [AOE Explosion] ระเบิดรัศมี {aoeRadius}! โดนศัตรู {hitEnemies.Count} ตัว (ดาเมจ: {bulletDamage})");
    }

    private void DestroyOrReturnBullet()
    {
        // ปลด TrailRenderer ชั่วคราวไม่ให้เส้นกระตุก/เพี้ยนข้ามหน้าจอเวลาทำลายหรือคืน Pool
        TrailRenderer trail = GetComponentInChildren<TrailRenderer>();
        if (trail != null)
        {
            trail.transform.SetParent(null);
            trail.autodestruct = true;
        }

        if (BulletPool.Instance != null)
        {
            if (trail != null) trail.Clear();
            BulletPool.Instance.ReturnBullet(gameObject);
        }
        else
        {
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
