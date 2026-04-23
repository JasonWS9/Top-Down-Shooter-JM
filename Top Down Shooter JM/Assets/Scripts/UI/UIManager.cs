using UnityEngine;
using UnityEngine.UI;
using TMPro; // Standard text system for Unity

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText; // The UI text for the player's level
    public Slider evolutionSlider; // Set Min=0, Max=1, Value=0.5 in Inspector

    void OnEnable()
    {
        // Listen to our existing PlayerManager events
        PlayerManager.OnHealthUpdated += UpdateHealthUI;
        
        // Listen to the GameManager events
        GameManager.OnScoreUpdated += UpdateScoreUI;
        GameManager.OnEvolutionMeterUpdated += UpdateEvolutionUI;
        GameManager.OnPlayerLevelUp += UpdateLevelUI; // Listen for level ups
    }

    void OnDisable()
    {
        PlayerManager.OnHealthUpdated -= UpdateHealthUI;
        GameManager.OnScoreUpdated -= UpdateScoreUI;
        GameManager.OnEvolutionMeterUpdated -= UpdateEvolutionUI;
        GameManager.OnPlayerLevelUp -= UpdateLevelUI; // Stop listening
    }

    void Start()
    {
        // Set defaults
        UpdateScoreUI(0);
        UpdateEvolutionUI(0.5f); 
        UpdateLevelUI(1); // Player starts at Level 1
    }

    void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {currentHealth} / {maxHealth}";
        }
    }

    void UpdateScoreUI(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = $"SCORE: {newScore}";
        }
    }

    void UpdateEvolutionUI(float normalizedValue)
    {
        if (evolutionSlider != null)
        {
            evolutionSlider.value = normalizedValue;
        }
    }

    // Method to update the level text
    void UpdateLevelUI(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"LVL: {level}";
        }
    }
}