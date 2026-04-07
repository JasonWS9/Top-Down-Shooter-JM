using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class MNPlayerMovement : MonoBehaviour
{
    public static MNPlayerMovement Instance;

    [Header("Speed & Projectiles")]
    public float speed = 5f;
    public float projectileSpeed = 10f;
    [SerializeField] public GameObject projectile;
    [SerializeField] public Transform firePoint;

    [Header("Screen Boundaries")]
    [Tooltip("Distance from the edge of the screen before the camera acts like a treadmill")]
    public float screenMargin = 3f; 
    [Tooltip("Buffer distance to ensure half your ship doesn't clip off-screen")]
    public float spriteBuffer = 0.5f;
    
    [Header("World Limits (Vertical Only)")]
    [Tooltip("The highest the camera can go (Stratosphere)")]
    public float maxWorldY = 20f; 
    [Tooltip("The lowest the camera can go (Planet Surface)")]
    public float minWorldY = -20f;

    private InputAction moveAction;
    private InputAction shootAction;
    public static event Action onFireProjectile;

    void Start()
    {
        Instance = this;
        moveAction = InputSystem.actions.FindAction("Move");
        shootAction = InputSystem.actions.FindAction("Shoot");
    }

    void Update()
    {
        // Aiming
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y);
        transform.up = direction;

        // Shooting
        if (shootAction.WasPressedThisFrame())
        {
            FireProjectile();
        }
        
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        // 1. Send Telemetry if moving
        if (moveInput.magnitude > 0.01f && MNAiUpdater.Instance != null)
        {
            MNAiUpdater.Instance.PlayerMoving();
        }

        // 2. Setup Camera and Movement Math
        Camera mainCam = Camera.main;
        float camHeight = mainCam.orthographicSize;
        float camWidth = camHeight * mainCam.aspect;

        Vector3 currentCamPos = mainCam.transform.position;
        Vector3 currentPlayerPos = transform.position;

        // How much the player wants to move this frame
        Vector3 intendedMove = new Vector3(moveInput.x, moveInput.y, 0) * speed * Time.deltaTime;
        
        // ==========================================
        // X-AXIS: INFINITE HORIZONTAL TREADMILL
        // ==========================================
        float targetPlayerX = currentPlayerPos.x + intendedMove.x;
        float playerOffsetX = targetPlayerX - currentCamPos.x;

        if (playerOffsetX > (camWidth - screenMargin))
        {
            // Pushing the right detector area. Lock player to the margin, move the world.
            currentPlayerPos.x = currentCamPos.x + (camWidth - screenMargin);
            currentCamPos.x += intendedMove.x;
            currentPlayerPos.x += intendedMove.x; // Keep player locked to the moving camera
        }
        else if (playerOffsetX < -(camWidth - screenMargin))
        {
            // Pushing the left detector area.
            currentPlayerPos.x = currentCamPos.x - (camWidth - screenMargin);
            currentCamPos.x += intendedMove.x;
            currentPlayerPos.x += intendedMove.x;
        }
        else
        {
            // Free movement within the safe box
            currentPlayerPos.x += intendedMove.x;
        }

        // ==========================================
        // Y-AXIS: CLAMPED VERTICAL TREADMILL
        // ==========================================
        float targetPlayerY = currentPlayerPos.y + intendedMove.y;
        float playerOffsetY = targetPlayerY - currentCamPos.y;

        if (playerOffsetY > (camHeight - screenMargin))
        {
            // Pushing the top detector area
            if (currentCamPos.y < maxWorldY)
            {
                // Push camera up until it hits the Stratosphere limit
                float pushAmount = Mathf.Min(intendedMove.y, maxWorldY - currentCamPos.y);
                currentCamPos.y += pushAmount;
                currentPlayerPos.y = currentCamPos.y + (camHeight - screenMargin);
                
                // If camera stopped, let the player move the leftover distance toward the edge of the screen
                currentPlayerPos.y += (intendedMove.y - pushAmount);
            }
            else
            {
                // Camera is maxed out. Player just moves freely toward the top of the screen.
                currentPlayerPos.y += intendedMove.y;
            }
        }
        else if (playerOffsetY < -(camHeight - screenMargin))
        {
            // Pushing the bottom detector area
            if (currentCamPos.y > minWorldY)
            {
                // Push camera down until it hits the Surface limit
                float pushAmount = Mathf.Max(intendedMove.y, minWorldY - currentCamPos.y); 
                currentCamPos.y += pushAmount;
                currentPlayerPos.y = currentCamPos.y - (camHeight - screenMargin);

                currentPlayerPos.y += (intendedMove.y - pushAmount);
            }
            else
            {
                currentPlayerPos.y += intendedMove.y;
            }
        }
        else
        {
            currentPlayerPos.y += intendedMove.y;
        }

        // ==========================================
        // THE ULTIMATE SCREEN CLAMP (THE WALL)
        // ==========================================
        // Regardless of all the math above, absolutely guarantee the player's 
        // position can never, ever fly outside the physical camera lens.
        
        currentPlayerPos.x = Mathf.Clamp(currentPlayerPos.x, 
            currentCamPos.x - camWidth + spriteBuffer, 
            currentCamPos.x + camWidth - spriteBuffer);
            
        currentPlayerPos.y = Mathf.Clamp(currentPlayerPos.y, 
            currentCamPos.y - camHeight + spriteBuffer, 
            currentCamPos.y + camHeight - spriteBuffer);

        // 3. Apply the final calculated positions
        mainCam.transform.position = currentCamPos;
        transform.position = currentPlayerPos;
    }

    void FireProjectile()
    {
        if (projectile == null)
        {
            Debug.LogError("Projectile prefab is missing! Assign it in the Inspector.");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("FirePoint is missing! Assign it in the Inspector.");
            return;
        }

        GameObject bullet = Instantiate(projectile, firePoint.position, firePoint.rotation);
        
        Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();
        if (bulletRB != null)
        {
            bulletRB.AddForce(firePoint.up * projectileSpeed, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogError("The bullet prefab needs a Rigidbody2D component!");
        }

        if (MNAiUpdater.Instance != null)
        {
            MNAiUpdater.Instance.Fire();
        }
        
        onFireProjectile?.Invoke();
    }
}