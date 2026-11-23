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
<<<<<<< HEAD
        attackType = EnemyAttackType.Melee;
        // Apply this enemy's stats before initialization and override BaseEnemy defaults.
=======
        // Apply this enemy's stats before initialization.
>>>>>>> d8ee8a61bf1f885c5b6c1c0a93acacfb51fa0830
        moveSpeed = fatMoveSpeed;
        maxHealth = fatMaxHealth;
        attackDamage = fatAttackDamage;

        // Initialize health and the rigidbody using the overridden stats.
        base.Start();

        Debug.Log("Fat Zombie spawned with HP: " + currentHealth);
    }

    // Override Die to add a death effect such as an explosion.
    /*
    protected override void Die()
    {
        Debug.Log("Fat Zombie exploded on death.");
        // Play the explosion effect here.
        
        base.Die(); // Let the base class report the kill to the wave manager.
    }
    */
}
