using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab; // Bullet prefab.
    [SerializeField] private Transform firePoint;     // Projectile spawn point at the muzzle.

    [Header("Single-Shot Timing")]
    [Tooltip("Minimum time in seconds between shots.")]
    [SerializeField] private float fireRate = 0.25f; 

    private float nextFireTime = 0f;

    private Equipment equipment;
    private void Awake() => equipment = GetComponent<Equipment>();

    private void Update()
    {
        if (Time.timeScale == 0f || (WaveManager.Instance != null && WaveManager.Instance.PauseBlocksInput)) return;
        if (equipment != null && equipment.CurrentWeapon != null)
        {
            Weapon weapon = equipment.CurrentWeapon;
            bool automatic = weapon.Stats != null && weapon.Stats.type == WeaponType.AssaultRifle;
            if (automatic ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0)) weapon.Fire();
            if (Input.GetKeyDown(KeyCode.R)) equipment.CurrentWeapon.Reload();
            return;
        }
        // Fire once per click, subject to the fire interval.
        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (firePoint == null)
        {
            Debug.LogWarning("Assign a Fire Point in the Inspector before shooting.");
            return;
        }

        // Get a bullet from the FIFO pool.
        if (BulletPool.Instance != null)
        {
            BulletPool.Instance.SpawnBullet(firePoint.position, firePoint.rotation);
            return;
        }

        // Instantiate a bullet when there is no pool.
        if (bulletPrefab != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
}
