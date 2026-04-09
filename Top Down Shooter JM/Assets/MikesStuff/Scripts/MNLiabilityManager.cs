using UnityEngine;
using System;

public class MNLiabilityManager : MonoBehaviour, IDamageable
{
    public static MNLiabilityManager Instance;

    [Header("Corporate Metrics")]
    public float maxLiabilityLimit = 1000000f; // 1 Million in damages gets you fired
    public float currentLiability = 0f;
    public float earningsQuota = 0f;

    [Header("Planetary Audit (Greenzone)")]
    public int totalDestructionValue = 0;
    public int greenzoneMin = 500;
    public int greenzoneMax = 800;
    
    [Header("Combat & Experience")]
    public int totalAlienKills = 0;
    public int totalPenaltyKills = 0; // Friendly fire
    public int currentXP = 0;

    // Add these new events to the top with your others
    public static event Action<float> OnLiabilityUpdated;
    public static event Action<int, int, int> OnStatsUpdated; // Kills, Penalties, XP

    public static event Action OnFired;
    public static event Action<float> OnEarningsUpdated;
    public static event Action<int> OnDestructionUpdated; // New event for UI

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        // Listen for buildings getting wrecked!
        MNStructureController.OnAuditValueChanged += AddDestruction;
    }

    void OnDisable()
    {
        MNStructureController.OnAuditValueChanged -= AddDestruction;
    }

    // --- PLAYER HEALTH LOGIC ---
    
    public void TakeDamage(int damage, bool fromPlayer = false)
    {
        // Damage from enemy bullets adds to your liability
        IncreaseLiability(damage * 5000f); 
    }

    public void IncreaseLiability(float penaltyAmount)
    {
        currentLiability += penaltyAmount;
        OnLiabilityUpdated?.Invoke(currentLiability); // Broadcast to UI

        if (currentLiability >= maxLiabilityLimit) TerminateEmployment();
    }

    void TerminateEmployment()
    {
        Debug.Log("Liability limit exceeded. You are fired (Dead).");
        OnFired?.Invoke();
    }

    // --- EARNINGS & AUDIT LOGIC ---

    public void AddEarnings(float profit)
    {
        earningsQuota += profit;
        OnEarningsUpdated?.Invoke(earningsQuota);
    }

    void AddDestruction(int valueAdded)
    {
        totalDestructionValue += valueAdded;
        OnDestructionUpdated?.Invoke(totalDestructionValue);
        CheckGreenzone();
    }

    void CheckGreenzone()
    {
        if (totalDestructionValue < greenzoneMin)
        {
            Debug.Log($"IRS Status: Too clean. Current Destruction: {totalDestructionValue}. Target: {greenzoneMin}");
        }
        else if (totalDestructionValue >= greenzoneMin && totalDestructionValue <= greenzoneMax)
        {
            Debug.Log($"IRS Status: PERFECT DESTRUCTION ZONE. Hold the line!");
        }
        else
        {
            Debug.Log($"IRS Status: BANKRUPTCY. The aliens destroyed too much ({totalDestructionValue}).");
            // You can call TerminateEmployment() here too if over-destruction gets you fired!
        }
    }

    // --- NEW METHOD FOR XP AND KILLS ---
    public void RecordKill(FactionType killedFaction, bool killedByPlayer)
    {
        if (killedFaction == FactionType.Alien)
        {
            totalAlienKills++;
            
            int xpGained = killedByPlayer ? 100 : 25; 
            currentXP += xpGained;

            // ---> ADD THIS LINE <---
            // Tell the Spawner to tick down the "Aliens Remaining" UI clock!
            if (MNSpawnManager.Instance != null) MNSpawnManager.Instance.RecordAlienDeath(); 
        }
        else if (killedFaction == FactionType.Defender || killedFaction == FactionType.Neutral)
        {
            if (killedByPlayer)
            {
                totalPenaltyKills++;
                IncreaseLiability(50000f); 
            }
        }

        OnStatsUpdated?.Invoke(totalAlienKills, totalPenaltyKills, currentXP);
    }
    
    // Call this from MNEnemyController or Bullet when an alien is damaged by the player
    public void AddAssistXP(int amount)
    {
        currentXP += amount;
        OnStatsUpdated?.Invoke(totalAlienKills, totalPenaltyKills, currentXP);
    }
}