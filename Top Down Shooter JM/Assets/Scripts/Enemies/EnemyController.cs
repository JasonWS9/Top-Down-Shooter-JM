using UnityEngine;
using System;

public class EnemyController : MonoBehaviour, IDamageable
{
    public static event Action<EnemyFaction, int> OnEnemyKilled;

    [Header("Assigned Data")]
    public EnemyData myData;
    public int currentLevel = 1;

    [Header("Live Stats (Calculated)")]
    public float currentHealth;
    public float maxHealth;
    public float currentSpeed;
    public float currentDamage;
    public SpecialAbility currentAbility = SpecialAbility.None;
    
    private bool hasFiredLowHealthCannon = false;
    private Color factionColor; // NEW: Store their original color so we can reset it after they flash!

    [Header("Spawn Settings")]
    public float spawnDelay = 1f;
    private bool isSpawning = true;
    public Animator enemyAnimator;

    private SpriteRenderer spriteRenderer;
    private Transform playerTarget;
    private float fireTimer;

    [Header("Movement States")]
    private Vector2 wanderDirection;
    private float wanderTimer;
    private bool isAggroed = false; 
    private bool isDashing = false; 
    
    // --- NEW: Rammer Wind-Up Variables ---
    private bool isWindingUp = false;
    private float windUpTimer = 0f;
    private Vector2 dashDirection;

    void OnEnable()
    {
        PlayerMovement.OnWorldShift += ShiftWithWorld;
    }

    void OnDisable()
    {
        PlayerMovement.OnWorldShift -= ShiftWithWorld;
    }

    void ShiftWithWorld(Vector3 shiftAmount)
    {
        transform.position += shiftAmount;
    }

    public void Initialize(EnemyData data, int spawnLevel)
    {
        myData = data;
        currentLevel = spawnLevel;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (PlayerManager.Instance != null)
        {
            playerTarget = PlayerManager.Instance.transform;
            transform.up = (playerTarget.position - transform.position).normalized;
        }

        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("PlayPortal");
        }

        ApplyDataAndFactionModifiers();
        ApplyEvolutionModifiers();
        
