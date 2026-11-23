using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GunCheckpoint
{
    public int type, level, ammo;
}

[Serializable]
public class KitCheckpoint
{
    public int primary, secondary, active;
    public List<GunCheckpoint> weapons = new List<GunCheckpoint>();

    public bool IsValid(List<int> unlocked)
    {
        if (weapons == null || weapons.Count != 3 || !unlocked.Contains(primary) ||
            (secondary != -1 && !unlocked.Contains(secondary)) || primary == secondary ||
            (active != primary && active != secondary) || active < 0) return false;
        var types = new HashSet<int>();
        foreach (var gun in weapons)
            if (gun == null || !Enum.IsDefined(typeof(WeaponType), gun.type) || gun.level < 1 ||
                gun.ammo < 0 || !types.Add(gun.type)) return false;
        return true;
    }
}

[Serializable]
public class RunCheckpoint
{
    private const string Key = "Outpost7.ManualCheckpoint.v1";
    public const int CampaignNights = 7;
    public static bool ResumeRequested;
    public int version = 1;
    public int nights, wave, cash, scrap, rations, score, wallHP, wallMax;
    public bool shopping;
    public Vector3 position;
    public KitCheckpoint player;
    public List<KitCheckpoint> guards = new List<KitCheckpoint>();
    public List<int> unlocked = new List<int>();

    private bool IsValid()
    {
        if (version != 1 || nights < 1 || nights > CampaignNights || wave < 0 || wave >= nights ||
            (shopping && wave + 1 >= nights) || wallHP < 1 || wallHP > wallMax ||
            cash < 0 || scrap < 0 || rations < 0 || unlocked == null ||
            !unlocked.Contains((int)WeaponType.Pistol) || guards == null || guards.Count > 3 ||
            player == null || !player.IsValid(unlocked)) return false;
        return guards.TrueForAll(guard => guard != null && guard.IsValid(unlocked));
    }

    public static bool TryLoad(out RunCheckpoint data)
    {
        data = null;
        if (!PlayerPrefs.HasKey(Key)) return false;
        try { data = JsonUtility.FromJson<RunCheckpoint>(PlayerPrefs.GetString(Key)); }
        catch (ArgumentException) { return false; }
        if (data == null || !data.IsValid()) return false;
        // Shorter runs from previous versions continue toward the full campaign.
        data.nights = CampaignNights;
        return true;
    }

    public static void Store(RunCheckpoint data)
    {
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }
}
