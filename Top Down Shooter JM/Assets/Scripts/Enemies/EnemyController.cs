using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Assigned Data")]
    public EnemyData myData;

    [Header("Live Stats (Calculated)")]
    public float currentHealth;
    public float currentSpeed;
    public float currentDamage;

    [Header("Spawn Settings")]
    public float spawnDelay = 1f;
    private bool isSpawning = true;
    public Animator enemyAnimator;

    private SpriteRenderer spriteRenderer;
    private Transform playerTarget;
    
    // Timer to control how often the enemy attacks
    private float fireTimer;

    // 1. Subscribe to the treadmill event so enemies shift with the world!
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
        // When the player pushes the world left, push the enemy left too
        transform.position += shiftAmount;
    }

    public void Initialize(EnemyData data)
    {
        myData = data;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (PlayerManager.Instance != null)
        {
            playerTarget = PlayerManager.Instance.transform;
        }

        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("PlayPortal");
        }

        ApplyDataAndFactionModifiers();
        
        // Initialize the fire timer so they don't shoot the exact frame they wake up
        fireTimer = myData.fireRate;
        
        Debug.Log($"[{gameObject.name}] Initialized! Faction: {myData.faction}, Type: {myData.enemyType}. Waiting {spawnDelay}s to spawn.");
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
    }

    void Update()
    {
        if (isSpawning)
        {
            spawnDelay -= Time.deltaTime;
            
            if (spawnDelay <= 0f)
            {
                isSpawning = false;
                Debug.Log($"[{gameObject.name}] Finished spawning! Waking up and hunting player.");
            }
            return; 
        }

        if (playerTarget == null || myData == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        // State Machine Logic
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
        // 1. Keep aiming at the player even while standing still to shoot
        Vector2 direction = (playerTarget.position - transform.position).normalized;
        transform.up = direction;

        // 2. Tick down the attack timer
        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f)
        {
            Debug.Log($"[{gameObject.name}] is attacking! (Type: {myData.enemyType})");
            
            // 3. Execute the actual attack based on type
            switch (myData.enemyType)
            {
                case EnemyType.SingleShot:
                    FireProjectile(1, 0f);
                    break;
                case EnemyType.BurstShot:
                    FireProjectile(3, 15f); // 3 bullets with a 15-degree spread
                    break;
                case EnemyType.Rammer:
                    Debug.Log($"[{gameObject.name}] triggered Ram Attack! Dealing {currentDamage} damage.");
                    // Deal damage directly if close enough, or lunge forward
                    if (Vector2.Distance(transform.position, playerTarget.position) <= 2f)
                    {
                        PlayerManager.Instance.TakeDamage(Mathf.RoundToInt(currentDamage));
                    }
                    break;
            }
            
            // 4. Reset the timer
            fireTimer = myData.fireRate;
        }
    }

    void FireProjectile(int amountOfBullets, float spreadAngle)
    {
        if (myData.projectilePrefab == null)
        {
            Debug.LogWarning($"[{gameObject.name}] tried to shoot but has no projectilePrefab assigned in its EnemyData!");
            return;
        }

        // Calculate the starting angle if we are firing a spread
        float startingAngle = (amountOfBullets > 1) ? -spreadAngle * (amountOfBullets - 1) / 2f : 0f;

        for (int i = 0; i < amountOfBullets; i++)
        {
            // Rotate the bullet according to its place in the spread
            Quaternion rotation = transform.rotation * Quaternion.Euler(0, 0, startingAngle + (spreadAngle * i));
            
            GameObject bullet = Instantiate(myData.projectilePrefab, transform.position, rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            
            if (rb != null)
            {
                rb.AddForce(bullet.transform.up * myData.projectileSpeed, ForceMode2D.Impulse);
            }
        }
    }

    public void TakeDamage(int damage, bool fromPlayer = false)
    {
        if (isSpawning) return; 

        currentHealth -= damage;
        Debug.Log($"[{gameObject.name}] took {damage} damage! Remaining Health: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Debug.Log($"[{gameObject.name}] destroyed!");
            Destroy(gameObject);
        }
    }
}