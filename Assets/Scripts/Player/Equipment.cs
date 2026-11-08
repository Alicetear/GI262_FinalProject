using UnityEngine;

public class Equipment : MonoBehaviour
{
    [Header("ช่องติดตั้งอาวุธ (Equipment Slots)")]
    [SerializeField] private Weapon primaryWeapon;
    [SerializeField] private Weapon secondaryWeapon;

    private Weapon currentWeapon;
    private PlayerShooting playerShooting;

    public Weapon CurrentWeapon => currentWeapon;
    public Weapon PrimaryWeapon => primaryWeapon;
    public Weapon SecondaryWeapon => secondaryWeapon;

    private void Start()
    {
        playerShooting = GetComponent<PlayerShooting>();

        if (primaryWeapon != null)
        {
            EquipWeapon(primaryWeapon);
            if (secondaryWeapon != null)
            {
                secondaryWeapon.gameObject.SetActive(false);
            }
        }
        else if (secondaryWeapon != null)
        {
            EquipWeapon(secondaryWeapon);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (primaryWeapon != null && currentWeapon != primaryWeapon)
            {
                EquipWeapon(primaryWeapon);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (secondaryWeapon != null && currentWeapon != secondaryWeapon)
            {
                EquipWeapon(secondaryWeapon);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchWeapon();
        }
    }

    public void SwitchWeapon()
    {
        if (currentWeapon == primaryWeapon && secondaryWeapon != null)
        {
            EquipWeapon(secondaryWeapon);
        }
        else if (primaryWeapon != null)
        {
            EquipWeapon(primaryWeapon);
        }
    }

    public void EquipWeapon(Weapon newWeapon)
    {
        if (newWeapon == null) return;

        if (currentWeapon != null && currentWeapon != newWeapon)
        {
            currentWeapon.gameObject.SetActive(false);
        }

        currentWeapon = newWeapon;
        currentWeapon.gameObject.SetActive(true);

        Debug.Log($"สลับมาถืออาวุธ: {currentWeapon.Stats?.weaponName ?? currentWeapon.name}");
    }
}

