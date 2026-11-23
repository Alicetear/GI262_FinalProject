using UnityEngine;
using TMPro;

public class InGameHUD : MonoBehaviour
{
    [Header("Currency Display")]
    [SerializeField] private TextMeshProUGUI cashText;
    [SerializeField] private TextMeshProUGUI scrapText;
    [SerializeField] private TextMeshProUGUI rationsText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Wave and Night Display")]
    [SerializeField] private TextMeshProUGUI nightText;
    [SerializeField] private TextMeshProUGUI enemiesRemainingText;

    [Header("Barricade Display")]
    [SerializeField] private TextMeshProUGUI wallHealthText;

    [Header("Weapon and Ammo Display")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private GameObject reloadIndicator;

    [Header("Player References")]
    [SerializeField] private PlayerShooting playerShooting;
    [SerializeField] private Equipment playerEquipment;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrencyChanged += UpdateCurrencyUI;
            GameManager.Instance.OnScoreChanged += UpdateScoreUI;
        }

        UpdateCurrencyUI();
        UpdateScoreUI(GameManager.Instance != null ? GameManager.Instance.Score : 0);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrencyChanged -= UpdateCurrencyUI;
            GameManager.Instance.OnScoreChanged -= UpdateScoreUI;
        }
    }

    private void Update()
    {
        UpdateWeaponAndAmmoUI();
        UpdateWaveAndWallUI();
    }

    private void UpdateCurrencyUI()
    {
        if (GameManager.Instance == null) return;

        if (cashText != null) cashText.text = $"Cash: ${GameManager.Instance.Cash}";
        if (scrapText != null) scrapText.text = $"Scrap: {GameManager.Instance.Scrap}";
        if (rationsText != null) rationsText.text = $"Rations: {GameManager.Instance.Rations}";
    }

    private void UpdateScoreUI(int score)
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }

    private void UpdateWaveAndWallUI()
    {
        if (WaveManager.Instance != null)
        {
            if (nightText != null)
            {
                var waves = WaveManager.Instance;
                bool defending = waves.RunStarted && GameManager.Instance.currentState == GameState.NightDefense;
                nightText.enabled = defending;
                if (defending) nightText.SetText("NIGHT {0}/{1}", waves.currentWaveIndex + 1, RunCheckpoint.CampaignNights);
            }

            if (enemiesRemainingText != null)
            {
                int remaining = WaveManager.Instance.ActiveEnemyCount + WaveManager.Instance.SpawnQueueCount;
                enemiesRemainingText.text = $"Zombies: {remaining}";
            }
        }

        Barricade wall = Barricade.Instance != null ? Barricade.Instance : FindAnyObjectByType<Barricade>();
        if (wall != null && wallHealthText != null)
        {
            wallHealthText.text = $"Wall: {wall.CurrentHealth} / {wall.MaxHealth}";
        }
    }

    private void UpdateWeaponAndAmmoUI()
    {
        Weapon activeWeapon = null;
        if (playerEquipment != null)
        {
            activeWeapon = playerEquipment.CurrentWeapon;
        }

        if (activeWeapon != null)
        {
            if (weaponNameText != null)
            {
                string wName = activeWeapon.Stats != null ? activeWeapon.Stats.weaponName : "Gun";
                weaponNameText.text = $"{wName} (Lv.{activeWeapon.currentLevel})";
            }

            if (ammoText != null)
            {
                ammoText.text = $"{activeWeapon.CurrentAmmo} / {activeWeapon.MaxAmmo}";
            }

            if (reloadIndicator != null)
            {
                reloadIndicator.SetActive(activeWeapon.IsReloading);
            }
        }
    }
}

