using UnityEngine;

public class RunnerEnemy : BaseEnemy
{
    [Header("ตั้งค่าสไปรต์เฉพาะตัวของ Runner")]
    [SerializeField] private Sprite runnerSprite; // ช่องสำหรับลากรูปสไปรต์ซอมบี้วิ่งมาใส่
    private SpriteRenderer spriteRenderer;

    protected override void Start()
    {
        base.Start();

        // เซ็ตสเตตัสความเร็วและเลือดของ Runner
        moveSpeed = 6.0f;
        maxHealth = 15;
        currentHealth = maxHealth;

        // ดึงคอมโพเนนต์ SpriteRenderer ออกมาควบคุม
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ถ้ามีการใส่สไปรต์รูปวิ่งไว้ ให้ทำการเปลี่ยนรูปทันทีตอนเกิด
        if (runnerSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = runnerSprite;
        }
    }
}