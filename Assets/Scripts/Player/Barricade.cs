using UnityEngine;

public class Barricade : MonoBehaviour
{
    public static Barricade Instance { get; private set; }

    [Header("Barricade Stats")]
    public int maxHealth = 1000;
    private int currentHealth;

    [Header("Health Gradient (Destroyed to Full Health)")]
    [SerializeField] private Gradient healthGradient;
    private SpriteRenderer spriteRenderer;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel; // Assign the game-over panel.

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        currentHealth = maxHealth;
    }

    private void Start()
    {
        if (currentHealth <= 0) currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateWallColor();
    }

    public void RestoreHealth(int hp, int maximum)
    {
        maxHealth = maximum;
        currentHealth = hp;
        UpdateWallColor();
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;
        Debug.Log($"Barricade hit. HP: {currentHealth}/{maxHealth}.");

        UpdateWallColor();

        if (currentHealth <= 0)
        {
            TriggerGameOver();
        }
    }

    public void RepairBarricade(int amount)
    {
        if (currentHealth >= maxHealth) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"Barricade repaired. HP: {currentHealth}/{maxHealth}.");
        UpdateWallColor();
    }

    public void UpgradeMaxHP(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        Debug.Log($"Barricade upgraded. New maximum HP: {maxHealth}.");
        UpdateWallColor();
    }

    private void UpdateWallColor()
    {
        if (spriteRenderer == null) return;
        float healthPercent = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        spriteRenderer.color = healthGradient.Evaluate(healthPercent);
    }

    private void TriggerGameOver()
    {
        Debug.Log("Barricade destroyed. Game over.");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }

        Time.timeScale = 0f;
    }
}
