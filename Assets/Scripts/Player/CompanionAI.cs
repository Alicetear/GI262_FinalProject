using System.Collections.Generic;
using UnityEngine;

public class CompanionAI : MonoBehaviour
{
    [Header("การตั้งค่าอาวุธและตำแหน่ง")]
    [SerializeField] private Weapon assignedWeapon;
    [SerializeField] private Transform gunPivot;
    [SerializeField] private float detectionRange = 12f;

    [Header("เป้าหมายปัจจุบัน")]
    [SerializeField] private BaseEnemy currentTarget;

    public Weapon AssignedWeapon => assignedWeapon;

    private void Start()
    {
        if (assignedWeapon == null)
        {
            assignedWeapon = GetComponentInChildren<Weapon>();
        }
    }

    private void Update()
    {
        FindNearestTarget();

        if (currentTarget != null)
        {
            AimAtTarget(currentTarget.transform.position);
            AutoShoot();
        }
    }

    public void FindNearestTarget()
    {
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            currentTarget = null;
        }

        if (WaveManager.Instance == null) return;

        List<BaseEnemy> enemies = WaveManager.Instance.ActiveEnemies;
        if (enemies == null || enemies.Count == 0)
        {
            currentTarget = null;
            return;
        }

        BaseEnemy nearestEnemy = null;
        float minDistanceSqr = detectionRange * detectionRange;
        Vector3 myPos = transform.position;

        for (int i = 0; i < enemies.Count; i++)
        {
            BaseEnemy enemy = enemies[i];
            if (enemy == null) continue;

            float distSqr = (enemy.transform.position - myPos).sqrMagnitude;
            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                nearestEnemy = enemy;
            }
        }

        currentTarget = nearestEnemy;
    }

    private void AimAtTarget(Vector3 targetPos)
    {
        Transform aimTransform = gunPivot != null ? gunPivot : transform;
        Vector3 direction = targetPos - aimTransform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        aimTransform.rotation = Quaternion.Euler(0f, 0f, angle + 180f);
    }

    private void AutoShoot()
    {
        if (assignedWeapon != null)
        {
            assignedWeapon.Fire();
        }
    }
}

