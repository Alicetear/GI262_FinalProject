using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("การตั้งค่าซ่อมและอัปเกรดกำแพง (ใช้ Scrap)")]
    [SerializeField] private int repairScrapCost = 20;
    [SerializeField] private int repairHealthAmount = 250;
    [SerializeField] private int upgradeWallScrapCost = 50;
    [SerializeField] private int upgradeWallHealthAmount = 300;

    [Header("การตั้งค่าจ้างเพื่อนร่วมทีม AI (ใช้ Rations)")]
    [SerializeField] private int recruitRationCost = 2;
    [SerializeField] private GameObject companionPrefab;
    [SerializeField] private Transform[] companionStations; // จุดประจำตำแหน่งสูงสุด 3 จุด
    private List<GameObject> activeCompanions = new List<GameObject>();

    [Header("UI แสดงผลทรัพยากรในร้านค้า")]
    [SerializeField] private TextMeshProUGUI cashText;
    [SerializeField] private TextMeshProUGUI scrapText;
    [SerializeField] private TextMeshProUGUI rationsText;
    [SerializeField] private TextMeshProUGUI barricadeHpText;

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

        UpdateShopUI();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrencyChanged -= UpdateShopUI;
        }
    }

    private void OnEnable()
    {
        UpdateShopUI();
    }

    public void UpdateShopUI()
    {
        if (GameManager.Instance != null)
        {
            if (cashText != null) cashText.text = $"Cash: ${GameManager.Instance.Cash}";
            if (scrapText != null) scrapText.text = $"Scrap: {GameManager.Instance.Scrap}";
            if (rationsText != null) rationsText.text = $"Rations: {GameManager.Instance.Rations}";
        }

        Barricade wall = Barricade.Instance != null ? Barricade.Instance : FindAnyObjectByType<Barricade>();
        if (wall != null && barricadeHpText != null)
        {
            barricadeHpText.text = $"Wall HP: {wall.CurrentHealth}/{wall.MaxHealth}";
        }
    }

    public bool RepairBarricadeWithScrap()
    {
        Barricade wall = Barricade.Instance != null ? Barricade.Instance : FindAnyObjectByType<Barricade>();
        if (wall == null || GameManager.Instance == null) return false;

        if (wall.CurrentHealth >= wall.MaxHealth)
        {
            Debug.Log("กำแพงเลือดเต็มอยู่แล้ว!");
            return false;
        }

        if (GameManager.Instance.SpendCurrency(CurrencyType.Scrap, repairScrapCost))
        {
            wall.RepairBarricade(repairHealthAmount);
            UpdateShopUI();
            return true;
        }

        return false;
    }

    public bool UpgradeBarricadeMaxHPWithScrap()
    {
        Barricade wall = Barricade.Instance != null ? Barricade.Instance : FindAnyObjectByType<Barricade>();
        if (wall == null || GameManager.Instance == null) return false;

        if (GameManager.Instance.SpendCurrency(CurrencyType.Scrap, upgradeWallScrapCost))
        {
            wall.UpgradeMaxHP(upgradeWallHealthAmount);
            UpdateShopUI();
            return true;
        }

        return false;
    }

    public bool UpgradeWeapon(Weapon weapon)
    {
        if (weapon == null || GameManager.Instance == null) return false;

        int cost = weapon.GetNextUpgradeCost();
        if (cost <= 0) return false;

        if (GameManager.Instance.SpendCurrency(CurrencyType.Cash, cost))
        {
            bool success = weapon.Upgrade();
            UpdateShopUI();
            return success;
        }

        return false;
    }

    public bool RecruitCompanion()
    {
        if (GameManager.Instance == null || companionPrefab == null) return false;

        if (companionStations == null || activeCompanions.Count >= companionStations.Length)
        {
            Debug.Log("เพื่อนร่วมทีมเต็มแล้ว! (สูงสุด 3 คน)");
            return false;
        }

        if (GameManager.Instance.SpendCurrency(CurrencyType.Rations, recruitRationCost))
        {
            Transform spawnPoint = companionStations[activeCompanions.Count];
            GameObject companion = Instantiate(companionPrefab, spawnPoint.position, spawnPoint.rotation);
            activeCompanions.Add(companion);
            Debug.Log($"จ้างเพื่อน AI สำเร็จ! ประจำการตำแหน่งที่ {activeCompanions.Count}");
            UpdateShopUI();
            return true;
        }

        return false;
    }
}

