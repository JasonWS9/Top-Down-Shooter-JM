using UnityEngine;

public class ShieldVisual : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("How fast the shield spins. Use a negative number to spin the other way!")]
    public float rotationSpeed = 50f;
    
    [Tooltip("How fast it throbs in and out.")]
    public float pulseSpeed = 3f;
    
    [Tooltip("How much larger/smaller it gets during the pulse.")]
    public float pulseAmount = 0.05f;

    private Vector3 baseScale;

    void Start()
    {
        // Remember the original size you set in the Inspector
        baseScale = transform.localScale;
    }

    void Update()
    {
        // 1. Slowly rotate the shield on the Z-axis
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        // 2. Make it pulse slightly bigger and smaller using a Sine wave
        float scaleOffset = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale + new Vector3(scaleOffset, scaleOffset, 0f);
    }
}