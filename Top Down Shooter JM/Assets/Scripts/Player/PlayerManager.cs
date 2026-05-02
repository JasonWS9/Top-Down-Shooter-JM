using UnityEngine;
using System;

public class PlayerManager : MonoBehaviour, IDamageable
{
    public static PlayerManager Instance;

    [Header("Health Stats")]
    public int maxHealth;
    public int currentHealth;
    private int baseMaxHealth; 

    [Header("Leveling Settings")]
    public float healthGrowthFactor = 0.2f; 

    [Header("Invincibility Frames (I-Frames)")]
    public float invincibilityDuration = 1f;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    
    [Header("Power-Ups")]
    public bool hasShield = false;
    public bool isGodMode = false;
    private float godModeTimer = 0f;

    public GameObject normalShieldVisual;
    public GameObject godModeShieldVisual;
    
    // We use this to make the player flash when hit
    private SpriteRenderer spriteRenderer; 

    public static event Action<int, int> OnHealthUpdated;
    public static event Action OnPlayerDeath; 

    void Awake()
    {
        Instance = this;
        baseMaxHealth = maxHealth;
        currentHealth = maxHealth;
        
        // Grab the sprite renderer (whether it's on this object or a child object)
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(); 
    }

    void OnEnable()
    {
        GameManager.OnPlayerLevelUp += HandleLevelUp;
    }

    void OnDisable()
    {
        GameManager.OnPlayerLevelUp -= HandleLevelUp;
    }

    void Start()
    {
        OnHealthUpdated?.Invoke(currentHealth, maxHealth);
    }

    void Update()
    {
        TrackRegion();
        HandleInvincibility();

        // Tick down God Mode
        if (isGodMode)
        {
            godModeTimer -= Time.deltaTime;
            if (godModeTimer <= 0f)
            {
                isGodMode = false;
                if (godModeShieldVisual != null) godModeShieldVisual.SetActive(false);
            }
        }

        if (currentHealth <= 0) PlayerDeath();
    }

    void HandleInvincibility()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;

            // Flash the sprite to show invincibility
            if (spriteRenderer != null)
            {
                // Toggles the sprite completely invisible/visible rapidly
                spriteRenderer.enabled = Mathf.PingPong(Time.time * 15f, 1f) > 0.5f;
            }

            // Once the timer hits 0, turn off invincibility and ensure the sprite is visible
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                if (spriteRenderer != null) spriteRenderer.enabled = true;
            }
        }
    }

    public void TakeDamage(int damage, bool fromPlayer = false)
    {
        if (currentHealth <= 0 || isInvincible || isGodMode) return; 

        // If it's a negative number (healing from a health pack), ignore shields and just heal
        if (damage < 0)
        {
            currentHealth -= damage; // Subtracting a negative = adding
            if (currentHealth > maxHealth) currentHealth = maxHealth;
            OnHealthUpdated?.Invoke(currentHealth, maxHealth);
            return;
        }

        // Standard damage processing
        if (hasShield)
        {
            hasShield = false;
            if (normalShieldVisual != null) normalShieldVisual.SetActive(false);
            
            // Give them brief I-Frames after shield breaks so they don't get double-hit instantly
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
            return; 
        }

        currentHealth -= damage;
        OnHealthUpdated?.Invoke(currentHealth, maxHealth);

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    void HandleLevelUp(int newLevel)
    {
        float multiplier = 1f + (healthGrowthFactor * Mathf.Log(newLevel));
        maxHealth = Mathf.RoundToInt(baseMaxHealth * multiplier);
        
        currentHealth += Mathf.RoundToInt(baseMaxHealth * 0.2f);
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        OnHealthUpdated?.Invoke(currentHealth, maxHealth);
    }

    void TrackRegion()
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        float distanceFromCenterX = Mathf.Abs(viewportPos.x - 0.5f);
        float distanceFromCenterY = Mathf.Abs(viewportPos.y - 0.5f);
        float distanceThreshold = 0.2f; 

        if (distanceFromCenterX < distanceThreshold && distanceFromCenterY < distanceThreshold) return;
    }
    
    public void ActivateShield()
    {
        hasShield = true;
        if (normalShieldVisual != null) normalShieldVisual.SetActive(true);
    }

    public void ActivateGodMode(float duration)
    {
        isGodMode = true;
        godModeTimer = duration;
        if (godModeShieldVisual != null) godModeShieldVisual.SetActive(true);
    }

    void PlayerDeath()
    {
        OnPlayerDeath?.Invoke(); 
        gameObject.SetActive(false); 
    }
}