using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float deleteTime = 4f;
    private float timer;

    public int damage;
    
    // 1. Create a reference to the Rigidbody
    private Rigidbody2D rb; 

    void Awake()
    {
        // 2. Grab the Rigidbody right as the bullet spawns
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
            damageable.TakeDamage(damage);
        }
    }
    
    void OnEnable()
    {
        MNPlayerMovement.OnWorldShift += ShiftWithTreadmill;
    }

    void OnDisable()
    {
        MNPlayerMovement.OnWorldShift -= ShiftWithTreadmill;
    }

    void ShiftWithTreadmill(Vector3 shiftAmount)
    {
        // 3. Shift the physics body directly instead of the Transform
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