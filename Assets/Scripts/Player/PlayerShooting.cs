using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("ตั้งค่าการยิง")]
    [SerializeField] private GameObject bulletPrefab; // ไฟล์ Prefab กระสุน
    [SerializeField] private Transform firePoint;     // จุดปล่อยกระสุนปลายปืน

    private void Update()
    {
        // ตรวจจับการกดคลิกเมาส์ซ้าย (0 คือเมาส์ซ้าย)
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        // กันเหนียวกรณีลืมใส่ตัวแปร
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("ลืมใส่ Bullet Prefab หรือ Fire Point ใน Inspector หรือเปล่า!");
            return;
        }

        // สั่งสร้างกระสุนออกมาที่ตำแหน่ง (Position) และองศาการหมุน (Rotation) ของ FirePoint พอดี
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}