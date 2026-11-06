using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private int damage = 10;

    private void Update()
    {
        // เปลี่ยนจาก right เป็น left กระสุนจะพุ่งออกไปหาเป้าหมาย
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // บรรทัดนี้จะแฉว่ากระสุนวิ่งไปชนกับ Object ชื่ออะไร และ Tag อะไร
        Debug.Log($"กระสุนชนกับ: {collision.gameObject.name} | Tag: {collision.tag}");

        if (collision.CompareTag("Enemy"))
        {
            BaseEnemy enemy = collision.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}