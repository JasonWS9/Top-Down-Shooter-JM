using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

    [Header("Speed & Projectiles")]
    public float speed;
    public float projectileSpeed;
    [SerializeField] public GameObject projectile;
    [SerializeField] public Transform firePoint;

    [Header("X-Axis Parallax Boundaries")]
    [Tooltip("Distance from screen edge where the world starts moving")]
    public float screenMarginX = 2f; 
    [Tooltip("Maximum distance the world can shift to the RIGHT")]
    public float maxWorldX = 20f; 
    [Tooltip("Maximum distance the world can shift to the LEFT")]
    public float minWorldX = -20f;

    // Tracks how far the "treadmill" has rolled horizontally
    public float currentWorldX = 0f; 

    private InputAction moveAction;
    private InputAction shootAction;

    // Events
    public static event Action<Vector3> OnWorldShift;
    public static event Action onFireProjectile;

    void Start()
    {
        Instance = this;
        moveAction = InputSystem.actions.FindAction("Move");
        shootAction = InputSystem.actions.FindAction("Shoot");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y);
        transform.up = direction;

        if (shootAction.WasPressedThisFrame())
        {
            FireProjectile();
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        if (moveInput.magnitude > 0.01f && AiUpdater.Instance != null)
        {
            AiUpdater.Instance.PlayerMoving();
        }

        // Switched to fixedDeltaTime since this runs in FixedUpdate
        Vector3 intendedMove = new Vector3(moveInput.x, moveInput.y, 0) * speed * Time.fixedDeltaTime;
        Vector3 newPlayerPos = transform.position + intendedMove;
        Vector3 worldShiftAmount = Vector3.zero;

        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        // ==========================================
        // X-AXIS: TREADMILL WITH FALL-AWAY MARGIN
        // ==========================================
        float rightMargin = cam.transform.position.x + camWidth - screenMarginX;
        float leftMargin = cam.transform.position.x - camWidth + screenMarginX;

        // Moving Right
        if (newPlayerPos.x > rightMargin)
        {
            float push = newPlayerPos.x - rightMargin;
            float availableShift = maxWorldX - currentWorldX;

            if (availableShift > 0)
            {
                float actualShift = Mathf.Min(push, availableShift);
                worldShiftAmount.x = actualShift;
                currentWorldX += actualShift;
                newPlayerPos.x = rightMargin + (push - actualShift); 
            }
        }
        // Moving Left
        else if (newPlayerPos.x < leftMargin)
        {
            float push = newPlayerPos.x - leftMargin; 
            float availableShift = minWorldX - currentWorldX; 

            if (availableShift < 0)
            {
                float actualShift = Mathf.Max(push, availableShift); 
                worldShiftAmount.x = actualShift;
                currentWorldX += actualShift;
                newPlayerPos.x = leftMargin + (push - actualShift);
            }
        }

        // ==========================================
        // Y-AXIS & HARD CLAMPS
        // ==========================================
        float edgePadding = 0.5f; 
        newPlayerPos.x = Mathf.Clamp(newPlayerPos.x, cam.transform.position.x - camWidth + edgePadding, cam.transform.position.x + camWidth - edgePadding);
        newPlayerPos.y = Mathf.Clamp(newPlayerPos.y, cam.transform.position.y - camHeight + edgePadding, cam.transform.position.y + camHeight - edgePadding);

        // Assign the calculated position (replaces standard Translate logic)
        transform.position = newPlayerPos;

        // ==========================================
        // SHIFT THE WORLD
        // ==========================================
        if (worldShiftAmount != Vector3.zero)
        {
            OnWorldShift?.Invoke(-worldShiftAmount);
        }
    }

    void FireProjectile()
    {
        Debug.Log("firing");
        GameObject bullet = Instantiate(projectile, firePoint.position, firePoint.rotation);

        Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();

        bulletRB.AddForce(firePoint.up * projectileSpeed, ForceMode2D.Impulse);

        if (AiUpdater.Instance != null)
        {
            AiUpdater.Instance.Fire();
        }
        
        onFireProjectile?.Invoke();
    }
}