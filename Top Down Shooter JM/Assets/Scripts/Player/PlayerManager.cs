using UnityEngine;
using System; // Required for the Action event

public class PlayerManager : MonoBehaviour, IDamageable
{

    public static PlayerManager Instance;

    public int maxHealth;
    public int currentHealth;

    // The event the UI is listening for
    public static event Action<int, int> OnHealthUpdated;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
        currentHealth = maxHealth;
    }

    void Start()
    {
        // Broadcast the starting health to the UI the moment the game loads
        OnHealthUpdated?.Invoke(currentHealth, maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        TrackRegion();

        if (currentHealth <= 0)
        {
            PlayerDeath();
        }
    }

    public void TakeDamage(int damage, bool fromPlayer = false)
    {
        currentHealth -= damage;
        
        // Broadcast the new health to the UI every time the player gets hit
        OnHealthUpdated?.Invoke(currentHealth, maxHealth);
    }

    void TrackRegion()
    {
        //Checks the player's position in the viewport (0 to 1)
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);

        //Finds the player's distance from the center (0 to 0.5)
        float distanceFromCenterX = Mathf.Abs(viewportPos.x - 0.5f);
        float distanceFromCenterY = Mathf.Abs(viewportPos.y - 0.5f);

        //Threshold for the player to be considered outside the middle (inner 10% of screen per 0.05)
        float distanceThreshold = 0.2f; //0.2f is the inner 40% of the screen

        if (distanceFromCenterX < distanceThreshold && distanceFromCenterY < distanceThreshold)
        {
            Debug.Log("player is near the middle");
            return;
        }

        //If the player isnt in the center checks which region they are in
        if (viewportPos.x > 0.5)
        {
            if (viewportPos.y > 0.5)
            {
                Debug.Log("player is in top right");
            }
            else
            {
                Debug.Log("player is in bottom right");
            }
        }
        else
        {
            if (viewportPos.y > 0.5)
            {
                Debug.Log("player is in top left");
            }
            else
            {
                Debug.Log("player is in bottom left");
            }
        }
    }
    void PlayerDeath()
    {
        Debug.Log("Dead");
    }
}