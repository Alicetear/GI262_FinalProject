using UnityEngine;

public class Barricade : MonoBehaviour
{
    [Header("ตั้งค่าเลือดกำแพง")]
    [SerializeField] private int maxBarricadeHealth = 100;
    private int currentHealth;

    [Header("สไปรต์กำแพงตามสภาพความเสียหาย")]
    [SerializeField] private SpriteRenderer wallSpriteRenderer;
    [SerializeField] private Sprite stateNormal;   // เลือด 70% ขึ้นไป
    [SerializeField] private Sprite stateDamaged; // เลือด 40% - 69%
    [SerializeField] private Sprite stateBroken;  // เลือดต่ำกว่า 40%

    private void Start()
    {
        currentHealth = maxBarricadeHealth;
        UpdateBarricadeVisual();
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxBarricadeHealth);

        Debug.Log($"กำแพงฐานโดนโจมตี! เลือดเหลือ: {currentHealth} / {maxBarricadeHealth}");
        UpdateBarricadeVisual();

        if (currentHealth <= 0)
        {
            BarricadeDestroyed();
        }
    }

    // ฟังก์ชันเช็กและสลับภาพ PNG ตามเลือดที่เหลือ
    private void UpdateBarricadeVisual()
    {
        if (wallSpriteRenderer == null) return;

        float healthPercentage = (float)currentHealth / maxBarricadeHealth;

        if (healthPercentage > 0.69f)
        {
            wallSpriteRenderer.sprite = stateNormal;
        }
        else if (healthPercentage > 0.39f)
        {
            wallSpriteRenderer.sprite = stateDamaged;
        }
        else
        {
            wallSpriteRenderer.sprite = stateBroken;
        }
    }

    private void BarricadeDestroyed()
    {
        Debug.Log("Game Over! กำแพงฐานพังทลายลงแล้ว!");
        // โค้ดเรียกหน้าจอ Game Over
    }
}