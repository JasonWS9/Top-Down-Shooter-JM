using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Assigned Data")]
    public EnemyData myData;

    [Header("Live Stats (Calculated)")]
    public float currentHealth;
    public float currentSpeed;
    public float currentDamage;

    private SpriteRenderer spriteRenderer;
    private Transform playerTarget;

    // Call this right after instantiating to set up the enemy
    public void Initialize(EnemyData data)
    {
        myData = data;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (PlayerManager.Instance != null)
        {
            playerTarget = PlayerManager.Instance.transform;
        }

        ApplyDataAndFactionModifiers();
    }

    void ApplyDataAndFactionModifiers()
    {
        // 1. Load Base Stats from Scriptable Object
        currentHealth = myData.baseHealth;
        currentSpeed = myData.baseSpeed;
        currentDamage = myData.attackDamage;

        // 2. Apply Faction Modifiers and Colors
        switch (myData.faction)
        {
            case EnemyFaction.Purple:
                currentSpeed *= 0.8f;      // Slower
                currentHealth *= 1.5f;     // Stronger defense
                if (spriteRenderer != null) spriteRenderer.color = new Color(0.5f, 0f, 0.5f); // Purple
                break;

            case EnemyFaction.Orange:
                currentSpeed *= 1.2f;      // Quicker
                currentDamage *= 1.2f;     // Slightly stronger attack
                if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.5f, 0f); // Orange
                break;
        }
    }

    void Update()
    {
        if (playerTarget == null || myData == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer > myData.attackRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            AttackBehavior();
        }
    }

    void MoveTowardsPlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, playerTarget.position, currentSpeed * Time.deltaTime);
        
        // Aim at player
        Vector2 direction = (playerTarget.position - transform.position).normalized;
        transform.up = direction;
    }

    void AttackBehavior()
    {
        // Behavior branches based on the ScriptableObject's defined type
        switch (myData.enemyType)
        {
            case EnemyType.SingleShot:
                // TODO: Fire myData.projectilePrefab
                break;
            case EnemyType.BurstShot:
                // TODO: Fire three of myData.projectilePrefab in a spread
                break;
            case EnemyType.Rammer:
                // TODO: Trigger ram/melee damage logic
                break;
        }
    }

    public void TakeDamage(int damage, bool fromPlayer = false)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}