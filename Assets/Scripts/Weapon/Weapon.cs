using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] private WeaponStats weaponStats;
    public int currentLevel = 1;

<<<<<<< HEAD
    [Header("พฤติกรรมอาวุธ (Single vs AOE)")]
    [SerializeField] private WeaponBehaviorType behaviorType = WeaponBehaviorType.Single;
    [SerializeField] private float aoeRadius = 2.5f;

    [Header("ส่วนประกอบปืน")]
=======
    [Header("Weapon Components")]
>>>>>>> d8ee8a61bf1f885c5b6c1c0a93acacfb51fa0830
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    // --- Data Structure ตามโจทย์อาจารย์: Struct + Dictionary ---
    // 1. Dictionary เก็บ WeaponStatus Struct ตามเลเวล (Key: int level -> Value: WeaponStatus struct)
    private Dictionary<int, WeaponStatus> statusByLevelDict = new Dictionary<int, WeaponStatus>();

    // 2. Dictionary เก็บสถานะปัจจุบัน (Key: string statName -> Value: object)
    private Dictionary<string, object> currentStatusDict = new Dictionary<string, object>();

    // ตัวแปรสเตตัสปัจจุบันในรูปแบบ struct
    private WeaponStatus currentStatus;

    private int currentAmmo;
    private bool isReloading = false;
    private float nextFireTime = 0f;

    public WeaponStats Stats => weaponStats;
    public WeaponStatus CurrentStatus => currentStatus;
    public Dictionary<int, WeaponStatus> StatusByLevelDict => statusByLevelDict;
    public Dictionary<string, object> CurrentStatusDict => currentStatusDict;

    public string WeaponName => currentStatus.weaponName;
    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => currentStatus.maxAmmo > 0 ? currentStatus.maxAmmo : (weaponStats != null ? weaponStats.GetMaxAmmo(currentLevel) : 12);
    public int CurrentDamage => currentStatus.damage > 0 ? currentStatus.damage : (weaponStats != null ? weaponStats.GetDamage(currentLevel) : 10);
    public float CurrentReloadTime => currentStatus.reloadTime > 0 ? currentStatus.reloadTime : (weaponStats != null ? weaponStats.GetReloadTime(currentLevel) : 1.5f);
    public WeaponBehaviorType BehaviorType => currentStatus.behaviorType;
    public float AOERadius => currentStatus.aoeRadius;
    public bool IsReloading => isReloading;

    private void Awake()
    {
        InitializeStatusDictionary();
    }

<<<<<<< HEAD
    // สร้างข้อมูลโครงสร้าง Dictionary สำหรับทุกระดับเลเวล
    public void InitializeStatusDictionary()
    {
        statusByLevelDict.Clear();
        int maxLvl = weaponStats != null ? weaponStats.maxLevel : 5;

        for (int lvl = 1; lvl <= maxLvl; lvl++)
        {
            if (weaponStats != null)
            {
                statusByLevelDict[lvl] = weaponStats.GenerateStatus(lvl);
            }
            else
            {
                statusByLevelDict[lvl] = new WeaponStatus(
                    gameObject.name,
                    lvl,
                    10 + (lvl - 1) * 5,
                    12 + (lvl - 1) * 3,
                    0.25f,
                    1.5f,
                    100 * lvl,
                    behaviorType,
                    aoeRadius
                );
            }
        }

        UpdateCurrentStatusFromDictionary();
    }

    private void UpdateCurrentStatusFromDictionary()
    {
        if (statusByLevelDict.TryGetValue(currentLevel, out WeaponStatus status))
        {
            currentStatus = status;
        }
        else
        {
            currentStatus = new WeaponStatus(gameObject.name, currentLevel, 10, 12, 0.25f, 1.5f, 100, behaviorType, aoeRadius);
        }

        // แปลงสถานะเป็น Dictionary<string, object>
        currentStatusDict = currentStatus.ToDictionary();
        currentAmmo = currentStatus.maxAmmo;
    }

