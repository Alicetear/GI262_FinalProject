using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Barricade Repairs and Upgrades (Scrap)")]
    [SerializeField] private int repairScrapCost = 20;
    [SerializeField] private int repairHealthAmount = 250;
    [SerializeField] private int upgradeWallScrapCost = 50;
    [SerializeField] private int upgradeWallHealthAmount = 300;

    [Header("Companion Recruitment (Rations)")]
    [SerializeField] private int recruitRationCost = 2;
    [SerializeField] private GameObject companionPrefab;
    [SerializeField] private Transform[] companionStations; // Up to three guard posts.
    private List<GameObject> activeCompanions = new List<GameObject>();

    [Header("Shop Currency Display")]
    [SerializeField] private TextMeshProUGUI cashText;
    [SerializeField] private TextMeshProUGUI scrapText;
    [SerializeField] private TextMeshProUGUI rationsText;

    [Header("Barricade Health Display")]
    [Tooltip("Displays barricade health, for example: Wall HP: 750 / 1000 (75%).")]
    [SerializeField] private TextMeshProUGUI barricadeHpText;
    [Tooltip("Optional slider showing the fraction of barricade health remaining.")]
    [SerializeField] private Slider barricadeHpSlider;

    [SerializeField] private Equipment playerEquipment;
    [SerializeField] private Button upgradeWeaponButton, upgradeWallButton, recruitButton, repairButton;
    [SerializeField] private TextMeshProUGUI upgradeWeaponLabel, recruitLabel, weaponDetails;

    [SerializeField] private GameObject armoryPage, basePage;
    [SerializeField] private Button armoryTab, baseTab;
    [SerializeField] private Button[] ownerButtons, weaponButtons;
    [SerializeField] private TextMeshProUGUI[] weaponStates;
    [SerializeField] private Image[] rackWeapons;
    [SerializeField, Min(1)] private int shotgunPrice = 125;
    [SerializeField, Min(1)] private int assaultRiflePrice = 175;
    private readonly HashSet<WeaponType> unlockedWeapons = new HashSet<WeaponType> { WeaponType.Pistol };

    public bool IsUnlocked(WeaponType type) => unlockedWeapons.Contains(type);

    private int UnlockPrice(WeaponType type) => type == WeaponType.AssaultRifle ? assaultRiflePrice : shotgunPrice;
    private int selectedOwner;

    private Equipment SelectedEquipment => selectedOwner == 0 ? playerEquipment :
        activeCompanions[selectedOwner - 1].GetComponent<Equipment>();

    public void ShowArmory(bool show)
    {
        armoryPage.SetActive(show);
        basePage.SetActive(!show);
        armoryTab.interactable = !show;
        baseTab.interactable = show;
        UpdateShopUI();
    }

    public void SelectOwner(int index)
    {
        if (index < 0 || index > activeCompanions.Count) return;
        selectedOwner = index;
        UpdateShopUI();
    }

    public void SelectArmoryWeapon(int index)
    {
        var type = (WeaponType)index;
        if (index < 0 || index >= weaponButtons.Length) return;
        if (!IsUnlocked(type))
        {
            if (!GameManager.Instance.SpendCurrency(CurrencyType.Cash, UnlockPrice(type))) return;
            unlockedWeapons.Add(type);
        }
        else SelectedEquipment.SelectWeapon(type);
        UpdateShopUI();
    }

    private void UpdateArmory(Weapon selected)
    {
        if (weaponButtons == null || selected == null) return;
        for (int i = 0; i < ownerButtons.Length; i++)
        {
            bool available = i <= activeCompanions.Count;
            ownerButtons[i].interactable = available && i != selectedOwner;
            string name = i == 0 ? "Player" : $"Post {i}";
            ownerButtons[i].GetComponentInChildren<TextMeshProUGUI>(true).text =
                !available ? name + " / Vacant" : i == selectedOwner ? "[" + name + "]" : name;
        }
        for (int i = 0; i < weaponButtons.Length; i++)
        {
            var type = (WeaponType)i;
            bool locked = !IsUnlocked(type);
            bool equipped = selected.Stats.type == type;
            rackWeapons[i].color = locked ? new Color32(24, 21, 17, 104) : Color.white;
            var kit = SelectedEquipment;
            bool inLoadout = kit.PrimaryWeapon?.Stats.type == type || kit.SecondaryWeapon?.Stats.type == type;
            var state = weaponStates[i];
            state.text = locked ? "LOCKED" : equipped ? "EQUIPPED" : inLoadout ? "HOLSTERED" : "UNLOCKED";
            state.color = locked ? new Color32(217,158,120,255) : equipped ? new Color32(167,202,162,255) : new Color32(207,203,188,255);
            var button = weaponButtons[i];
            button.interactable = locked ? GameManager.Instance.Cash >= UnlockPrice(type) : !equipped;
            button.GetComponentInChildren<TextMeshProUGUI>(true).text = locked ? $"Unlock / ${UnlockPrice(type)}" : equipped ? "In use" : inLoadout ? "Switch" : "Equip";
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrencyChanged += UpdateShopUI;
        }

        if (playerEquipment != null) playerEquipment.Changed += UpdateShopUI;
        UpdateShopUI();
    }

    private void OnDestroy()
    {
        if (playerEquipment != null) playerEquipment.Changed -= UpdateShopUI;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrencyChanged -= UpdateShopUI;
        }
    }

    private void OnEnable()
    {
        // Refresh balances and barricade health when the morning shop opens.
        UpdateShopUI();
    }

    public void UpdateShopUI()
    {
        // Update currency balances.
        if (GameManager.Instance != null)
        {
            if (cashText != null) cashText.text = $"Cash: ${GameManager.Instance.Cash}";
            if (scrapText != null) scrapText.text = $"Scrap: {GameManager.Instance.Scrap}";
            if (rationsText != null) rationsText.text = $"Rations: {GameManager.Instance.Rations}";
        }

        Weapon selected = SelectedEquipment != null ? SelectedEquipment.CurrentWeapon : null;
        int price = selected != null ? selected.GetNextUpgradeCost() : 0;
        if (weaponDetails != null && selected != null)
            weaponDetails.text = $"DMG {selected.CurrentDamage}   MAG {selected.MaxAmmo}   RELOAD {selected.CurrentReloadTime:0.##}s";
        if (upgradeWeaponLabel != null && selected != null)
            upgradeWeaponLabel.text = price > 0 ? $"Upgrade {selected.Stats.weaponName}   /   ${price}" : $"{selected.Stats.weaponName}   /   Max level";
        if (upgradeWeaponButton != null) upgradeWeaponButton.interactable = price > 0 && GameManager.Instance.Cash >= price;
        UpdateArmory(selected);
        int slots = companionStations == null ? 0 : companionStations.Length;
        if (recruitButton != null) recruitButton.interactable = companionPrefab != null && activeCompanions.Count < slots && GameManager.Instance.Rations >= recruitRationCost;
        if (recruitLabel != null) recruitLabel.text = $"Recruit   /   {recruitRationCost} rations   ({activeCompanions.Count}/{slots})";
        // Update barricade health.
        Barricade wall = Barricade.Instance != null ? Barricade.Instance : FindAnyObjectByType<Barricade>();
        if (wall != null)
        {
            if (upgradeWallButton != null) upgradeWallButton.interactable = GameManager.Instance.Scrap >= upgradeWallScrapCost;
            if (repairButton != null) repairButton.interactable = wall.CurrentHealth < wall.MaxHealth && GameManager.Instance.Scrap >= repairScrapCost;
            float hpPercent = wall.MaxHealth > 0 ? (float)wall.CurrentHealth / wall.MaxHealth : 0f;
            int percentInt = Mathf.RoundToInt(hpPercent * 100f);

            // Display current and maximum HP.
            if (barricadeHpText != null)
            {
                barricadeHpText.text = $"Wall HP: {wall.CurrentHealth} / {wall.MaxHealth} ({percentInt}%)";

                // Use green for healthy, amber for damaged, and red for critical health.
                if (percentInt > 60)
                    barricadeHpText.color = new Color(0.2f, 1f, 0.2f); // Healthy.
                else if (percentInt > 30)
                    barricadeHpText.color = new Color(1f, 0.8f, 0.1f); // Damaged.
                else
                    barricadeHpText.color = new Color(1f, 0.2f, 0.2f); // Critical.
            }

            // Update the optional health bar.
            if (barricadeHpSlider != null)
            {
                barricadeHpSlider.value = hpPercent;
            }
        }
    }

    // Repair the barricade with scrap. This method is bound to the repair button.
    public void RepairBarricadeWithScrap()
    {
        Debug.Log("[ShopManager] Barricade repair requested.");

        Barricade wall = Barricade.Instance != null ? Barricade.Instance : FindAnyObjectByType<Barricade>();
        if (wall == null)
        {
            Debug.LogError("[ShopManager] Cannot repair: no barricade found in the scene.");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("[ShopManager] Cannot repair: no GameManager found in the scene.");
            return;
        }

        if (wall.CurrentHealth >= wall.MaxHealth)
        {
            Debug.LogWarning($"[ShopManager] Barricade already at full health ({wall.CurrentHealth} / {wall.MaxHealth}).");
            return;
        }

        if (GameManager.Instance.SpendCurrency(CurrencyType.Scrap, repairScrapCost))
        {
            wall.RepairBarricade(repairHealthAmount);
            UpdateShopUI(); // Refresh the shop after the repair.
            Debug.Log($"[ShopManager] Barricade repaired to {wall.CurrentHealth} / {wall.MaxHealth} HP for {repairScrapCost} scrap.");
        }
        else
        {
            Debug.LogWarning($"[ShopManager] Repair requires {repairScrapCost} scrap. Available: {GameManager.Instance.Scrap}.");
        }
    }

    // Spend scrap to increase maximum barricade health.
    public void UpgradeBarricadeMaxHPWithScrap()
    {
        Barricade wall = Barricade.Instance != null ? Barricade.Instance : FindAnyObjectByType<Barricade>();
        if (wall == null || GameManager.Instance == null) return;

        if (GameManager.Instance.SpendCurrency(CurrencyType.Scrap, upgradeWallScrapCost))
        {
            wall.UpgradeMaxHP(upgradeWallHealthAmount);
            UpdateShopUI();
            Debug.Log($"Barricade maximum HP increased by {upgradeWallHealthAmount} for {upgradeWallScrapCost} scrap.");
        }
        else
        {
            Debug.Log("Insufficient scrap to upgrade barricade health.");
        }
    }

    // Spend cash to upgrade damage, magazine capacity, and reload speed.
    public void UpgradeEquippedWeapon()
    {
        Equipment equipment = SelectedEquipment;
        if (equipment != null && equipment.CurrentWeapon != null)
        {
            UpgradeWeapon(equipment.CurrentWeapon);
        }
    }

    public void UpgradeWeapon(Weapon weapon)
    {
        if (weapon == null || GameManager.Instance == null) return;

        int cost = weapon.GetNextUpgradeCost();
        if (cost <= 0) return;

        if (GameManager.Instance.SpendCurrency(CurrencyType.Cash, cost))
        {
            weapon.Upgrade();
            UpdateShopUI();
            Debug.Log($"Upgraded {weapon.Stats?.weaponName ?? weapon.name} to level {weapon.currentLevel}.");
        }
        else
        {
            Debug.Log("Insufficient cash to upgrade the weapon.");
        }
    }

    // Spend rations to recruit a guard for an available post.
    public void RecruitCompanion()
    {
        if (GameManager.Instance == null || companionPrefab == null || playerEquipment == null) return;

        if (companionStations == null || activeCompanions.Count >= companionStations.Length)
        {
            Debug.Log("All guard posts are occupied.");
            return;
        }

        Transform station = companionStations[activeCompanions.Count];
        if (station == null || !companionPrefab.TryGetComponent<CompanionAI>(out _)) return;
        if (GameManager.Instance.SpendCurrency(CurrencyType.Rations, recruitRationCost))
        {
            GameObject companion = Instantiate(companionPrefab, station.position, Quaternion.identity);
            activeCompanions.Add(companion);
            Debug.Log($"Companion recruited for post {activeCompanions.Count}.");
            UpdateShopUI();
        }
        else
        {
            Debug.Log("Insufficient rations to recruit a companion.");
        }
    }
}
