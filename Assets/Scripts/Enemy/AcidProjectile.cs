using UnityEngine;

public class AcidProjectile : MonoBehaviour
{
    [Header("สเตตัสกระสุนกรด")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 15;
    [SerializeField] private float lifeTime = 5f;

    [Header("ทิศทางการพุ่ง")]
    [Tooltip("ทิศทางการพุ่งของกรด (ค่าเริ่มต้นคือพุ่งไปทาง Barricade)")]
    [SerializeField] private Vector2 flyDirection = Vector2.right;

    [Header("เอฟเฟกต์กรดแตก (Acid Splash VFX)")]
    [SerializeField] private GameObject acidSplashVfxPrefab;

    private Transform targetTransform;
    private bool hasHit = false;

    private void Start()
    {
        Destroy(gameObject, lifeTime);

        // หาเป้าหมาย Barricade อัตโนมัติถ้ายังไม่ได้กำหนด
        if (targetTransform == null)
        {
            Barricade barricade = Barricade.Instance != null ? Barricade.Instance : FindAnyObjectByType<Barricade>();
            if (barricade != null)
            {
                SetTarget(barricade.transform);
            }
            else
            {
                UpdateRotation();
            }
        }
        else
        {
            UpdateRotation();
        }
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    public void SetDirection(Vector2 direction)
    {
        flyDirection = direction.normalized;
        UpdateRotation();
    }

    public void SetTarget(Transform target)
    {
        targetTransform = target;
        if (targetTransform != null)
        {
            flyDirection = (targetTransform.position - transform.position).normalized;
            UpdateRotation();
        }
    }

    private void UpdateRotation()
    {
        if (flyDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(flyDirection.y, flyDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void Update()
    {
        transform.Translate(flyDirection * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void HandleHit(GameObject hitObj)
    {
        if (hasHit) return;

        // ตรวจสอบทั้ง Tag "Wall", "Barricade" หรือมีคอมโพเนนต์ Barricade
        if (hitObj.CompareTag("Wall") || hitObj.CompareTag("Barricade") || hitObj.GetComponent<Barricade>() != null || hitObj.GetComponentInParent<Barricade>() != null)
        {
            hasHit = true;
            Barricade barricade = hitObj.GetComponent<Barricade>() ?? hitObj.GetComponentInParent<Barricade>();
            if (barricade != null)
            {
                barricade.TakeDamage(damage);
                Debug.Log($"🧪 [AcidProjectile] กรดพุ่งชนกำแพง! ทำดาเมจ: {damage}");
            }

            SpawnSplashVFX();
            Destroy(gameObject);
            return;
        }

        // กรณีชนผู้เล่น
        if (hitObj.CompareTag("Player"))
        {
            hasHit = true;
            Debug.Log($"🧪 [AcidProjectile] กรดโดนผู้เล่น! ดาเมจ: {damage}");
            SpawnSplashVFX();
            Destroy(gameObject);
            return;
        }
    }

    private void SpawnSplashVFX()
    {
        if (acidSplashVfxPrefab != null)
        {
            GameObject splash = Instantiate(acidSplashVfxPrefab, transform.position, Quaternion.identity);
            Destroy(splash, 1.5f);
        }
    }
}
