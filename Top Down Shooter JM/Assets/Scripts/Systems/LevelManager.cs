using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events; // Required for UnityEvents

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Game Over Settings")]
    public float delayBeforeRestart = 2f;
    
    [Tooltip("Drag your Game Over UI Panel or Sound Effects here to trigger them!")]
    public UnityEvent onGameOver;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        // Listen for the player dying
        PlayerManager.OnPlayerDeath += HandleGameOver;
    }

    void OnDisable()
    {
        PlayerManager.OnPlayerDeath -= HandleGameOver;
    }

    void HandleGameOver()
    {
        Debug.Log("Game Over Sequence Started");
        
        // Trigger whatever UI panels or particle effects you set up in the Inspector
        onGameOver?.Invoke(); 
        
        // Wait a couple of seconds so the player can see their score, then reload
        Invoke(nameof(ReloadScene), delayBeforeRestart);
    }

    // Switched to 'string' so you can easily call this from UI Buttons
    public void LoadScene(string sceneName) 
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ReloadScene()
    {
        // A cleaner way to reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game");
        Application.Quit();
    }
}