using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

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
    
    [Header("Faction Levels")]
    public int purpleLevel = 1;
    public int orangeLevel = 1;

    // Events to broadcast to the UI Manager
    public static event Action<int> OnScoreUpdated;
    public static event Action<float> OnEvolutionMeterUpdated;

    void Awake()
    {
        Instance = this;
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
        float timeMultiplier = 1f + (timeSurvived / 60f); // Earn more points the longer you survive
        float accuracyMultiplier = (consecutiveHits >= 10) ? 2f : 1f; // Double points if on a streak!

        int pointsEarned = Mathf.RoundToInt(basePoints * level * comboMultiplier * timeMultiplier * accuracyMultiplier);
        currentScore += pointsEarned;

        OnScoreUpdated?.Invoke(currentScore);
    }

    void UpdateEvolutionMeter(EnemyFaction faction)
    {
        if (faction == EnemyFaction.Purple) evolutionMeter -= pointsPerKill;
        else if (faction == EnemyFaction.Orange) evolutionMeter += pointsPerKill;

        // Check for Evolution
        if (evolutionMeter <= -100)
        {
            purpleLevel++; // INCREASE PURPLE LEVEL
            Debug.Log($"PURPLE FACTION EVOLVED TO LEVEL {purpleLevel}!");
            evolutionMeter = 0; 
        }
        else if (evolutionMeter >= 100)
        {
            orangeLevel++; // INCREASE ORANGE LEVEL
            Debug.Log($"ORANGE FACTION EVOLVED TO LEVEL {orangeLevel}!");
            evolutionMeter = 0; 
        }

        float normalizedMeter = (evolutionMeter + 100f) / 200f;
        OnEvolutionMeterUpdated?.Invoke(normalizedMeter);
    }
}