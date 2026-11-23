using UnityEngine;

public class RunnerEnemy : BaseEnemy
{
    [Header("Runner Sprite")]
    [SerializeField] private Sprite runnerSprite; // Assign the runner sprite here.
    private SpriteRenderer spriteRenderer;

    protected override void Start()
    {
        attackType = EnemyAttackType.Melee;
        base.Start();

        // Set the runner's speed and health.
        moveSpeed = 6.0f;
        maxHealth = 15;
        currentHealth = maxHealth;

        // Get the renderer used by this enemy.
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Apply the runner sprite on spawn if one is assigned.
        if (runnerSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = runnerSprite;
        }
    }
}