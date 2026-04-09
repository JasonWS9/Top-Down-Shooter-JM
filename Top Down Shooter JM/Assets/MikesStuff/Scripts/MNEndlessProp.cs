using UnityEngine;

public class MNEndlessProp : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("1 = Moves at normal speed. < 1 = Parallax (moves slower)")]
    public float parallaxFactor = 1f;

    [Header("Wrap Settings")]
    [Tooltip("How far past the camera edge should it go before teleporting? (e.g., The extra 1.5 units to reach 14)")]
    public float offScreenBuffer = 2f; 

    private Camera mainCam;
    private float wrapLimit; // The +/- 14 in your example

    void Start()
    {
        mainCam = Camera.main;
        
        // Calculate half the camera's width (e.g., the 12.5)
        float halfCamWidth = mainCam.orthographicSize * mainCam.aspect;
        
        // Add your buffer to get the exact teleport point (e.g., 12.5 + 1.5 = 14)
        wrapLimit = halfCamWidth + offScreenBuffer;
    }

    void OnEnable()
    {
        MNPlayerMovement.OnWorldShift += ShiftProp;
    }

    void OnDisable()
    {
        MNPlayerMovement.OnWorldShift -= ShiftProp;
    }

    void ShiftProp(Vector3 shiftAmount)
    {
        // Move the building along the treadmill
        float newX = transform.position.x + (shiftAmount.x * parallaxFactor);
        float newY = transform.position.y + shiftAmount.y;
        transform.position = new Vector3(newX, newY, transform.position.z);
    }

    void LateUpdate()
    {
        // Find exactly how far this building is from the center of the camera
        float distFromCam = transform.position.x - mainCam.transform.position.x;

        // If the building is too far LEFT (-14)
        if (distFromCam < -wrapLimit)
        {
            // Teleport it to the far RIGHT (+14)
            transform.position += new Vector3(wrapLimit * 2f, 0, 0);
        }
        // If the building is too far RIGHT (+14)
        else if (distFromCam > wrapLimit)
        {
            // Teleport it to the far LEFT (-14)
            transform.position -= new Vector3(wrapLimit * 2f, 0, 0);
        }
    }
}