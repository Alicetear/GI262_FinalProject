using UnityEngine;
using UnityEngine.Serialization;

public class Equipment : MonoBehaviour
{
    [Header("Weapon instances for this character")]
    [FormerlySerializedAs("primaryWeapon"), SerializeField] private Weapon firstWeapon;
    [FormerlySerializedAs("secondaryWeapon"), SerializeField] private Weapon secondWeapon;
    [SerializeField] private Weapon reserveWeapon;
    [SerializeField] private bool aiControlled;

    public Weapon PrimaryWeapon { get; private set; }
    public Weapon SecondaryWeapon { get; private set; }
    public Weapon CurrentWeapon { get; private set; }
    public event System.Action Changed;

    private void Awake()
    {
        foreach (var weapon in new[] { firstWeapon, secondWeapon, reserveWeapon })
        {
            if (weapon == null) continue;
            weapon.gameObject.SetActive(false);
            if (weapon.Stats != null && weapon.Stats.type == WeaponType.Pistol)
                PrimaryWeapon = weapon;
        }
        EquipWeapon(PrimaryWeapon);
    }

    public KitCheckpoint Capture()
    {
        var kit = new KitCheckpoint { primary = (int)PrimaryWeapon.Stats.type,
            secondary = SecondaryWeapon == null ? -1 : (int)SecondaryWeapon.Stats.type,
            active = (int)CurrentWeapon.Stats.type };
        foreach (var w in new[] {firstWeapon, secondWeapon, reserveWeapon})
            if (w != null) kit.weapons.Add(new GunCheckpoint { type = (int)w.Stats.type, level = w.currentLevel, ammo = w.CurrentAmmo });
        return kit;
    }

    public void Restore(KitCheckpoint kit)
    {
        foreach (var gun in kit.weapons)
            FindWeapon((WeaponType)gun.type)?.RestoreStats(gun.level, gun.ammo);
        PrimaryWeapon = FindWeapon((WeaponType)kit.primary);
        SecondaryWeapon = FindWeapon((WeaponType)kit.secondary);
        EquipWeapon(FindWeapon((WeaponType)kit.active));
    }

    private void Update()
    {
        if (aiControlled || (WaveManager.Instance != null && WaveManager.Instance.PauseBlocksInput)) return;
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon(PrimaryWeapon);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon(SecondaryWeapon);
        else if (Input.GetKeyDown(KeyCode.Q)) SwitchWeapon();
    }

    // The armory assigns a weapon to this character. Combat input only selects occupied slots.
    public void SelectWeapon(WeaponType type)
    {
        if (type != WeaponType.Pistol && (ShopManager.Instance == null || !ShopManager.Instance.IsUnlocked(type))) return;
        Weapon wanted = FindWeapon(type);
        if (wanted == null) return;
        if (wanted != PrimaryWeapon && wanted != SecondaryWeapon)
        {
            if (PrimaryWeapon == null) PrimaryWeapon = wanted;
            else if (SecondaryWeapon == null) SecondaryWeapon = wanted;
            else if (CurrentWeapon == PrimaryWeapon) PrimaryWeapon = wanted;
            else SecondaryWeapon = wanted;
        }
        EquipWeapon(wanted);
    }

    private Weapon FindWeapon(WeaponType type)
    {
        if (firstWeapon != null && firstWeapon.Stats != null && firstWeapon.Stats.type == type) return firstWeapon;
        if (secondWeapon != null && secondWeapon.Stats != null && secondWeapon.Stats.type == type) return secondWeapon;
        if (reserveWeapon != null && reserveWeapon.Stats != null && reserveWeapon.Stats.type == type) return reserveWeapon;
        return null;
    }

    public void SwitchWeapon() => EquipWeapon(CurrentWeapon == PrimaryWeapon ? SecondaryWeapon : PrimaryWeapon);

    public void EquipWeapon(Weapon weapon)
    {
        if (weapon == null || weapon == CurrentWeapon || (weapon != PrimaryWeapon && weapon != SecondaryWeapon)) return;
        if (CurrentWeapon != null) CurrentWeapon.gameObject.SetActive(false);
        CurrentWeapon = weapon;
        weapon.gameObject.SetActive(true);
        Changed?.Invoke();
    }
}
