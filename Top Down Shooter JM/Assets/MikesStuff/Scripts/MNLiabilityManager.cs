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
        if (killedByPlayer)
        {
            // 1. Tell the Moral Compass to shift the gauge!
            if (MNAlignmentManager.Instance != null) 
            {
                MNAlignmentManager.Instance.EvaluateAction(killedFaction, true);
            }

            // 2. Distribute rewards based on your active track
            if (MNAlignmentManager.Instance.currentTrack == MNAlignmentManager.PlayTrack.Corporate)
            {
                // CORPORATE TRACK REWARDS
                if (killedFaction == FactionType.Alien) MNAlignmentManager.Instance.corporateMoney += 500f;
                if (killedFaction == FactionType.Defender) IncreaseLiability(50000f); // Still getting fined!
            }
            else 
            {
                // HERO TRACK REWARDS
                MNAlignmentManager.Instance.heroXP += 100;
                
                // You only get reputation if you kill the ENEMY of your chosen faction
                if (MNAlignmentManager.Instance.currentTrack == MNAlignmentManager.PlayTrack.HeroDefender && killedFaction == FactionType.Alien)
                {
                    MNAlignmentManager.Instance.heroReputation += 10;
                }
                else if (MNAlignmentManager.Instance.currentTrack == MNAlignmentManager.PlayTrack.HeroAlien && killedFaction == FactionType.Defender)
                {
                    MNAlignmentManager.Instance.heroReputation += 10;
                }
            }
        }

        if (killedFaction == FactionType.Alien)
        {
            totalAlienKills++;
            if (MNSpawnManager.Instance != null) MNSpawnManager.Instance.RecordAlienDeath(); 
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