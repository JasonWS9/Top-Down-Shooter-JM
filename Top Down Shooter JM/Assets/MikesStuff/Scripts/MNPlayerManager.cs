using UnityEngine;

public class MNPlayerManager : MonoBehaviour, IDamageable
{
    public static MNPlayerManager Instance;

    public int maxHealth;
    public int currentHealth;

    void Awake()
    {
        Instance = this;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (currentHealth <= 0)
        {
            PlayerDeath();
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
    }

    // You can call this from your AI Director when needed, rather than every frame
    public Vector2 GetPlayerViewportPosition()
    {
        return Camera.main.WorldToViewportPoint(transform.position);
    }

    void PlayerDeath()
    {
        Debug.Log("Player is Dead");
    }
}