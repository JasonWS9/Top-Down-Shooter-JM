using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    // Adjust this in the Inspector to match how long your animation is
    public float lifetime = 0.5f; 

    void Start()
    {
        // This destroys the GameObject this script is attached to 
        // after 'lifetime' seconds have passed.
        Destroy(gameObject, lifetime);
    }
}