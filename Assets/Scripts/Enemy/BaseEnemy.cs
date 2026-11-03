using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] protected int hp = 30;
    [SerializeField] protected float speed = 2f;

    protected bool isMoving = true; // เพิ่มตัวแปรเช็กสถานะการเดิน

    protected virtual void Update()
    {
        Move();
    }

    protected virtual void Move()
    {
        // จะเดินก็ต่อเมื่อ isMoving เป็น true
        if (isMoving)
        {
            transform.Translate(Vector3.right * speed * Time.deltaTime);
        }
    }

    // ฟังก์ชันตรวจจับการชนแบบ Trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            isMoving = false; // สั่งให้หยุดเดิน
            Debug.Log($"{gameObject.name} ถึงกำแพงแล้ว! เตรียมโจมตีฐาน");
        }
    }

    private void OnMouseDown()
    {
        TakeDamage(10);
    }

    public virtual void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0) Die();
    }

    protected virtual void Die()
    {
        if (WaveManager.Instance != null) WaveManager.Instance.OnEnemyDied();
        Destroy(gameObject);
    }
}