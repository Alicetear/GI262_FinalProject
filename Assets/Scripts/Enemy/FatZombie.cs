using UnityEngine;

// สืบทอดฟังก์ชันเดิน โดนยิง และตีกำแพงมาจาก BaseEnemy
public class FatZombie : BaseEnemy
{
    [Header("สเตตัสเฉพาะตัว: ซอมบี้อ้วน")]
    [SerializeField] private float fatMoveSpeed = 1.0f;   // เดินช้ากว่าปกติ
    [SerializeField] private int fatMaxHealth = 100;      // เลือดเยอะตาม Class Diagram
    [SerializeField] private int fatAttackDamage = 25;    // ตีกำแพงแรงมาก (รั้วพังเร็ว)

    protected override void Start()
    {
        // 1. นำค่าเฉพาะตัวของซอมบี้อ้วน ไปทับค่าพื้นฐานใน BaseEnemy
        moveSpeed = fatMoveSpeed;
        maxHealth = fatMaxHealth;
        attackDamage = fatAttackDamage;

        // 2. เรียกใช้ Start ของคลาสแม่ (BaseEnemy) เพื่อรีเซ็ตเลือดและเตรียม Rigidbody
        base.Start();

        Debug.Log("Fat Zombie เกิดแล้ว! เลือด: " + currentHealth);
    }

    // ถ้าอยากให้ซอมบี้อ้วนมีเอฟเฟกต์พิเศษตอนตาย (เช่น ระเบิดทำดาเมจ) สามารถ Override ฟังก์ชัน Die() ได้ที่นี่
    /*
    protected override void Die()
    {
        Debug.Log("ซอมบี้อ้วนตาย! ตู้มมม!");
        // โค้ดเอฟเฟกต์ระเบิด...
        
        base.Die(); // สั่งให้ WaveManager นับจำนวนซอมบี้ตายตามปกติ
    }
    */
}
