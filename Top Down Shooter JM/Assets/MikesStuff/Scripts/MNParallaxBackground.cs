using UnityEngine;

public class MNParallaxBackground : MonoBehaviour
{
    [Header("Parallax Settings")]
    [Tooltip("0 = Moves with camera (Foreground). 1 = Static (Deep Background).")]
    [Range(0f, 1f)]
    public float parallaxFactor;

    [Header("Cluster Settings")]
    [Tooltip("Check this if this object is an empty parent with multiple child sprites.")]
    public bool isCluster = false;
    
    [Tooltip("If it's a cluster, how wide is this 'chunk' of space?")]
    public float clusterWidth = 30f; 

    private float length;
    private float startPosX;
    private Transform mainCam;

    void Start()
    {
        mainCam = Camera.main.transform;
        startPosX = transform.position.x;
        
        // Determine the length based on what type of object this is
        if (isCluster)
        {
            length = clusterWidth;
        }
        else
        {
            // Fallback to the automatic method if it's just a single sprite
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                length = sr.bounds.size.x;
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] Missing SpriteRenderer! Check 'Is Cluster' or add a SpriteRenderer.");
            }
        }
    }

    void Update()
    {
        float temp = (mainCam.position.x * (1 - parallaxFactor));
        float dist = (mainCam.position.x * parallaxFactor);

        transform.position = new Vector3(startPosX + dist, transform.position.y, transform.position.z);

        if (temp > startPosX + length)
        {
            startPosX += length;
        }
        else if (temp < startPosX - length)
        {
            startPosX -= length;
        }
    }
}