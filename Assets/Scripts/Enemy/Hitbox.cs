using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("ตั้งค่าชิ้นส่วน")]
    [SerializeField] private BaseEnemy mainEnemy; // ลากตัวแม่มาใส่
    [SerializeField] private float damageMultiplier = 1.0f; // ตัวคูณดาเมจ (เช่น หัว = 2.0, ตัว = 1.0)

    public BaseEnemy MainEnemy
    {
        get
        {
            if (mainEnemy == null) mainEnemy = GetComponentInParent<BaseEnemy>();
            return mainEnemy;
        }
    }

    private void Awake()
    {
        if (mainEnemy == null)
        {
            mainEnemy = GetComponentInParent<BaseEnemy>();
        }
    }

    // ฟังก์ชันนี้จะถูกเรียกจากกระสุนปืน พร้อมตำแหน่งปะทะและทิศทางกระสุนสำหรับ Blood VFX
    public void TakeHit(int baseDamage, Vector2 hitPoint, Vector2 hitDirection)
    {
        if (mainEnemy == null)
        {
            mainEnemy = GetComponentInParent<BaseEnemy>();
        }

        // คำนวณดาเมจใหม่ (ดาเมจพื้นฐาน x ตัวคูณ)
        int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);

        if (mainEnemy != null)
        {
            Debug.Log($"ยิงโดน {gameObject.name}! ดาเมจที่ทำได้: {finalDamage}");
            mainEnemy.TakeDamage(finalDamage, hitPoint, hitDirection);
        }
        else
        {
            Debug.LogWarning($"[Hitbox] ไม่พบ BaseEnemy บน {gameObject.name} หรือ Parent!");
        }
    }

    // โอเวอร์โหลดเดิมเพื่อความเข้ากันได้
    public void TakeHit(int baseDamage)
    {
        TakeHit(baseDamage, transform.position, Vector2.left);
    }
}
