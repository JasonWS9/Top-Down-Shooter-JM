using UnityEngine;

public class Projectile : MonoBehaviour
{

    private float deleteTime = 4f;
    private float timer;

    public int damage;

    void Start()
    {
        timer = deleteTime;
    }

    // Update is called once per frame
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
}
