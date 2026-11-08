using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 15f;

    [Header("ตั้งค่าความแรงกระสุน")]
    [SerializeField] private int bulletDamage = 10;

    [Header("อายุขัยกระสุน (วินาที)")]
    [SerializeField] private float lifeTime = 4f;

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ล็อกเป้าหาคลาส Hitbox ที่อยู่บน Head_Hitbox หรือ Body_Hitbox เท่านั้น
        Hitbox targetHitbox = collision.GetComponent<Hitbox>();

        if (targetHitbox != null)
        {
            targetHitbox.TakeHit(bulletDamage);
            Destroy(gameObject);
        }
    }
}
