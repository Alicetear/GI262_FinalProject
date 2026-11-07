using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private int damage = 10;
    [Header("ตั้งค่าความแรงกระสุน")]
    // 💡 สิ่งที่ขาดไปคือบรรทัดนี้ครับ! ประกาศตัวแปรเก็บค่าดาเมจไว้ด้านบนสุด
    [SerializeField] private int bulletDamage = 10;
    private void Update()
    {
        // เปลี่ยนจาก right เป็น left กระสุนจะพุ่งออกไปหาเป้าหมาย
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ล็อกเป้าหาคลาส Hitbox ที่อยู่บน Head_Hitbox หรือ Body_Hitbox เท่านั้น
        Hitbox targetHitbox = collision.GetComponent<Hitbox>();

        if (targetHitbox != null)
        {
            // พอเราสร้างตัวแปรด้านบนแล้ว บรรทัดนี้จะทำงานได้ 100%
            targetHitbox.TakeHit(bulletDamage);
            Destroy(gameObject);
        }
    }

}
