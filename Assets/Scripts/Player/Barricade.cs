using UnityEngine;

public class Barricade : MonoBehaviour
{
    public static Barricade Instance { get; private set; }

    [Header("สเตตัสกำแพง")]
    public int maxHealth = 1000;
    private int currentHealth;

    [Header("ระบบสีกำแพง (ซ้าย=พัง/สีแดง, ขวา=เต็ม/สีเขียว)")]
    [SerializeField] private Gradient healthGradient;
    private SpriteRenderer spriteRenderer;

    [Header("ระบบจบเกม")]
    [SerializeField] private GameObject gameOverPanel; // ลาก UI Game Over มาใส่

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateWallColor();
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;
        Debug.Log($"กำแพงโดนตี! เลือดเหลือ: {currentHealth}/{maxHealth}");

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
        Debug.Log($"ซ่อมแซมกำแพงแล้ว! เลือดปัจจุบัน: {currentHealth}/{maxHealth}");
        UpdateWallColor();
    }

    public void UpgradeMaxHP(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        Debug.Log($"อัปเกรดความจุเลือดกำแพง! เลือดสูงสุดใหม่: {maxHealth}");
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
        Debug.Log("แนวป้องกันแตก! GAME OVER!");

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
