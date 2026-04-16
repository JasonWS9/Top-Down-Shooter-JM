using UnityEngine;
using System; // Required for Action events

public class MNPlayerManager : MonoBehaviour, IDamageable
{
    public static MNPlayerManager Instance;

    public int maxHealth;
    public int currentHealth;

    // 1. Declare the event the UI is listening for
    public static event Action<int, int> OnHealthUpdated;

    void Awake()
    {
        Instance = this;
        currentHealth = maxHealth;
    }

    void Start()
    {
        // 2. Broadcast the starting health to the UI the moment the game loads
        OnHealthUpdated?.Invoke(currentHealth, maxHealth);
    }

    void Update()
    {
        if (currentHealth <= 0)
        {
            PlayerDeath();
        }
    }

    // 3. Updated to match the new IDamageable contract!
    public void TakeDamage(int damage, bool fromPlayer = false)
    {
        currentHealth -= damage;
        
        // 4. Broadcast the new health to the UI every time the player gets hit
        OnHealthUpdated?.Invoke(currentHealth, maxHealth);
    }

    public Vector2 GetPlayerViewportPosition()
    {
        return Camera.main.WorldToViewportPoint(transform.position);
    }

    void PlayerDeath()
    {
        Debug.Log("Player is Dead");
    }
}