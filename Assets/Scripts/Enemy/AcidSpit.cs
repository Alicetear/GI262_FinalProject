using UnityEngine;

public class AcidSpit : MonoBehaviour
{
    [Header("สเตตัสกระสุนกรด")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 15;
    [SerializeField] private float lifeTime = 5f;

    [Header("เอฟเฟกต์กรดแตก (AcidSplash_VFX)")]
    [SerializeField] private GameObject splashVfxPrefab;

    private Vector2 moveDirection = Vector2.right;
    private bool hasHit = false;

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.color == Color.white)
        {
            sr.color = new Color(0.2f, 1f, 0.2f, 1f);
        }
    }

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.GetComponentInParent<BaseEnemy>() != null) return;
        HandleHit(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.GetComponentInParent<BaseEnemy>() != null) return;
        HandleHit(collision.gameObject);
    }

    private void HandleHit(GameObject hitObj)
    {
        if (hasHit || hitObj == null) return;

        // ไม่ให้กระสุนกรดชนกับตัวซอมบี้เอง (Enemy / Hitbox)
        if (hitObj.CompareTag("Enemy") ||
            hitObj.GetComponent<BaseEnemy>() != null ||
            hitObj.GetComponentInParent<BaseEnemy>() != null ||
            hitObj.GetComponent<Hitbox>() != null)
        {
            return;
        }

        // สร้างดาเมจและระเบิดเอฟเฟกต์เฉพาะเมื่อชนกับกำแพง (Wall / Barricade) เท่านั้น
        if (hitObj.CompareTag("Wall") || hitObj.GetComponent<Barricade>() != null || hitObj.GetComponentInParent<Barricade>() != null)
        {
            hasHit = true;
            Barricade barricade = hitObj.GetComponent<Barricade>() ?? hitObj.GetComponentInParent<Barricade>();
            if (barricade != null)
            {
                barricade.TakeDamage(damage);
                Debug.Log($"กรดกัดกร่อนกำแพง! ดาเมจ: {damage}");
            }

            SpawnSplashVFX();
            Destroy(gameObject);
        }
    }

    private void SpawnSplashVFX()
    {
        if (splashVfxPrefab != null)
        {
            GameObject splash = Instantiate(splashVfxPrefab, transform.position, Quaternion.identity);
            Destroy(splash, 2.0f);
        }
    }
}

