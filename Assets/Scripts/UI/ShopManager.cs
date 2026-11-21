using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("การตั้งค่าซ่อมและอัปเกรดกำแพง (ใช้ Scrap)")]
    [SerializeField] private int repairScrapCost = 10;
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

    [Header("UI แสดงเลือดกำแพง (Wall Health)")]
    [Tooltip("ข้อความแสดงเลือดกำแพง เช่น Wall HP: 750 / 1000 (75%)")]
    [SerializeField] private TextMeshProUGUI barricadeHpText;
    [Tooltip("แถบ Slider แสดงเลือดกำแพง (ใส่หรือไม่ใส่ก็ได้)")]
    [SerializeField] private Slider barricadeHpSlider;

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
        // อัปเดตข้อมูลเลือดกำแพงและเงินทันทีที่หน้าต่างร้านค้าเปิดขึ้นมาตอนเช้า
        UpdateShopUI();
    }

    public void UpdateShopUI()
    {
        // 1. อัปเดตตัวเลขเงิน
        if (GameManager.Instance != null)
        {
            if (cashText != null) cashText.text = $"Cash: ${GameManager.Instance.Cash}";
            if (scrapText != null) scrapText.text = $"Scrap: {GameManager.Instance.Scrap}";
            if (rationsText != null) rationsText.text = $"Rations: {GameManager.Instance.Rations}";
        }

        // 2. อัปเดตเลือดกำแพง
        Barricade wall = Barricade.Instance != null ? Barricade.Instance : FindAnyObjectByType<Barricade>();
        if (wall != null)
        {
            float hpPercent = wall.MaxHealth > 0 ? (float)wall.CurrentHealth / wall.MaxHealth : 0f;
            int percentInt = Mathf.RoundToInt(hpPercent * 100f);

            // อัปเดตข้อความตัวเลข
            if (barricadeHpText != null)
            {
                barricadeHpText.text = $"Wall HP: {wall.CurrentHealth} / {wall.MaxHealth} ({percentInt}%)";

                // เปลี่ยนสีข้อความตามสถานะเลือด (เขียว = ปลอดภัย, ส้ม = ปานกลาง, แดง = วิกฤต)
                if (percentInt > 60)
                    barricadeHpText.color = new Color(0.2f, 1f, 0.2f); // เขียว
                else if (percentInt > 30)
                    barricadeHpText.color = new Color(1f, 0.8f, 0.1f); // ส้มเหลือง
                else
                    barricadeHpText.color = new Color(1f, 0.2f, 0.2f); // แดง
            }

            // อัปเดตแถบ Slider (ถ้ามี)
            if (barricadeHpSlider != null)
            {
                barricadeHpSlider.value = hpPercent;
            }
        }
    }

    // 1. ซ่อมแซมกำแพงด้วย Scrap (void เพื่อให้ปุ่ม UI OnClick มองเห็นได้)
    public void RepairBarricadeWithScrap()
    {
        Debug.Log("[ShopManager] >>> ผู้เล่นกดปุ่มซ่อมกำแพง <<<");

        Barricade wall = Barricade.Instance != null ? Barricade.Instance : FindAnyObjectByType<Barricade>();
        if (wall == null)
        {
            Debug.LogError("[ShopManager] ผิดพลาด: ไม่พบ Barricade (กำแพง) ในฉาก!");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("[ShopManager] ผิดพลาด: ไม่พบ GameManager ในฉาก!");
            return;
        }

        if (wall.CurrentHealth >= wall.MaxHealth)
        {
            Debug.LogWarning($"[ShopManager] ซ่อมไม่ได้: เลือดกำแพงเต็มอยู่แล้ว! ({wall.CurrentHealth} / {wall.MaxHealth})");
            return;
        }

        if (GameManager.Instance.SpendCurrency(CurrencyType.Scrap, repairScrapCost))
        {
            wall.RepairBarricade(repairHealthAmount);
            UpdateShopUI(); // อัปเดตหน้าจอทันทีหลังจากซ่อม
            Debug.Log($"[ShopManager] ซ่อมกำแพงสำเร็จ! เลือดเพิ่มเป็น: {wall.CurrentHealth} / {wall.MaxHealth} (ใช้ Scrap: {repairScrapCost})");
        }
        else
        {
            Debug.LogWarning($"[ShopManager] ซ่อมไม่ได้: Scrap ไม่พอ! (คุณมี {GameManager.Instance.Scrap} Scrap แต่ต้องใช้ {repairScrapCost} Scrap)");
        }
    }

    // 2. อัปเกรด Max HP กำแพงด้วย Scrap
    public void UpgradeBarricadeMaxHPWithScrap()
    {
        Barricade wall = Barricade.Instance != null ? Barricade.Instance : FindAnyObjectByType<Barricade>();
        if (wall == null || GameManager.Instance == null) return;

        if (GameManager.Instance.SpendCurrency(CurrencyType.Scrap, upgradeWallScrapCost))
        {
            wall.UpgradeMaxHP(upgradeWallHealthAmount);
            UpdateShopUI();
            Debug.Log($"อัปเกรด Max HP สำเร็จ! +{upgradeWallHealthAmount} Max HP (ใช้ {upgradeWallScrapCost} Scrap)");
        }
        else
        {
            Debug.Log("Scrap ไม่พอสำหรับอัปเกรด Max HP!");
        }
    }

    // 3. อัปเกรดสถานะปืน (Damage/Mag/Reload) ด้วย Cash
    public void UpgradeEquippedWeapon()
    {
        Equipment equipment = FindAnyObjectByType<Equipment>();
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
            Debug.Log($"อัปเกรดปืน {weapon.Stats?.weaponName ?? weapon.name} สำเร็จ! Level: {weapon.currentLevel}");
        }
        else
        {
            Debug.Log("Cash ไม่พอสำหรับอัปเกรดปืน!");
        }
    }

    // 4. จ้างเพื่อน AI มาประจำการ (สูงสุด 3 คน) ด้วย Rations
    public void RecruitCompanion()
    {
        if (GameManager.Instance == null || companionPrefab == null) return;

        if (companionStations == null || activeCompanions.Count >= companionStations.Length)
        {
            Debug.Log("เพื่อนร่วมทีมเต็มแล้ว! (สูงสุด 3 คน)");
            return;
        }

        if (GameManager.Instance.SpendCurrency(CurrencyType.Rations, recruitRationCost))
        {
            Transform spawnPoint = companionStations[activeCompanions.Count];
            GameObject companion = Instantiate(companionPrefab, spawnPoint.position, spawnPoint.rotation);
            activeCompanions.Add(companion);
            Debug.Log($"จ้างเพื่อน AI สำเร็จ! ประจำการตำแหน่งที่ {activeCompanions.Count}");
            UpdateShopUI();
        }
        else
        {
            Debug.Log("Rations ไม่พอสำหรับจ้างเพื่อน AI!");
        }
    }
}
