using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("ตั้งค่าชิ้นส่วน")]
    [SerializeField] private BaseEnemy mainEnemy; // ลากตัวแม่มาใส่
    [SerializeField] private float damageMultiplier = 1.0f; // ตัวคูณดาเมจ (เช่น หัว = 2.0, ตัว = 1.0)

    // ฟังก์ชันนี้จะถูกเรียกจากกระสุนปืน
    public void TakeHit(int baseDamage)
    {
        // คำนวณดาเมจใหม่ (ดาเมจพื้นฐาน x ตัวคูณ)
        int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);

        if (mainEnemy != null)
        {
            Debug.Log($"ยิงโดน {gameObject.name}! ดาเมจที่ทำได้: {finalDamage}");
            mainEnemy.TakeDamage(finalDamage);
        }
    }
}