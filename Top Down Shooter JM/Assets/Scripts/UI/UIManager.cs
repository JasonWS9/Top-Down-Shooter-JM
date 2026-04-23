using UnityEngine;
using UnityEngine.UI;
using TMPro; // Standard text system for Unity

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI scoreText;
    public Slider evolutionSlider; // Set Min=0, Max=1, Value=0.5 in Inspector

    void OnEnable()
    {
        // Listen to our existing PlayerManager health event
        PlayerManager.OnHealthUpdated += UpdateHealthUI;
        
        // Listen to the new GameManager events
        GameManager.OnScoreUpdated += UpdateScoreUI;
        GameManager.OnEvolutionMeterUpdated += UpdateEvolutionUI;
    }

    void OnDisable()
    {
        PlayerManager.OnHealthUpdated -= UpdateHealthUI;
        GameManager.OnScoreUpdated -= UpdateScoreUI;
        GameManager.OnEvolutionMeterUpdated -= UpdateEvolutionUI;
    }

    void Start()
    {
        // Set defaults
        UpdateScoreUI(0);
        UpdateEvolutionUI(0.5f); 
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
}