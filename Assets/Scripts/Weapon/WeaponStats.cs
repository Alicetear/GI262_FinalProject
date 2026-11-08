using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponStats", menuName = "Outpost7/Weapon Stats")]
public class WeaponStats : ScriptableObject
{
    [Header("ข้อมูลอาวุธ")]
    public string weaponName = "Pistol";
    public int maxLevel = 5;

    [Header("พลังโจมตี (Damage Scaling)")]
    public int baseDamage = 10;
    public float damageGrowthPerLevel = 5f;

    [Header("อัตราการยิง (Fire Rate - วินาทีต่อนัด)")]
    public float baseFireInterval = 0.35f;
    public float fireRateImprovementPerLevel = 0.03f;

    [Header("ความจุกระสุน (Magazine Size)")]
    public int baseMaxAmmo = 12;
    public int ammoGrowthPerLevel = 3;

    [Header("เวลารีโหลด (Reload Time - วินาที)")]
    public float baseReloadTime = 1.8f;
    public float reloadReductionPerLevel = 0.15f;

    [Header("ค่าใช้จ่ายในการอัปเกรด (Cash Cost)")]
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
}