        maxHealth = currentHealth; 
        fireTimer = myData.fireRate;
        wanderTimer = 0f; 
    }

    void ApplyDataAndFactionModifiers()
    {
        currentHealth = myData.baseHealth;
        currentSpeed = myData.baseSpeed;
        currentDamage = myData.attackDamage;

        switch (myData.faction)
        {
            case EnemyFaction.Purple:
                currentSpeed *= 0.8f;      
                currentHealth *= 1.5f;     
                if (spriteRenderer != null) spriteRenderer.color = new Color(0.5f, 0f, 0.5f); 
                break;

            case EnemyFaction.Orange:
                currentSpeed *= 1.2f;      
                currentDamage *= 1.2f;     
                if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.5f, 0f); 
                break;
        }
        
        // Save the color so we know what to revert to after flashing
        if (spriteRenderer != null) factionColor = spriteRenderer.color; 
    }

    void ApplyEvolutionModifiers()
    {
        if (myData.evolutionTiers == null || myData.evolutionTiers.Count == 0) return;

        foreach (EvolutionTier tier in myData.evolutionTiers)
        {
            if (currentLevel >= tier.requiredLevel)
            {
                currentHealth *= tier.healthMultiplier;
                currentSpeed *= tier.speedMultiplier;
                currentDamage *= tier.damageMultiplier;
                
                if (tier.unlockedAbility != SpecialAbility.None)
                {
                    currentAbility = tier.unlockedAbility;
                }

                if (tier.evolutionSprite != null && spriteRenderer != null)
                {
                    spriteRenderer.sprite = tier.evolutionSprite;
                }
                
                if (tier.evolutionAnimator != null && enemyAnimator != null)
                {
                    enemyAnimator.runtimeAnimatorController = tier.evolutionAnimator;
                }
            }
        }
    }

    void Update()
    {
        if (isSpawning)
        {
            spawnDelay -= Time.deltaTime;
            if (spawnDelay <= 0f) isSpawning = false;
            return; 
        }

        if (playerTarget == null || myData == null) return;

        if (fireTimer > 0f) fireTimer -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        switch (myData.enemyType)
        {
            case EnemyType.SingleShot:
                HandleSingleShot(distanceToPlayer);
                break;
            case EnemyType.BurstShot:
                HandleBurstShot(distanceToPlayer);
                break;
            case EnemyType.Rammer:
                HandleRammer(distanceToPlayer);
                break;
        }
    }

    // ==========================================
    // UNIQUE ENEMY BEHAVIORS
    // ==========================================

    void HandleSingleShot(float distance)
    {
        transform.up = (playerTarget.position - transform.position).normalized;

        if (distance > myData.attackRange)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerTarget.position, currentSpeed * Time.deltaTime);
        }
        else
        {
            AttackBehavior();
        }
    }

    void HandleBurstShot(float distance)
    {
        float aggroRadius = myData.attackRange * 1.5f;

        if (distance <= aggroRadius) isAggroed = true; 
        else if (distance > aggroRadius * 1.5f) isAggroed = false; 

        if (isAggroed)
        {
            transform.up = (playerTarget.position - transform.position).normalized;

            if (distance > myData.attackRange)
            {
                transform.position = Vector2.MoveTowards(transform.position, playerTarget.position, currentSpeed * Time.deltaTime);
            }
            else
            {
                AttackBehavior();
            }
        }
        else
        {
            WanderTowardsCenter(straightLine: true);
        }
    }

    void HandleRammer(float distance)
    {
        if (isDashing)
        {
            // State 3: Launch!
            transform.position += (Vector3)(dashDirection * currentSpeed * 3.5f * Time.deltaTime);

            Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
            
            if (viewportPos.x <= -0.1f || viewportPos.x >= 1.1f || viewportPos.y <= -0.1f || viewportPos.y >= 1.1f)
            {
                isDashing = false;
                wanderTimer = 0f; 
            }
        }
        else if (isWindingUp)
        {
            // State 2: Wind-up / Telegraphing!
            windUpTimer -= Time.deltaTime;
            
            // Rapidly flash white to warn the player!
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(factionColor, Color.white, Mathf.PingPong(Time.time * 20f, 1f));
            }

            // Once the timer hits 0, execute the dash
            if (windUpTimer <= 0f)
            {
                isWindingUp = false;
                isDashing = true;
                
                // Reset their color back to normal
                if (spriteRenderer != null) spriteRenderer.color = factionColor;
            }
        }
        else
        {
            // State 1: Hunting
            if (distance <= myData.attackRange) 
            {
                AttackBehavior(); 
            }
            else
            {
                WanderTowardsCenter(straightLine: false);
            }
        }
    }

    // ==========================================
    // WANDERING & ATTACKING
    // ==========================================

    void WanderTowardsCenter(bool straightLine)
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f)
        {
            wanderTimer = UnityEngine.Random.Range(2f, 4f);
            wanderDirection = GetBiasedWanderDirection();
        }

        if (straightLine)
        {
            transform.position += (Vector3)(wanderDirection * currentSpeed * 0.6f * Time.deltaTime);
            transform.up = wanderDirection;
        }
        else
        {
            Vector3 forwardMove = wanderDirection * currentSpeed * 0.8f * Time.deltaTime;
            Vector2 perpDir = new Vector2(-wanderDirection.y, wanderDirection.x);
            float sinOffset = Mathf.Sin(Time.time * 6f) * 2.5f; 
            Vector3 snakeMove = perpDir * sinOffset * Time.deltaTime;

            transform.position += forwardMove + snakeMove;
            transform.up = wanderDirection; 
        }
    }

    Vector2 GetBiasedWanderDirection()
    {
        Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
        Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;

        if (vp.x > 0.2f && vp.x < 0.8f && vp.y > 0.2f && vp.y < 0.8f)
        {
            return randomDir;
        }
        else
        {
            Vector2 toCenter = ((Vector2)Camera.main.transform.position - (Vector2)transform.position).normalized;
            return (randomDir + toCenter * 2f).normalized; 
        }
    }

    void AttackBehavior()
    {
        if (fireTimer <= 0f)
        {
            switch (myData.enemyType)
            {
                case EnemyType.SingleShot:
                    FireProjectile(1, 0f);
                    break;
                case EnemyType.BurstShot:
                    FireProjectile(3, 15f); 
                    break;
                case EnemyType.Rammer:
                    // --- NEW: Trigger the Wind-Up instead of dashing instantly! ---
                    isWindingUp = true;
                    windUpTimer = 0.5f; // Wait half a second before striking
                    
                    // Lock onto the player's CURRENT position
                    dashDirection = (playerTarget.position - transform.position).normalized;
                    transform.up = dashDirection; 
                    break;
            }
            fireTimer = myData.fireRate;
        }
    }

    void FireProjectile(int amountOfBullets, float spreadAngle)
    {
        if (myData.projectilePrefab == null) return;

        float startingAngle = (amountOfBullets > 1) ? -spreadAngle * (amountOfBullets - 1) / 2f : 0f;

        for (int i = 0; i < amountOfBullets; i++)
        {
            Quaternion rotation = transform.rotation * Quaternion.Euler(0, 0, startingAngle + (spreadAngle * i));
            GameObject bullet = Instantiate(myData.projectilePrefab, transform.position, rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            
            if (rb != null) rb.AddForce(bullet.transform.up * myData.projectileSpeed, ForceMode2D.Impulse);
        }
    }

    // ==========================================
    // COLLISIONS & DAMAGE
    // ==========================================

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isSpawning) return;

        PlayerManager player = collision.gameObject.GetComponent<PlayerManager>();
        if (player != null)
        {
            // GOD MODE CHECK
            if (player.isGodMode)
            {
                TakeDamage(99999, true); // Instant Death
                return;
            }

            player.TakeDamage(Mathf.RoundToInt(currentDamage));
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (isSpawning) return;

        PlayerManager player = collision.gameObject.GetComponent<PlayerManager>();
        if (player != null)
        {
            // GOD MODE CHECK
            if (player.isGodMode)
            {
                TakeDamage(99999, true); // Instant Death
                return;
            }

            player.TakeDamage(Mathf.RoundToInt(currentDamage));
        }
    }

    public void TakeDamage(int damage, bool fromPlayer = false)
    {
        if (isSpawning) return; 

        currentHealth -= damage;
        
        if (currentAbility == SpecialAbility.CannonAtLowHealth && !hasFiredLowHealthCannon)
        {
            if (currentHealth <= maxHealth * 0.7f)
            {
                hasFiredLowHealthCannon = true;
                FireProjectile(5, 10f); 
            }
        }

        if (currentHealth <= 0)
        {
            if (currentAbility == SpecialAbility.SpawnMinionsOnDeath && EnemySpawner.Instance != null)
            {
                EnemySpawner.Instance.SpawnSpecificEnemyAtLocation(myData, 1, transform.position + new Vector3(1, 1, 0));
                EnemySpawner.Instance.SpawnSpecificEnemyAtLocation(myData, 1, transform.position + new Vector3(-1, -1, 0));
            }

            if (fromPlayer) 
            {
                OnEnemyKilled?.Invoke(myData.faction, currentLevel);
                
                // --- NEW: Try to drop a power-up before dying! ---
                // We only drop loot if the PLAYER killed them, to prevent exploiting minions!
                PowerUpDropper dropper = GetComponent<PowerUpDropper>();
                if (dropper != null)
                {
                    dropper.TryDrop(currentLevel); // Pass their current level into the math!
                }
            }
            Destroy(gameObject);
        }
    }
}