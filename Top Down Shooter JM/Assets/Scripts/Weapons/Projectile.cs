using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public int damage;
    [Tooltip("Check this if the player fires it. Uncheck for enemy projectiles.")]
    public bool isPlayerProjectile = true;
    
    private float deleteTime = 4f;
    private float timer;
    
    // Create a reference to the Rigidbody
    private Rigidbody2D rb; 

    void Awake()
    {
        // Grab the Rigidbody right as the bullet spawns
        rb = GetComponent<Rigidbody2D>(); 
    }

    void Start()
    {
        timer = deleteTime;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // Tell the target they got hit, and whether it was the player who shot them!
            damageable.TakeDamage(damage, isPlayerProjectile); 
        }
        
        Destroy(gameObject);
    }
    
    void OnEnable()
    {
        PlayerMovement.OnWorldShift += ShiftWithTreadmill;
    }

    void OnDisable()
    {
        PlayerMovement.OnWorldShift -= ShiftWithTreadmill;
    }

    void ShiftWithTreadmill(Vector3 shiftAmount)
    {
        // Shift the physics body directly instead of the Transform
        if (rb != null)
        {
            rb.position += (Vector2)shiftAmount;
        }
        else
        {
            // Fallback just in case a bullet is missing its Rigidbody component
            transform.position += shiftAmount;
        }
    }
}