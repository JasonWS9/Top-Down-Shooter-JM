using UnityEngine;
using System;

public class MNLiabilityManager : MonoBehaviour, IDamageable
{
    public static MNLiabilityManager Instance;

    [Header("Corporate Metrics")]
    public float maxLiabilityLimit = 1000000f; // 1 Million in damages gets you fired
    public float currentLiability = 0f;
    public float earningsQuota = 0f;

    public static event Action OnFired;
    public static event Action<float> OnEarningsUpdated;

    void Awake()
    {
        Instance = this;
    }

    public void TakeDamage(int damage)
    {
        // Damage from enemy bullets adds to your liability
        IncreaseLiability(damage * 5000f); 
    }

    public void IncreaseLiability(float penaltyAmount)
    {
        currentLiability += penaltyAmount;
        Debug.Log($"Liability increased! Current: ${currentLiability}");

        if (currentLiability >= maxLiabilityLimit)
        {
            TerminateEmployment();
        }
    }

    public void AddEarnings(float profit)
    {
        earningsQuota += profit;
        OnEarningsUpdated?.Invoke(earningsQuota);
    }

    void TerminateEmployment()
    {
        Debug.Log("Liability limit exceeded. You are fired (Dead).");
        OnFired?.Invoke();
    }
}