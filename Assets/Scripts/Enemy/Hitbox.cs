using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("Hitbox Settings")]
    [SerializeField] private BaseEnemy mainEnemy; // Assign the enemy that owns this hitbox.
    [SerializeField] private float damageMultiplier = 1.0f; // Damage multiplier (head: 2, body: 1).

    public BaseEnemy MainEnemy
    {
        get
        {
            if (mainEnemy == null) mainEnemy = GetComponentInParent<BaseEnemy>();
            return mainEnemy;
        }
    }

    private void Awake()
    {
        if (mainEnemy == null)
        {
            mainEnemy = GetComponentInParent<BaseEnemy>();
        }
    }

    // Called when a bullet hits this collider.
    public void TakeHit(int baseDamage)
    {
        TakeHit(baseDamage, transform.position, Vector2.left);
    }

    public void TakeHit(int baseDamage, Vector2 hitPoint, Vector2 hitDirection)
    {
        if (mainEnemy == null)
        {
            mainEnemy = GetComponentInParent<BaseEnemy>();
        }

        // Scale the base damage for the body part that was hit.
        int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);

        if (mainEnemy != null)
        {
            Debug.Log($"Hit {gameObject.name} for {finalDamage} damage.");
            mainEnemy.TakeDamage(finalDamage, hitPoint, hitDirection);
        }
        else
        {
            Debug.LogWarning($"[Hitbox] ไม่พบ BaseEnemy บน {gameObject.name} หรือ Parent!");
        }
    }
}