using UnityEngine;

public class MNWorldObject : MonoBehaviour
{
    [Header("Depth Settings")]
    [Tooltip("1.0 = Regular physical object. 0.5 = Moves half as fast (midground). 0.1 = Barely moves (deep background).")]
    [Range(0f, 1f)]
    public float parallaxFactor = 1f;

    void OnEnable()
    {
        // Hook this object onto the treadmill
        MNPlayerMovement.OnWorldShift += ShiftWithTreadmill;
    }

    void OnDisable()
    {
        // Unhook it if it gets destroyed
        MNPlayerMovement.OnWorldShift -= ShiftWithTreadmill;
    }

    void ShiftWithTreadmill(Vector3 shiftAmount)
    {
        // X-AXIS: Parallax shift. (Smaller number = moves slower = looks further away)
        float newX = transform.position.x + (shiftAmount.x * parallaxFactor);
        
        // Y-AXIS: 1:1 shift. It always moves vertically exactly with the world.
        float newY = transform.position.y + shiftAmount.y;

        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}