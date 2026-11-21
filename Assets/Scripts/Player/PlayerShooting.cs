using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("ตั้งค่าการยิง")]
    [SerializeField] private GameObject bulletPrefab; // ไฟล์ Prefab กระสุน
    [SerializeField] private Transform firePoint;     // จุดปล่อยกระสุนปลายปืน

    [Header("ควบคุมจังหวะการยิง (ทีละนัด)")]
    [Tooltip("หน่วงเวลาระหว่างนัด (วินาที) ป้องกันกระสุนออกซ้อนกัน")]
    [SerializeField] private float fireRate = 0.25f; 

    private float nextFireTime = 0f;
    private Equipment playerEquipment;
    private Weapon attachedWeapon;

    private void Start()
    {
        playerEquipment = GetComponent<Equipment>();
        attachedWeapon = GetComponentInChildren<Weapon>();
    }

    private void Update()
    {
        // 1 คลิก = 1 นัด พร้อมหน่วงเวลาตาม fireRate
        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        // ถ้ามี Weapon ที่ติดตั้งอยู่ ให้ยิงผ่าน Weapon เพื่อใช้ระบบ Struct + Dictionary + AOE/Single
        if (playerEquipment != null && playerEquipment.CurrentWeapon != null)
        {
            playerEquipment.CurrentWeapon.Fire();
            return;
        }

        if (attachedWeapon != null)
        {
            attachedWeapon.Fire();
            return;
        }

        if (firePoint == null)
        {
            Debug.LogWarning("ลืมใส่ Fire Point ใน Inspector หรือเปล่า!");
            return;
        }

        // นำระบบ Object Pooling มาใช้ (Queue FIFO)
        if (BulletPool.Instance != null)
        {
            BulletPool.Instance.SpawnBullet(firePoint.position, firePoint.rotation);
            return;
        }

        // Fallback กรณีไม่ได้ใช้ BulletPool
        if (bulletPrefab != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
}
