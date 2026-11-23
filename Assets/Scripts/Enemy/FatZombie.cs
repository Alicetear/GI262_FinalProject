using UnityEngine;

// BaseEnemy handles movement, incoming damage, and barricade attacks.
public class FatZombie : BaseEnemy
{
    [Header("Fat Zombie Stats")]
    [SerializeField] private float fatMoveSpeed = 1.0f;   // Slower than a standard zombie.
    [SerializeField] private int fatMaxHealth = 100;      // Higher health than a standard zombie.
    [SerializeField] private int fatAttackDamage = 25;    // Heavy hits deal more damage to the barricade.

    protected override void Start()
    {
        attackType = EnemyAttackType.Melee;
        moveSpeed = fatMoveSpeed;
        maxHealth = fatMaxHealth;
        attackDamage = fatAttackDamage;

        // Initialize health and the rigidbody using the overridden stats.
        base.Start();

        Debug.Log("Fat Zombie spawned with HP: " + currentHealth);
    }
}
