using System.Collections.Generic;
using UnityEngine;

<<<<<<< HEAD
public enum WeaponBehaviorType
{
    Single,
    AOE
}

public enum WeaponType { Pistol, Shotgun, AssaultRifle }

[System.Serializable]
public struct WeaponStatus
{
    public string weaponName;
    public int level;
    public int damage;
    public int maxAmmo;
    public float fireInterval;
    public float reloadTime;
    public int upgradeCost;
    public WeaponBehaviorType behaviorType;
    public float aoeRadius;

    public WeaponStatus(string weaponName, int level, int damage, int maxAmmo, float fireInterval, float reloadTime, int upgradeCost, WeaponBehaviorType behaviorType = WeaponBehaviorType.Single, float aoeRadius = 0f)
    {
        this.weaponName = weaponName;
        this.level = level;
        this.damage = damage;
        this.maxAmmo = maxAmmo;
        this.fireInterval = fireInterval;
        this.reloadTime = reloadTime;
        this.upgradeCost = upgradeCost;
        this.behaviorType = behaviorType;
        this.aoeRadius = aoeRadius;
    }

    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { "WeaponName", weaponName },
            { "Level", level },
            { "Damage", damage },
            { "MaxAmmo", maxAmmo },
            { "FireInterval", fireInterval },
            { "ReloadTime", reloadTime },
            { "UpgradeCost", upgradeCost },
            { "BehaviorType", behaviorType },
            { "AOERadius", aoeRadius }
        };
    }
}

=======
public enum WeaponType { Pistol, Shotgun, AssaultRifle }

>>>>>>> d8ee8a61bf1f885c5b6c1c0a93acacfb51fa0830
[CreateAssetMenu(fileName = "NewWeaponStats", menuName = "Outpost7/Weapon Stats")]
public class WeaponStats : ScriptableObject
{
    [Header("Weapon Details")]
    public WeaponType type;
    [Min(1)] public int pelletCount = 5;
    [Range(0, 90)] public float spreadAngle = 24f;
    public string weaponName = "Pistol";
    public int maxLevel = 5;

<<<<<<< HEAD
    [Header("พฤติกรรมอาวุธ (Weapon Behavior: Single vs AOE)")]
    public WeaponBehaviorType behaviorType = WeaponBehaviorType.Single;
    public float aoeRadius = 2.5f;

=======
>>>>>>> d8ee8a61bf1f885c5b6c1c0a93acacfb51fa0830
    [Header("Damage Scaling")]
    public int baseDamage = 10;
    public float damageGrowthPerLevel = 5f;

    [Header("Fire Interval (Seconds per Shot)")]
    public float baseFireInterval = 0.35f;
    public float fireRateImprovementPerLevel = 0.03f;

    [Header("Magazine Capacity")]
    public int baseMaxAmmo = 12;
    public int ammoGrowthPerLevel = 3;

    [Header("Reload Time (Seconds)")]
    public float baseReloadTime = 1.8f;
    public float reloadReductionPerLevel = 0.15f;

    [Header("Upgrade Cost (Cash)")]
    public int baseUpgradeCost = 100;
    public float costMultiplier = 1.5f;

    public int GetDamage(int level)
    {
        return Mathf.RoundToInt(baseDamage + (level - 1) * damageGrowthPerLevel);
    }

    public int GetMaxAmmo(int level)
    {
        return baseMaxAmmo + (level - 1) * ammoGrowthPerLevel;
    }

    public float GetFireInterval(int level)
    {
        return Mathf.Max(0.08f, baseFireInterval - (level - 1) * fireRateImprovementPerLevel);
    }

    public float GetReloadTime(int level)
    {
        return Mathf.Max(0.5f, baseReloadTime - (level - 1) * reloadReductionPerLevel);
    }

    public int GetUpgradeCost(int level)
    {
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(costMultiplier, level - 1));
    }

    public WeaponStatus GenerateStatus(int level)
    {
        return new WeaponStatus(
            weaponName,
            level,
            GetDamage(level),
            GetMaxAmmo(level),
            GetFireInterval(level),
            GetReloadTime(level),
            GetUpgradeCost(level),
            behaviorType,
            aoeRadius
        );
    }
}

