using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MNEnemyController : MonoBehaviour, IDamageable
{
    public MNEnemyData enemyData;
    
    private int currentHealth;
    private Rigidbody2D rb;
    private Transform player;

    // State Timers
    private float actionTimer;
    private float shootTimer;
    private Vector2 wanderTarget;

    // Momentum specific
    private float currentMomentumSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = enemyData.maxHealth;
        
        // Find player (assuming your player has the MNPlayerManager tag or instance)
        if (MNPlayerManager.Instance != null)
        {
            player = MNPlayerManager.Instance.transform;
        }

        InitializeBehavior();
    }

    void InitializeBehavior()
    {
        if (enemyData.behaviorType == EnemyBehaviorType.BurstChaser)
        {
            rb.linearDamping = enemyData.burstDrag; // Use linearDamping instead of drag in newer Unity versions
            actionTimer = Random.Range(enemyData.minBurstInterval, enemyData.maxBurstInterval);
        }
        else if (enemyData.behaviorType == EnemyBehaviorType.SlowShooter || enemyData.behaviorType == EnemyBehaviorType.StaticTurret)
        {
            // Both mobile shooters and static turrets use the shoot timer
            shootTimer = Random.Range(enemyData.minShootInterval, enemyData.maxShootInterval);
        }
        else if (enemyData.behaviorType == EnemyBehaviorType.WandererErratic || enemyData.behaviorType == EnemyBehaviorType.WandererSmooth)
        {
            actionTimer = enemyData.changeDirectionTime;
            PickNewWanderTarget();
        }
    }

    void Update()
    {
        if (player == null) return;

        actionTimer -= Time.deltaTime;
        shootTimer -= Time.deltaTime;

        if ((enemyData.behaviorType == EnemyBehaviorType.SlowShooter || enemyData.behaviorType == EnemyBehaviorType.StaticTurret) && shootTimer <= 0)
        {
            Shoot();
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        switch (enemyData.behaviorType)
        {
            case EnemyBehaviorType.StaticTurret:
                // Ensure they absolutely do not move or get pushed by physics
                rb.linearVelocity = Vector2.zero; 
                rb.angularVelocity = 0f;
                break;
            case EnemyBehaviorType.BurstChaser:
                HandleBurstChaser();
                break;
            case EnemyBehaviorType.MomentumChaser:
                HandleMomentumChaser();
                break;
            case EnemyBehaviorType.SlowShooter:
                HandleSlowShooter();
                break;
            case EnemyBehaviorType.WandererErratic:
                HandleWanderer(true);
                break;
            case EnemyBehaviorType.WandererSmooth:
                HandleWanderer(false);
                break;
        }
    }

    // --- BEHAVIOR LOGIC ---

    void HandleBurstChaser()
    {
        // Always face the player
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        transform.up = directionToPlayer;

        if (actionTimer <= 0)
        {
            // BURST! Add instant force towards the player
            rb.AddForce(directionToPlayer * enemyData.burstForce, ForceMode2D.Impulse);
            
            // Reset timer
            actionTimer = Random.Range(enemyData.minBurstInterval, enemyData.maxBurstInterval);
        }
    }

    void HandleMomentumChaser()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        
        // Calculate how much we need to turn
        float angleToPlayer = Vector2.SignedAngle(transform.up, directionToPlayer);
        
        // If we are turning sharply, lose momentum
        if (Mathf.Abs(angleToPlayer) > 10f) 
        {
            currentMomentumSpeed -= enemyData.momentumLossMultiplier * Time.fixedDeltaTime;
            currentMomentumSpeed = Mathf.Max(currentMomentumSpeed, 0.5f); // Don't stop completely
        }
        else
        {
            // Speed up if going straight
            currentMomentumSpeed += enemyData.acceleration * Time.fixedDeltaTime;
            currentMomentumSpeed = Mathf.Min(currentMomentumSpeed, enemyData.baseSpeed);
        }

        // Rotate smoothly
        float step = enemyData.turnSpeed * Time.fixedDeltaTime;
        transform.up = Vector3.RotateTowards(transform.up, directionToPlayer, step, 0.0f);

        // Move forward
        rb.linearVelocity = transform.up * currentMomentumSpeed; // Use linearVelocity in newer Unity
    }

    void HandleSlowShooter()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        transform.up = directionToPlayer;
        
        // Move constantly but slowly
        rb.linearVelocity = directionToPlayer * enemyData.baseSpeed;
    }

    void Shoot()
    {
        if (enemyData.projectilePrefab != null)
        {
            GameObject bullet;

            if (enemyData.behaviorType == EnemyBehaviorType.StaticTurret)
            {
                // Turrets fire straight up
                bullet = Instantiate(enemyData.projectilePrefab, transform.position, Quaternion.identity);
                bullet.transform.up = Vector2.up; 
            }
            else
            {
                // Slow shooters fire at the player (they are already rotated to face the player in Update)
                bullet = Instantiate(enemyData.projectilePrefab, transform.position, transform.rotation);
            }

            // --- THE FIX: MAKE IT MOVE ---
            Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();
            if (bulletRB != null)
            {
                // We push it in its local "up" direction, multiplied by the speed from the ScriptableObject
                bulletRB.AddForce(bullet.transform.up * enemyData.projectileSpeed, ForceMode2D.Impulse);
            }
            else
            {
                Debug.LogWarning("Enemy projectile prefab is missing a Rigidbody2D component!");
            }
        }
        
        shootTimer = Random.Range(enemyData.minShootInterval, enemyData.maxShootInterval);
    }

    void HandleWanderer(bool erratic)
    {
        if (actionTimer <= 0)
        {
            PickNewWanderTarget();
            actionTimer = enemyData.changeDirectionTime;
        }

        Vector2 directionToTarget = (wanderTarget - (Vector2)transform.position).normalized;
        transform.up = directionToTarget;

        if (erratic)
        {
            // Erratic changes speed randomly
            rb.linearVelocity = directionToTarget * (enemyData.baseSpeed * Random.Range(0.5f, 1.5f));
        }
        else
        {
            // Smooth is a constant drift
            rb.linearVelocity = directionToTarget * enemyData.baseSpeed;
        }
    }

    void PickNewWanderTarget()
    {
        // Pick a random point within a radius
        wanderTarget = (Vector2)transform.position + Random.insideUnitCircle * 5f;
    }

    // --- IDAMAGEABLE IMPLEMENTATION ---
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Play explosion, add score, trigger AI events, etc.
        Destroy(gameObject);
    }
}