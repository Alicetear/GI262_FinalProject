using System.Collections;
using UnityEngine;

public class Barricade : MonoBehaviour
{
    public static Barricade Instance { get; private set; }

    [Header("สเตตัสกำแพง")]
    public int maxHealth = 1000;
    private int currentHealth;

    [Header("Visual Damage States")]
    [Tooltip("สภาพสมบูรณ์ (เลือด > 60%)")]
    [SerializeField] private Sprite spriteFullHealth;
    [Tooltip("สภาพเริ่มพัง (เลือด <= 60% และ > 30%)")]
    [SerializeField] private Sprite spriteMediumDamage;
    [Tooltip("สภาพใกล้พังวิกฤต (เลือด <= 30%)")]
    [SerializeField] private Sprite spriteHeavyDamage;

    [Header("Hit Impact Feedback")]
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;

    [Header("ระบบสีกำแพงสำรอง (กรณีไม่ได้ใส่ Sprite)")]
    [SerializeField] private Gradient healthGradient;

    private SpriteRenderer spriteRenderer;
    private Coroutine flashCoroutine;

    [Header("ระบบจบเกม")]
    [SerializeField] private GameObject gameOverPanel; // ลาก UI Game Over มาใส่

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (currentHealth <= 0) currentHealth = maxHealth;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateWallVisual();
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;
        Debug.Log($"กำแพงโดนตี! เลือดเหลือ: {currentHealth}/{maxHealth}");

        UpdateWallVisual();
        TriggerHitFlash();

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
        UpdateWallVisual();
    }

    public void Heal(int amount) => RepairBarricade(amount);

    public void UpgradeMaxHP(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        Debug.Log($"อัปเกรดความจุเลือดกำแพง! เลือดสูงสุดใหม่: {maxHealth}");
        UpdateWallVisual();
    }

    private void UpdateWallVisual()
    {
        if (spriteRenderer == null) return;

        float hpPercent = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;

        if (hpPercent > 0.6f)
        {
            if (spriteFullHealth != null) spriteRenderer.sprite = spriteFullHealth;
        }
        else if (hpPercent > 0.3f && hpPercent <= 0.6f)
        {
            if (spriteMediumDamage != null) spriteRenderer.sprite = spriteMediumDamage;
        }
        else
        {
            if (spriteHeavyDamage != null) spriteRenderer.sprite = spriteHeavyDamage;
        }

        // หากไม่ได้ใส่ Sprite 3 ระดับไว้ ให้ใช้ Gradient เดิมเป็น Fallback
        bool hasAnyDamageSprite = spriteFullHealth != null || spriteMediumDamage != null || spriteHeavyDamage != null;
        if (!hasAnyDamageSprite && healthGradient != null)
        {
            spriteRenderer.color = healthGradient.Evaluate(hpPercent);
        }
        else if (flashCoroutine == null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    private void TriggerHitFlash()
    {
        if (spriteRenderer == null || !gameObject.activeInHierarchy) return;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);

        bool hasAnyDamageSprite = spriteFullHealth != null || spriteMediumDamage != null || spriteHeavyDamage != null;
        if (!hasAnyDamageSprite && healthGradient != null)
        {
            float hpPercent = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
            spriteRenderer.color = healthGradient.Evaluate(hpPercent);
        }
        else
        {
            spriteRenderer.color = Color.white;
        }

        flashCoroutine = null;
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
