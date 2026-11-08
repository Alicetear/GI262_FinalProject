using UnityEngine;

public class AcidSpit : MonoBehaviour
{
    [Header("สเตตัสกระสุนกรด")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 15;
    [SerializeField] private float lifeTime = 5f;

    private Vector2 moveDirection = Vector2.right;

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
        if (hitObj.CompareTag("Wall"))
        {
            Barricade barricade = hitObj.GetComponent<Barricade>();
            if (barricade != null)
            {
                barricade.TakeDamage(damage);
                Debug.Log($"กรดกัดกร่อนกำแพง! ดาเมจ: {damage}");
            }
            Destroy(gameObject);
            return;
        }

        if (hitObj.CompareTag("Player"))
        {
            Debug.Log($"ผู้เล่นโดนน้ำกรด! ดาเมจ: {damage}");
            Destroy(gameObject);
            return;
        }
    }
}

