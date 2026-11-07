using UnityEngine;

public class Barricade : MonoBehaviour
{
    [Header("สเตตัสกำแพง")]
    public int maxHealth = 1000;
    private int currentHealth;

    [Header("ระบบสีกำแพง (ซ้าย=พัง/สีแดง, ขวา=เต็ม/สีเขียว)")]
    // ตัวแปรนี้จะสร้างแถบสีใน Inspector ให้เราปรับแต่งได้ง่ายๆ
    [SerializeField] private Gradient healthGradient;
    private SpriteRenderer spriteRenderer;

    [Header("ระบบจบเกม")]
    [SerializeField] private GameObject gameOverPanel; // ลาก UI Game Over มาใส่

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateWallColor(); // เซ็ตสีเริ่มต้นตอนเริ่มเกม
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return; // ถ้าพังแล้วไม่ต้องรับดาเมจเพิ่ม

        currentHealth -= damageAmount;
        Debug.Log($"กำแพงโดนตี! เลือดเหลือ: {currentHealth}/{maxHealth}");

        UpdateWallColor(); // อัปเดตสีทุกครั้งที่โดนตี

        if (currentHealth <= 0)
        {
            TriggerGameOver();
        }
    }

    // ฟังก์ชันคำนวณและเปลี่ยนสีกำแพงตามเปอร์เซ็นต์เลือด
    private void UpdateWallColor()
    {
        if (spriteRenderer == null) return;

        // 1. หาเปอร์เซ็นต์เลือด (0.0 ถึง 1.0)
        float healthPercent = (float)currentHealth / maxHealth;

        // 2. ใช้คำสั่ง Evaluate ดูดสีจาก Gradient ตามเปอร์เซ็นต์
        // (ถ้า healthPercent = 1 มันจะดึงสีขวาสุด, ถ้า = 0.5 ดึงสีตรงกลาง)
        spriteRenderer.color = healthGradient.Evaluate(healthPercent);
    }

    private void TriggerGameOver()
    {
        Debug.Log("แนวป้องกันแตก! GAME OVER!");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // เปิดหน้าจอพ่ายแพ้
        }

        Time.timeScale = 0f; // หยุดเวลาในเกมทั้งหมด
    }
}