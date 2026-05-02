using UnityEngine;

public enum PowerUpType { Health, Shields, ScoreMultiplier, GodMode, RailGun }

public class PowerUp : MonoBehaviour
{
    public PowerUpType myType;
    public float lifetime = 10f; 

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ApplyPowerUp();
            Destroy(gameObject); 
        }
    }

    void ApplyPowerUp()
    {
        switch (myType)
        {
            case PowerUpType.Health:
                if (PlayerManager.Instance != null)
                {
                    int healAmount = Mathf.RoundToInt(PlayerManager.Instance.maxHealth * 0.2f);
                    PlayerManager.Instance.TakeDamage(-healAmount); 
                }
                break;

            case PowerUpType.Shields:
                if (PlayerManager.Instance != null) PlayerManager.Instance.ActivateShield();
                break;

            case PowerUpType.ScoreMultiplier:
                if (GameManager.Instance != null) GameManager.Instance.ActivateScoreMultiplier(10f); // 10 seconds of Double Score!
                break;

            case PowerUpType.GodMode:
                if (PlayerManager.Instance != null) PlayerManager.Instance.ActivateGodMode(3f);
                break;

            case PowerUpType.RailGun:
                if (PlayerMovement.Instance != null) PlayerMovement.Instance.ActivateRailGun();
                break;
        }
    }
}