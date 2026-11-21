using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("การตั้งค่าสเตตัส")]
    [SerializeField] private WeaponStats weaponStats;
    public int currentLevel = 1;

    [Header("พฤติกรรมอาวุธ (Single vs AOE)")]
    [SerializeField] private WeaponBehaviorType behaviorType = WeaponBehaviorType.Single;
    [SerializeField] private float aoeRadius = 2.5f;

    [Header("ส่วนประกอบปืน")]
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

        GameObject bulletObj = null;
        if (BulletPool.Instance != null)
        {
            bulletObj = BulletPool.Instance.SpawnBullet(firePoint.position, firePoint.rotation);
        }
        else if (bulletPrefab != null)
        {
            bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
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

        currentAmmo--;

        if (currentAmmo <= 0)
        {
            StartCoroutine(ReloadRoutine());
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
        if (statusByLevelDict.TryGetValue(currentLevel, out WeaponStatus status))
        {
            return status.upgradeCost;
        }
        return weaponStats != null ? weaponStats.GetUpgradeCost(currentLevel) : 100;
    }
}

