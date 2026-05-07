using UnityEngine;

public enum PowerUpType { Health, Shields, ScoreMultiplier, GodMode, RailGun }

public class PowerUp : MonoBehaviour
{
    [Header("Power-Up Settings")]
    public PowerUpType myType;
    public float lifetime = 10f; 

    [Header("Treadmill Settings")]
    [Tooltip("Set this to match your scrolling background speed! (e.g., X: 0, Y: -2)")]
    public Vector2 driftSpeed = Vector2.zero;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // --- NEW: Subscribe to the world teleport! ---
    void OnEnable()
    {
        PlayerMovement.OnWorldShift += ShiftWithWorld;
    }

    void OnDisable()
    {
        PlayerMovement.OnWorldShift -= ShiftWithWorld;
    }

    void ShiftWithWorld(Vector3 shiftAmount)
    {
        transform.position += shiftAmount;
    }
    // ---------------------------------------------

    // --- NEW: Continuously drift backward with the background! ---
    void Update()
    {
        transform.position += (Vector3)driftSpeed * Time.deltaTime;
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
                if (GameManager.Instance != null) GameManager.Instance.ActivateScoreMultiplier(10f);
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