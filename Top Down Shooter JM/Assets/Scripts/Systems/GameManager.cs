using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Leveling")]
    public int playerLevel = 1;
    public int pointsPerLevel = 2000;
    
    [Header("Score System")]
    public int currentScore = 0;
    public float timeSurvived = 0f;
    private int consecutiveHits = 0;
    
    [Header("Combo Tracking")]
    public EnemyFaction currentComboFaction;
    public int comboCount = 0;

    [Header("Evolution Meter")]
    [Tooltip("Ranges from -100 (Purple) to 100 (Orange). Starts at 0.")]
    public int evolutionMeter = 0;
    public int pointsPerKill = 10;
    
    [Header("Dynamic Difficulty (Dodging)")]
    public float evaluationWindow = 5f; // Check every 5 seconds
    public int highApmThreshold = 15;   // How many actions count as "trying hard"
    public int evolutionBonusForDodging = 15; // Bonus points given to the enemies if player is dodging well
    
    private float evaluationTimer;
    private int currentWindowActions = 0;
    private int healthAtStartOfWindow;

    [Header("Faction Levels")]
    public int purpleLevel = 1;
    public int orangeLevel = 1;
    
    [Header("Power-Ups")]
    public float doubleScoreTimer = 0f;

    // Events
    public static event Action<int> OnScoreUpdated;
    public static event Action<float> OnEvolutionMeterUpdated;
    public static event Action<int> OnPlayerLevelUp;

    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        healthAtStartOfWindow = PlayerManager.Instance.currentHealth;
        evaluationTimer = evaluationWindow;
    }

    void OnEnable()
    {
        EnemyController.OnEnemyKilled += HandleEnemyKill;
        Projectile.OnShotResolved += HandleShotResolved;
    }

    void OnDisable()
    {
        EnemyController.OnEnemyKilled -= HandleEnemyKill;
        Projectile.OnShotResolved -= HandleShotResolved;
    }

    void Update()
    {
        timeSurvived += Time.deltaTime;
        EvaluateDodgingEfficiency();
        
        // Tick down the score multiplier
        if (doubleScoreTimer > 0f) doubleScoreTimer -= Time.deltaTime;
    }
    
    // Called by the PlayerMovement script
    public void RegisterAction()
    {
        currentWindowActions++;
    }

    void HandleShotResolved(bool hitTarget)
    {
        if (hitTarget)
        {
            consecutiveHits++;
        }
        else
        {
            consecutiveHits = 0; // Reset streak if a shot misses
        }
    }

    void HandleEnemyKill(EnemyFaction faction, int enemyLevel)
    {
        // 1. Process Combo
        if (faction == currentComboFaction)
        {
            comboCount++;
        }
        else
        {
            currentComboFaction = faction;
            comboCount = 1; // Reset to 1 for the new faction
        }

        // 2. Process Score
        CalculateAndAddScore(enemyLevel);

        // 3. Process Evolution Meter
        UpdateEvolutionMeter(faction);
    }

    void CalculateAndAddScore(int level)
    {
        int basePoints = 100;
        float comboMultiplier = 1f + (comboCount * 0.1f);
        float timeMultiplier = 1f + (timeSurvived / 60f); 
        float accuracyMultiplier = (consecutiveHits >= 10) ? 2f : 1f; 

        // Apply Power-Up Multiplier!
        float powerUpMultiplier = (doubleScoreTimer > 0f) ? 2f : 1f;

        int pointsEarned = Mathf.RoundToInt(basePoints * level * comboMultiplier * timeMultiplier * accuracyMultiplier * powerUpMultiplier);
        currentScore += pointsEarned;

        OnScoreUpdated?.Invoke(currentScore);

        // Check for Player Level Up
        int calculatedLevel = (currentScore / pointsPerLevel) + 1; 

        if (calculatedLevel > playerLevel)
        {
            playerLevel = calculatedLevel;
            Debug.Log($"PLAYER LEVELED UP TO {playerLevel}!");
            OnPlayerLevelUp?.Invoke(playerLevel);
        }
    }

    void UpdateEvolutionMeter(EnemyFaction faction, int amount = -1)
    {
        // If no custom amount is passed, use the default pointsPerKill
        int pointsToApply = (amount == -1) ? pointsPerKill : amount;

        if (faction == EnemyFaction.Purple) evolutionMeter -= pointsToApply;
        else if (faction == EnemyFaction.Orange) evolutionMeter += pointsToApply;

        // Check for Evolution
        if (evolutionMeter <= -100)
        {
            purpleLevel++; 
            Debug.Log($"PURPLE FACTION EVOLVED TO LEVEL {purpleLevel}!");
            evolutionMeter = 0; 
        }
        else if (evolutionMeter >= 100)
        {
            orangeLevel++; 
            Debug.Log($"ORANGE FACTION EVOLVED TO LEVEL {orangeLevel}!");
            evolutionMeter = 0; 
        }

        // Convert the -100 to 100 scale into a 0.0 to 1.0 scale for the UI Slider
        float normalizedMeter = (evolutionMeter + 100f) / 200f;
        OnEvolutionMeterUpdated?.Invoke(normalizedMeter);
    }
    
    // NEW: The Professor's Logic
    void EvaluateDodgingEfficiency()
    {
        evaluationTimer -= Time.deltaTime;

        if (evaluationTimer <= 0f)
        {
            int currentHealth = PlayerManager.Instance.currentHealth;
            int healthLost = healthAtStartOfWindow - currentHealth;

            // THE CORE LOGIC: High APM + No Damage Taken = Effective Dodging!
            if (currentWindowActions >= highApmThreshold && healthLost <= 0)
            {
                Debug.Log($"[DDA] High APM ({currentWindowActions}) & No Damage! Accelerating Enemy Evolution.");
                
                // Punish the player for being too good by boosting the current combo faction's meter
                UpdateEvolutionMeter(currentComboFaction, evolutionBonusForDodging);
            }

            // Reset for the next 5-second window
            currentWindowActions = 0;
            healthAtStartOfWindow = currentHealth;
            evaluationTimer = evaluationWindow;
        }
    }
    
    public void ActivateScoreMultiplier(float duration)
    {
        doubleScoreTimer = duration;
        Debug.Log("DOUBLE SCORE ACTIVE!");
    }
}