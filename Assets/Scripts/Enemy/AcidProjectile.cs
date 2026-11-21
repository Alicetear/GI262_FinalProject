using UnityEngine;

public class AcidProjectile : MonoBehaviour
{
    [Header("สเตตัสก้อนกรด")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 15;
    [SerializeField] private float lifeTime = 5f;

    [Header("ทิศทางเริ่มต้น (พุ่งไปทางซ้าย หรือหา Barricade อัตโนมัติ)")]
    [SerializeField] private Vector2 defaultDirection = Vector2.left;
    [SerializeField] private bool autoTargetBarricade = true;

    [Header("เอฟเฟกต์กรดแตก (Acid Splash VFX)")]
    [SerializeField] private GameObject acidSplashVfxPrefab;

    private Vector2 moveDirection;
    private bool hasHit = false;

    public void SetDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.0001f)
        {
            moveDirection = direction.normalized;
        }
    }

    private void Awake()
    {
        moveDirection = defaultDirection.sqrMagnitude > 0.0001f ? defaultDirection.normalized : Vector2.left;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);

        if (autoTargetBarricade)
        {
            Barricade wall = Barricade.Instance != null ? Barricade.Instance : FindAnyObjectByType<Barricade>();
            if (wall != null)
            {
                Vector2 dir = ((Vector2)wall.transform.position - (Vector2)transform.position);
                if (dir.sqrMagnitude > 0.0001f)
                {
                    moveDirection = dir.normalized;
                }
            }
        }
    }

    private void Update()
    {
        transform.Translate((Vector3)(moveDirection * speed * Time.deltaTime), Space.World);
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

        Barricade barricade = hitObj.GetComponent<Barricade>();
        bool isBarricadeHit = hitObj.CompareTag("Wall") || barricade != null;

        if (isBarricadeHit)
        {
            hasHit = true;

            if (barricade == null)
            {
                barricade = hitObj.GetComponentInParent<Barricade>();
            }

            if (barricade != null)
            {
                barricade.TakeDamage(damage);
                Debug.Log($"AcidProjectile ชนกำแพง! ลดเลือด: {damage}");
            }

            if (acidSplashVfxPrefab != null)
            {
                GameObject vfx = Instantiate(acidSplashVfxPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 2.0f);
            }

            Destroy(gameObject);
        }
    }
}
