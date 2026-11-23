using UnityEngine;

public class WalkerEnemy : BaseEnemy
{
    [Header("Walker Sprite")]
    [SerializeField] private Sprite walkerSprite; // Assign the walker sprite here.
    private SpriteRenderer spriteRenderer;

    protected override void Start()
    {
        attackType = EnemyAttackType.Melee;
        base.Start();

        // Walkers move slowly and have moderate health.
        moveSpeed = 2.5f;
        maxHealth = 30;
        currentHealth = maxHealth;

        // Get the renderer used to apply the walker sprite.
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (walkerSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = walkerSprite;
        }
    }
}