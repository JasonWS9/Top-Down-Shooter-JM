using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MNEnemyController : MonoBehaviour, IDamageable
{
    public MNEnemyData enemyData;
    
    private int currentHealth;
    private Rigidbody2D rb;

    // Targeting
    private Transform currentTarget;
    private float radarTimer = 0f;

    // State Timers
    private float actionTimer;
    private float shootTimer;
    private Vector2 wanderTarget;

    // Momentum specific
    private float currentMomentumSpeed;
    
    // Tower specific
    private bool isBoltedToGround = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = enemyData.maxHealth;

        InitializeBehavior();
        
        // Lock planetary defender towers to the surface
        if (enemyData.faction == FactionType.Defender && enemyData.behaviorType == EnemyBehaviorType.StaticTurret)
        {
            isBoltedToGround = true;
        }
    }

    void InitializeBehavior()
    {
        if (enemyData.behaviorType == EnemyBehaviorType.BurstChaser)
        {
            rb.linearDamping = enemyData.burstDrag; 
            actionTimer = Random.Range(enemyData.minBurstInterval, enemyData.maxBurstInterval);
        }
        else if (enemyData.behaviorType == EnemyBehaviorType.SlowShooter || enemyData.behaviorType == EnemyBehaviorType.StaticTurret)
        {
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
        // Radar pulse every 0.5 seconds
        radarTimer -= Time.deltaTime;
        if (radarTimer <= 0)
        {
            FindTarget();
            radarTimer = 0.5f;
        }

        actionTimer -= Time.deltaTime;
        shootTimer -= Time.deltaTime;

        if ((enemyData.behaviorType == EnemyBehaviorType.SlowShooter || enemyData.behaviorType == EnemyBehaviorType.StaticTurret) && shootTimer <= 0)
        {
            Shoot();
        }

        // =======================================
        // THE BOLT LOGIC
        // =======================================
        if (isBoltedToGround && MNPlayerMovement.Instance != null)
        {
            float trueSurfaceY = MNPlayerMovement.Instance.minWorldY - MNPlayerMovement.Instance.currentTreadmillY;
            
            if (rb != null)
            {
                rb.position = new Vector2(rb.position.x, trueSurfaceY);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, trueSurfaceY, transform.position.z);
            }
        }
    }

    void FixedUpdate()
    {
        switch (enemyData.behaviorType)
        {
            case EnemyBehaviorType.StaticTurret:
                if (!isBoltedToGround) rb.linearVelocity = Vector2.zero; 
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

    // --- TARGETING LOGIC ---

    void FindTarget()
    {
        if (enemyData.faction == FactionType.Rival)
        {
            if (MNPlayerMovement.Instance != null)
                currentTarget = MNPlayerMovement.Instance.transform;
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, enemyData.radarRange);
        Transform bestTarget = null;
        float closestDist = Mathf.Infinity;
        bool foundPriorityTarget = false; 

        foreach (var hit in hits)
        {
            float dist = Vector2.Distance(transform.position, hit.transform.position);

            if (enemyData.faction == FactionType.Alien)
            {
                if (hit.CompareTag("Defender"))
                {
                    if (dist < closestDist || !foundPriorityTarget) 
                    {
                        bestTarget = hit.transform;
                        closestDist = dist;
                        foundPriorityTarget = true;
                    }
                }
                else if (hit.CompareTag("Player") && !foundPriorityTarget)
                {
                    if (dist < closestDist)
                    {
                        bestTarget = hit.transform;
                        closestDist = dist;
                    }
                }
            }
            else if (enemyData.faction == FactionType.Defender)
            {
                if (hit.CompareTag("Alien"))
                {
                    if (dist < closestDist)
                    {
                        bestTarget = hit.transform;
                        closestDist = dist;
                    }
                }
            }
        }

        currentTarget = bestTarget;
    }

    Vector2 GetTargetDirection()
    {
        if (currentTarget != null)
        {
            return (currentTarget.position - transform.position).normalized;
        }
        
        if (enemyData.faction == FactionType.Alien) return Vector2.down;
        if (enemyData.faction == FactionType.Defender) return Vector2.up;
        return transform.up; 
    }

    // --- BEHAVIOR LOGIC ---

    void HandleBurstChaser()
    {
        Vector2 direction = GetTargetDirection();
        transform.up = direction;

        if (actionTimer <= 0)
        {
            rb.AddForce(direction * enemyData.burstForce, ForceMode2D.Impulse);
            actionTimer = Random.Range(enemyData.minBurstInterval, enemyData.maxBurstInterval);
        }
    }

    void HandleMomentumChaser()
    {
        Vector2 direction = GetTargetDirection();
        float angleToTarget = Vector2.SignedAngle(transform.up, direction);
        
        if (Mathf.Abs(angleToTarget) > 10f) 
        {
            currentMomentumSpeed -= enemyData.momentumLossMultiplier * Time.fixedDeltaTime;
            currentMomentumSpeed = Mathf.Max(currentMomentumSpeed, 0.5f);
        }
        else
        {
            currentMomentumSpeed += enemyData.acceleration * Time.fixedDeltaTime;
            currentMomentumSpeed = Mathf.Min(currentMomentumSpeed, enemyData.baseSpeed);
        }

        float step = enemyData.turnSpeed * Time.fixedDeltaTime;
        transform.up = Vector3.RotateTowards(transform.up, direction, step, 0.0f);

        rb.linearVelocity = transform.up * currentMomentumSpeed; 
    }

    void HandleSlowShooter()
    {
        Vector2 direction = GetTargetDirection();
        transform.up = direction;
        rb.linearVelocity = direction * enemyData.baseSpeed;
    }

    void Shoot()
    {
        if (enemyData.projectilePrefab != null)
        {
            GameObject bullet;

            if (enemyData.behaviorType == EnemyBehaviorType.StaticTurret)
            {
                bullet = Instantiate(enemyData.projectilePrefab, transform.position, Quaternion.identity);
                bullet.transform.up = GetTargetDirection(); 
            }
            else
            {
                bullet = Instantiate(enemyData.projectilePrefab, transform.position, transform.rotation);
            }

            Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();
            if (bulletRB != null)
            {
                bulletRB.AddForce(bullet.transform.up * enemyData.projectileSpeed, ForceMode2D.Impulse);
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
            rb.linearVelocity = directionToTarget * (enemyData.baseSpeed * Random.Range(0.5f, 1.5f));
        }
        else
        {
            rb.linearVelocity = directionToTarget * enemyData.baseSpeed;
        }
    }

    void PickNewWanderTarget()
    {
        wanderTarget = (Vector2)transform.position + Random.insideUnitCircle * 5f;
    }

    // --- IDAMAGEABLE ---
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
        Destroy(gameObject);
    }
    
    // --- TREADMILL SHIFT ---
    void OnEnable()
    {
        MNPlayerMovement.OnWorldShift += ShiftWithTreadmill;
    }

    void OnDisable()
    {
        MNPlayerMovement.OnWorldShift -= ShiftWithTreadmill;
    }

    void ShiftWithTreadmill(Vector3 shiftAmount)
    {
        if (rb != null)
        {
            rb.position += (Vector2)shiftAmount; // Fixes physics stutter
        }
        else
        {
            transform.position += shiftAmount;
        }
    }
}