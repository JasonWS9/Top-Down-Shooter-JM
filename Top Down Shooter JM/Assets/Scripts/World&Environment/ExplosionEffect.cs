using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [Tooltip("How long does the animation take to finish?")]
    public float lifetime = 0.5f;

    void Start()
    {
        // Automatically delete this object after the animation finishes
        Destroy(gameObject, lifetime);
    }
    
    // Subscribe to the treadmill shift so explosions drift with the background!
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
}