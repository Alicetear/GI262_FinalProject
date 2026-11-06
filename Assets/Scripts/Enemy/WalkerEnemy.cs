using UnityEngine;

public class WalkerEnemy : BaseEnemy
{
    [Header("ตั้งค่าสไปรต์เฉพาะตัวของ Walker")]
    [SerializeField] private Sprite walkerSprite; // ช่องลากรูปสไปรต์ Walker มาใส่
    private SpriteRenderer spriteRenderer;

    protected override void Start()
    {
        base.Start();

        // เซ็ตสเตตัสเฉพาะตัวของ Walker (เดินช้า เลือดปานกลาง)
        moveSpeed = 2.5f;
        maxHealth = 30;
        currentHealth = maxHealth;

        // ดึงคอมโพเนนต์ SpriteRenderer ออกมาสลับรูปภาพ
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (walkerSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = walkerSprite;
        }
    }
}