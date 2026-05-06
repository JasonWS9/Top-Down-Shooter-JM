using UnityEngine;

public class MovementTrail : MonoBehaviour
{
    [Header("Trail Settings")]
    public SpriteRenderer targetSpriteRenderer; 

    public float timeBetweenSpawns = 0.05f;
    public float ghostStartingAlpha = 0.4f;
    public float fadeSpeed = 3f;

    [Header("Treadmill / Idle Settings")]
    [Tooltip("Check this so the player constantly drops a trail even when you aren't pressing buttons!")]
    public bool forceSpawnWhenIdle = false;
    
    [Tooltip("If your background moves automatically, add that speed here so the ghosts drift backwards with the space dust!")]
    public Vector2 backgroundDriftSpeed = Vector2.zero; 

    private Vector3 lastPosition;
    private float spawnTimer;

    void Start()
    {
        if (targetSpriteRenderer == null)
        {
            targetSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        lastPosition = transform.position;
    }

    void LateUpdate() 
    {
        if (targetSpriteRenderer == null || !targetSpriteRenderer.enabled) return;

        bool isMoving = Vector3.Distance(transform.position, lastPosition) > 0.001f;

        // FIXED: Now we check if they are moving OR if we forced the trail to always spawn!
        if (isMoving || forceSpawnWhenIdle)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnGhost();
                spawnTimer = timeBetweenSpawns;
            }
        }
        else
        {
            spawnTimer = 0f;
        }
        
        lastPosition = transform.position;
    }

    void SpawnGhost()
    {
        GameObject ghostObj = new GameObject(gameObject.name + "_Ghost");
        
        ghostObj.transform.position = targetSpriteRenderer.transform.position;
        ghostObj.transform.rotation = targetSpriteRenderer.transform.rotation;
        ghostObj.transform.localScale = targetSpriteRenderer.transform.lossyScale;

        SpriteRenderer ghostSprite = ghostObj.AddComponent<SpriteRenderer>();
        ghostSprite.sprite = targetSpriteRenderer.sprite;
        
        // FIXED: Copy the Flip checkboxes!
        ghostSprite.flipX = targetSpriteRenderer.flipX;
        ghostSprite.flipY = targetSpriteRenderer.flipY;
        
        ghostSprite.color = new Color(targetSpriteRenderer.color.r, targetSpriteRenderer.color.g, targetSpriteRenderer.color.b, ghostStartingAlpha);
        
        ghostSprite.sortingLayerID = targetSpriteRenderer.sortingLayerID;
        ghostSprite.sortingOrder = targetSpriteRenderer.sortingOrder - 1; 

        GhostFader fader = ghostObj.AddComponent<GhostFader>();
        
        // Pass the new drift speed to the fader!
        fader.Initialize(fadeSpeed, backgroundDriftSpeed);
    }
}

// ==========================================
// GHOST FADER (Handles Fade, World Shift, and Drift)
// ==========================================
public class GhostFader : MonoBehaviour
{
    private SpriteRenderer rend;
    private float speed;
    private Vector2 drift;

    public void Initialize(float fadeSpeed, Vector2 driftSpeed)
    {
        rend = GetComponent<SpriteRenderer>();
        speed = fadeSpeed;
        drift = driftSpeed; // Store the treadmill drift
    }

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

    void Update()
    {
        if (rend == null) return;

        // Apply the continuous background drift!
        transform.position += (Vector3)drift * Time.deltaTime;

        Color c = rend.color;
        c.a -= speed * Time.deltaTime;
        rend.color = c;

        if (c.a <= 0f)
        {
            Destroy(gameObject);
        }
    }
}