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

    [Header("Leveling Settings")]
    public float speedGrowthFactor = 0.15f; 
    public float damageGrowthFactor = 0.25f;
    private float baseSpeed;
    private float baseProjectileSpeed;

    [Header("X-Axis Parallax Boundaries")]
    public float screenMarginX = 2f; 
    public float maxWorldX = 20f; 
    public float minWorldX = -20f;
    public float currentWorldX = 0f;
    
    [Header("RailGun Power-Up")]
    public float railGunAmmo = 0f;
    private float forcedFireTimer = 0f; // Keeps the laser on for the minimum 0.5s per click
    public GameObject railGunBeamVisual; // Drag your laser child object here

    // Input Actions & APM Tracking
    private InputAction moveAction;
    private InputAction shootAction;
    private Vector2 lastMoveDirection;

    // Events
    public static event Action<Vector3> OnWorldShift;
    public static event Action onFireProjectile;

    void Start()
    {
        Instance = this;
        moveAction = InputSystem.actions.FindAction("Move");
        shootAction = InputSystem.actions.FindAction("Shoot");

        baseSpeed = speed;
        baseProjectileSpeed = projectileSpeed;
    }

    void OnEnable()
    {
        GameManager.OnPlayerLevelUp += HandleLevelUp;
    }

    void OnDisable()
    {
        GameManager.OnPlayerLevelUp -= HandleLevelUp;
    }

    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y);
        transform.up = direction;

        // RAILGUN FIRING LOGIC
        if (railGunAmmo > 0f)
        {
            // Initial click: Instantly deduct 0.5 ammo and force the beam on for 0.5 seconds
            if (shootAction.WasPressedThisFrame())
            {
                railGunAmmo -= 0.5f;
                forcedFireTimer = 0.5f;
                if (railGunBeamVisual != null) railGunBeamVisual.SetActive(true);
                if (GameManager.Instance != null) GameManager.Instance.RegisterAction();
            }
            // Continuous holding: Once the initial 0.5s penalty is over, drain smoothly
            else if (shootAction.IsPressed())
            {
                if (forcedFireTimer <= 0f) railGunAmmo -= Time.deltaTime;
            }
            // Released: Turn off the beam once the 0.5s penalty finishes
            else
            {
                if (forcedFireTimer <= 0f && railGunBeamVisual != null) railGunBeamVisual.SetActive(false);
            }

            // Tick down the forced fire timer
            if (forcedFireTimer > 0f) forcedFireTimer -= Time.deltaTime;

            // Turn off RailGun when completely empty
            if (railGunAmmo <= 0f && forcedFireTimer <= 0f)
            {
                railGunAmmo = 0f;
                if (railGunBeamVisual != null) railGunBeamVisual.SetActive(false);
            }
        }
        else 
        {
            // NORMAL FIRING LOGIC
            if (shootAction.WasPressedThisFrame()) FireProjectile();
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        // [DDA] Check if the player is actively changing directions (weaving/dodging)
        if (moveInput.magnitude > 0.1f && Vector2.Distance(moveInput, lastMoveDirection) > 0.5f)
        {
            lastMoveDirection = moveInput;
            if (GameManager.Instance != null) GameManager.Instance.RegisterAction();
        }

        Vector3 intendedMove = new Vector3(moveInput.x, moveInput.y, 0) * speed * Time.fixedDeltaTime;
        Vector3 newPlayerPos = transform.position + intendedMove;
        Vector3 worldShiftAmount = Vector3.zero;

        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        // X-AXIS: TREADMILL WITH FALL-AWAY MARGIN
        float rightMargin = cam.transform.position.x + camWidth - screenMarginX;
        float leftMargin = cam.transform.position.x - camWidth + screenMarginX;

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

        // Y-AXIS & HARD CLAMPS
        float edgePadding = 0.5f; 
        newPlayerPos.x = Mathf.Clamp(newPlayerPos.x, cam.transform.position.x - camWidth + edgePadding, cam.transform.position.x + camWidth - edgePadding);
        newPlayerPos.y = Mathf.Clamp(newPlayerPos.y, cam.transform.position.y - camHeight + edgePadding, cam.transform.position.y + camHeight - edgePadding);

        transform.position = newPlayerPos;

        if (worldShiftAmount != Vector3.zero)
        {
            OnWorldShift?.Invoke(-worldShiftAmount);
        }
    }

    void FireProjectile()
    {
        if (GameManager.Instance != null) GameManager.Instance.RegisterAction();

        GameObject bullet = Instantiate(projectile, firePoint.position, firePoint.rotation);
        Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();
        bulletRB.AddForce(firePoint.up * projectileSpeed, ForceMode2D.Impulse);
        
        onFireProjectile?.Invoke();
    }

    void HandleLevelUp(int newLevel)
    {
        float speedMultiplier = 1f + (speedGrowthFactor * Mathf.Log(newLevel));
        speed = baseSpeed * speedMultiplier;
        projectileSpeed = baseProjectileSpeed * speedMultiplier;
    }
    
    public void ActivateRailGun()
    {
        railGunAmmo = 2f; // Gives exactly 2 seconds of total fire time
        Debug.Log("RAILGUN ARMED!");
    }

    public float GetDamageMultiplier()
    {
        if (GameManager.Instance == null) return 1f;
        return 1f + (damageGrowthFactor * Mathf.Log(GameManager.Instance.playerLevel));
    }
}