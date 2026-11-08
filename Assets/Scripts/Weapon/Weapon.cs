using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("การตั้งค่าสเตตัส")]
    [SerializeField] private WeaponStats weaponStats;
    public int currentLevel = 1;

    [Header("ส่วนประกอบปืน")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    private int currentAmmo;
    private bool isReloading = false;
    private float nextFireTime = 0f;

    public WeaponStats Stats => weaponStats;
    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => weaponStats != null ? weaponStats.GetMaxAmmo(currentLevel) : 12;
    public int CurrentDamage => weaponStats != null ? weaponStats.GetDamage(currentLevel) : 10;
    public float CurrentReloadTime => weaponStats != null ? weaponStats.GetReloadTime(currentLevel) : 1.5f;
    public bool IsReloading => isReloading;

    private void Awake()
    {
        if (weaponStats != null)
        {
            currentAmmo = weaponStats.GetMaxAmmo(currentLevel);
        }
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

        float fireInterval = weaponStats != null ? weaponStats.GetFireInterval(currentLevel) : 0.25f;
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
                bullet.SetDamage(CurrentDamage);
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
        if (weaponStats == null) return false;
        if (currentLevel >= weaponStats.maxLevel) return false;

        currentLevel++;
        currentAmmo = MaxAmmo;
        return true;
    }

    public int GetNextUpgradeCost()
    {
        if (weaponStats == null) return 0;
        return weaponStats.GetUpgradeCost(currentLevel);
    }
}

