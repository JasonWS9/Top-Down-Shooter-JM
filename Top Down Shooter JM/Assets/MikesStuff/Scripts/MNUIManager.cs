using UnityEngine;
using TMPro; // Required for TextMeshPro
using System;

public class MNUIManager : MonoBehaviour
{
    [Header("Invasion Metrics")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI aliensLeftText;
    
    [Header("Combat Metrics")]
    public TextMeshProUGUI alienKillsText;
    public TextMeshProUGUI penaltiesText; // Civilian/Defender kills
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI xpText;

    [Header("Corporate Metrics")]
    public TextMeshProUGUI destructionText;
    public TextMeshProUGUI liabilityText;

    private float timeElapsed = 0f;

    void OnEnable()
    {
        // Subscribe to all the broadcasts
        MNLiabilityManager.OnLiabilityUpdated += UpdateLiability;
        MNLiabilityManager.OnDestructionUpdated += UpdateDestruction;
        MNLiabilityManager.OnStatsUpdated += UpdateCombatStats;
        MNSpawnManager.OnInvasionCountUpdated += UpdateAliensLeft;
        
        // Assuming your PlayerManager has a health update event
        MNPlayerManager.OnHealthUpdated += UpdateHealth; 
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        MNLiabilityManager.OnLiabilityUpdated -= UpdateLiability;
        MNLiabilityManager.OnDestructionUpdated -= UpdateDestruction;
        MNLiabilityManager.OnStatsUpdated -= UpdateCombatStats;
        MNSpawnManager.OnInvasionCountUpdated -= UpdateAliensLeft;
        MNPlayerManager.OnHealthUpdated -= UpdateHealth;
    }

    void Start()
    {
        // Initialize default text
        UpdateLiability(MNLiabilityManager.Instance.currentLiability);
        UpdateDestruction(MNLiabilityManager.Instance.totalDestructionValue);
        UpdateCombatStats(0, 0, 0);
    }

    void Update()
    {
        // Handle the running clock
        timeElapsed += Time.deltaTime;
        TimeSpan time = TimeSpan.FromSeconds(timeElapsed);
        timeText.text = $"INVASION DURATION: {time.Minutes:D2}:{time.Seconds:D2}";
    }

    // --- BROADCAST RECEIVERS ---

    void UpdateLiability(float amount)
    {
        liabilityText.text = $"LIABILITY: ${amount:N0} / $1,000,000";
    }

    void UpdateDestruction(int amount)
    {
        // Let's color code it based on your Greenzone!
        if (amount < MNLiabilityManager.Instance.greenzoneMin)
            destructionText.text = $"DESTRUCTION: <color=yellow>{amount}</color> (Under Quota)";
        else if (amount <= MNLiabilityManager.Instance.greenzoneMax)
            destructionText.text = $"DESTRUCTION: <color=green>{amount}</color> (Optimal)";
        else
            destructionText.text = $"DESTRUCTION: <color=red>{amount}</color> (CRITICAL)";
    }

    void UpdateCombatStats(int alienKills, int penaltyKills, int xp)
    {
        alienKillsText.text = $"ALIENS AUDITED: {alienKills}";
        penaltiesText.text = $"UNAUTHORIZED CASUALTIES: {penaltyKills}";
        xpText.text = $"CORP XP: {xp}";
    }

    void UpdateHealth(int currentHealth, int maxHealth)
    {
        healthText.text = $"HULL INTEGRITY: {currentHealth}/{maxHealth}";
    }

    void UpdateAliensLeft(int count)
    {
        aliensLeftText.text = $"INVASION FORCES REMAINING: {count}";
    }
}