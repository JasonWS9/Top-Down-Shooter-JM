using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class MNPlayerMovement : MonoBehaviour
{
    public static MNPlayerMovement Instance;

    [Header("Speed & Projectiles")]
    public float speed = 5f;
    public float projectileSpeed = 10f;
    public GameObject projectile;
    public Transform firePoint;

    [Header("Treadmill Boundaries")]
    [Tooltip("Keep this small, like 2 or 3")]
    public float screenMargin = 2f; 
    public float maxWorldY = 20f; 
    public float minWorldY = -20f;

    // Tracks how far the "treadmill" has rolled vertically to know when we hit the stratosphere/surface
    public float currentTreadmillY = 0f; 

    private InputAction moveAction;
    private InputAction shootAction;
    
    // THE MAGIC EVENT: Tells the rest of the game to move backward
    public static event Action<Vector3> OnWorldShift;
    public static event Action onFireProjectile;

    void Start()
    {
        Instance = this;
        moveAction = InputSystem.actions.FindAction("Move");
        shootAction = InputSystem.actions.FindAction("Shoot");
    }

    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.up = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y);

        if (shootAction.WasPressedThisFrame()) FireProjectile();
        
        HandleTreadmillMovement();
    }

    void HandleTreadmillMovement()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        if (moveInput.magnitude > 0.01f && MNAiUpdater.Instance != null)
        {
            MNAiUpdater.Instance.PlayerMoving();
        }

        Vector3 intendedMove = new Vector3(moveInput.x, moveInput.y, 0) * speed * Time.deltaTime;
        Vector3 newPlayerPos = transform.position + intendedMove;
        Vector3 worldShiftAmount = Vector3.zero; // How much the universe needs to move

        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        // ==========================================
        // X-AXIS: INFINITE TREADMILL
        // ==========================================
        float rightBound = cam.transform.position.x + camWidth - screenMargin;
        float leftBound = cam.transform.position.x - camWidth + screenMargin;

        if (newPlayerPos.x > rightBound)
        {
            worldShiftAmount.x = newPlayerPos.x - rightBound; // Measure the overlap
            newPlayerPos.x = rightBound; // Lock player to the margin
        }
        else if (newPlayerPos.x < leftBound)
        {
            worldShiftAmount.x = newPlayerPos.x - leftBound;
            newPlayerPos.x = leftBound;
        }

        // ==========================================
        // Y-AXIS: CLAMPED TREADMILL
        // ==========================================
        float topBound = cam.transform.position.y + camHeight - screenMargin;
        float bottomBound = cam.transform.position.y - camHeight + screenMargin;

        if (newPlayerPos.y > topBound)
        {
            float push = newPlayerPos.y - topBound;
            if (currentTreadmillY + push < maxWorldY)
            {
                worldShiftAmount.y = push;
                newPlayerPos.y = topBound;
                currentTreadmillY += push; // Track how high we've climbed
            }
        }
        else if (newPlayerPos.y < bottomBound)
        {
            float push = newPlayerPos.y - bottomBound;
            if (currentTreadmillY + push > minWorldY)
            {
                worldShiftAmount.y = push;
                newPlayerPos.y = bottomBound;
                currentTreadmillY += push; // Track how low we've descended
            }
        }

        // Apply Player Position. Hard clamp to ensure they never visually pop off screen.
        newPlayerPos.x = Mathf.Clamp(newPlayerPos.x, cam.transform.position.x - camWidth + 0.5f, cam.transform.position.x + camWidth - 0.5f);
        newPlayerPos.y = Mathf.Clamp(newPlayerPos.y, cam.transform.position.y - camHeight + 0.5f, cam.transform.position.y + camHeight - 0.5f);
        transform.position = newPlayerPos;

        // ==========================================
        // SHIFT THE WORLD
        // ==========================================
        if (worldShiftAmount != Vector3.zero)
        {
            // We send the NEGATIVE shift amount, so the world moves opposite to the player
            OnWorldShift?.Invoke(-worldShiftAmount);
        }
    }

    void FireProjectile()
    {
        if (projectile != null && firePoint != null)
        {
            GameObject bullet = Instantiate(projectile, firePoint.position, firePoint.rotation);
            bullet.GetComponent<Rigidbody2D>().AddForce(firePoint.up * projectileSpeed, ForceMode2D.Impulse);
            if (MNAiUpdater.Instance != null) MNAiUpdater.Instance.Fire();
            onFireProjectile?.Invoke();
        }
    }
}