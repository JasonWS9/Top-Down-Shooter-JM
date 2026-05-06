using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem; // Standard text system for Unity

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Elements")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText; // NEW: The UI text for the player's level
    public Slider evolutionSlider; // Set Min=0, Max=1, Value=0.5 in Inspector

    public GameObject pauseUI;
    
    private InputAction pauseAction;
    private bool isPaused = false;

    void OnEnable()
    {
        // Listen to our existing PlayerManager events
        PlayerManager.OnHealthUpdated += UpdateHealthUI;
        
        // Listen to the GameManager events
        GameManager.OnScoreUpdated += UpdateScoreUI;
        GameManager.OnEvolutionMeterUpdated += UpdateEvolutionUI;
        GameManager.OnPlayerLevelUp += UpdateLevelUI; // NEW: Listen for level ups!
    }

    void OnDisable()
    {
        PlayerManager.OnHealthUpdated -= UpdateHealthUI;
        GameManager.OnScoreUpdated -= UpdateScoreUI;
        GameManager.OnEvolutionMeterUpdated -= UpdateEvolutionUI;
        GameManager.OnPlayerLevelUp -= UpdateLevelUI; // NEW: Stop listening
    }

    void Awake()
    {
       Instance = this; 
       pauseAction = InputSystem.actions.FindAction("Pause");
    }

    void Start()
    {
        pauseUI.SetActive(false);
        isPaused = false;

        // Set defaults
        UpdateScoreUI(0);
        UpdateEvolutionUI(0.5f); 
        UpdateLevelUI(1); // NEW: Player starts at Level 1
    }

    void Update()
    {
        if (pauseAction.WasPressedThisFrame())
        {
            if (isPaused)
            {
                UnpauseGame();
            } else
            {
                PauseGame();
            }
        }
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

    // NEW: Method to update the level text
    void UpdateLevelUI(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"LVL: {level}";
        }
    }

    public void PauseGame()
    {
        //Play pausing sound effect
        isPaused = true;
        pauseUI.SetActive(true);
        Time.timeScale = 0;
    }

    public void UnpauseGame()
    {
        //Play unpausing sound effect
        isPaused = false;
        pauseUI.SetActive(false);
        Time.timeScale = 1;
    }
}