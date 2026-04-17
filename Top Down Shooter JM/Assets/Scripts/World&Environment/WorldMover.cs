using UnityEngine;

public class WorldMover : MonoBehaviour
{
    void OnEnable()
    {
        // Because OnWorldShift is static, we can subscribe directly to the class
        // without worrying about whether the player Instance has loaded yet!
        PlayerMovement.OnWorldShift += ShiftEnvironment;
    }

    void OnDisable()
    {
        PlayerMovement.OnWorldShift -= ShiftEnvironment;
    }

    void ShiftEnvironment(Vector3 shiftAmount)
    {
        transform.position += shiftAmount;
    }
}