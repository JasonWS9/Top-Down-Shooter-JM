using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MNParallaxBackground : MonoBehaviour
{
    [Range(0f, 1f)]
    public float parallaxFactor;

    private float singleImageLength; // The width of ONE image
    private float startPosX;

    void Start()
    {
        startPosX = transform.position.x;

        // Measure the width of the original sprite (e.g., 18.78)
        singleImageLength = GetComponent<SpriteRenderer>().sprite.bounds.size.x * transform.localScale.x;
    }

    void OnEnable()
    {
        MNPlayerMovement.OnWorldShift += ShiftBackground;
    }

    void OnDisable()
    {
        MNPlayerMovement.OnWorldShift -= ShiftBackground;
    }

    void ShiftBackground(Vector3 shiftAmount)
    {
        float newX = transform.position.x + (shiftAmount.x * parallaxFactor);
        float newY = transform.position.y + shiftAmount.y;
        transform.position = new Vector3(newX, newY, transform.position.z);
    }

    void LateUpdate()
    {
        float distFromStart = transform.position.x - startPosX;

        // Because we have two images side-by-side, we teleport when we've moved the length of ONE image.
        // This makes the second image slide into the first image's original position!
        if (distFromStart < -singleImageLength)
        {
            transform.position = new Vector3(transform.position.x + singleImageLength, transform.position.y, transform.position.z);
        }
        else if (distFromStart > singleImageLength)
        {
            transform.position = new Vector3(transform.position.x - singleImageLength, transform.position.y, transform.position.z);
        }
    }
}