=======
>>>>>>> d8ee8a61bf1f885c5b6c1c0a93acacfb51fa0830
    private void OnDisable() => isReloading = false;

    public bool CanFire()
    {
        if (isReloading) return false;
        if (Time.time < nextFireTime) return false;
        if (currentAmmo <= 0) return false;
        return true;
    }

    public void Fire()
    {
        if (!CanFire())
        {
            if (currentAmmo <= 0 && !isReloading)
            {
                StartCoroutine(ReloadRoutine());
            }
            return;
        }

        if (firePoint == null) return;

        float fireInterval = currentStatus.fireInterval > 0 ? currentStatus.fireInterval : 0.25f;
        nextFireTime = Time.time + fireInterval;

        int pellets = weaponStats != null && weaponStats.type == WeaponType.Shotgun ? Mathf.Max(1, weaponStats.pelletCount) : 1;
        for (int i = 0; i < pellets; i++)
<<<<<<< HEAD
        {
            float angle = pellets == 1 ? 0f : weaponStats.spreadAngle * ((float)i / (pellets - 1) - 0.5f);
            FireProjectile(firePoint.rotation * Quaternion.Euler(0f, 0f, angle));
=======
        {
            float angle = pellets == 1 ? 0f : weaponStats.spreadAngle * ((float)i / (pellets - 1) - 0.5f);
            FireProjectile(firePoint.rotation * Quaternion.Euler(0f, 0f, angle));
        }

        currentAmmo--;

        if (currentAmmo <= 0)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    private void FireProjectile(Quaternion rotation)
    {
        GameObject bulletObj = null;
        if (BulletPool.Instance != null)
        {
            bulletObj = BulletPool.Instance.SpawnBullet(firePoint.position, rotation);
        }
        else if (bulletPrefab != null)
        {
            bulletObj = Instantiate(bulletPrefab, firePoint.position, rotation);
        }

        if (bulletObj != null)
        {
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.SetDamage(CurrentDamage);
            }
>>>>>>> d8ee8a61bf1f885c5b6c1c0a93acacfb51fa0830
        }
    }

    private void FireProjectile(Quaternion rotation)
    {
        GameObject bulletObj = null;
        if (BulletPool.Instance != null)
        {
            bulletObj = BulletPool.Instance.SpawnBullet(firePoint.position, rotation);
        }
        else if (bulletPrefab != null)
        {
            bulletObj = Instantiate(bulletPrefab, firePoint.position, rotation);
        }

        if (bulletObj != null)
        {
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                // กำหนดดาเมจและพฤติกรรม (Single vs AOE) ให้ลูกกระสุน
                bullet.Configure(CurrentDamage, CurrentStatus.behaviorType, CurrentStatus.aoeRadius);
            }
        }
    }

    public void Reload()
    {
        if (!isReloading && currentAmmo < MaxAmmo)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(CurrentReloadTime);
        currentAmmo = MaxAmmo;
        isReloading = false;
    }

    public bool Upgrade()
    {
        int maxLvl = weaponStats != null ? weaponStats.maxLevel : 5;
        if (currentLevel >= maxLvl) return false;

        currentLevel++;
        UpdateCurrentStatusFromDictionary();
        Debug.Log($"[Weapon] อัปเกรดสำเร็จ! เลเวล: {currentLevel} ดาเมจ: {CurrentDamage} พฤติกรรม: {BehaviorType}");
        return true;
    }

    public int GetNextUpgradeCost()
    {
<<<<<<< HEAD
        if (weaponStats == null) return 0;
        if (currentLevel >= weaponStats.maxLevel) return 0;

        if (statusByLevelDict.TryGetValue(currentLevel, out WeaponStatus status))
        {
            return status.upgradeCost;
        }

=======
        if (weaponStats == null || currentLevel >= weaponStats.maxLevel) return 0;
>>>>>>> d8ee8a61bf1f885c5b6c1c0a93acacfb51fa0830
        return weaponStats.GetUpgradeCost(currentLevel);
    }
}

