using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

    [Header("Cursor Settings")]
    [Tooltip("Drag your crosshair image here! Must be imported as 'Cursor' texture type.")]
    public Texture2D crosshairTexture;

    [Header("Speed & Projectiles")]
    public float speed;
    public float projectileSpeed;
    [SerializeField] public GameObject projectile;
    [SerializeField] public Transform firePoint;

    [Header("Weapon Upgrades (Geometry Wars Style)")]
    [Range(1, 4)] public int weaponLevel = 1;
    public float fireRate = 0.15f; // How fast they shoot when holding the button
    private float fireTimer = 0f;

    [Header("Leveling Settings")]
    public float speedGrowthFactor = 0.15f; 
    public float damageGrowthFactor = 0.25f;
    private float baseSpeed;
    private float baseProjectileSpeed;

    [Header("RailGun Power-Up")]
    public float railGunAmmo = 0f;
    private float forcedFireTimer = 0f; 
    public GameObject railGunBeamVisual; 

    [Header("X-Axis Parallax Boundaries")]
    public float screenMarginX = 2f; 
    public float maxWorldX = 20f; 
    public float minWorldX = -20f;
    public float currentWorldX = 0f; 

    private InputAction moveAction;
    private InputAction shootAction;
    private Vector2 lastMoveDirection;

    public static event Action<Vector3> OnWorldShift;
    public static event Action onFireProjectile;

    void Start()
    {
        Instance = this;
        moveAction = InputSystem.actions.FindAction("Move");
        shootAction = InputSystem.actions.FindAction("Shoot");

        baseSpeed = speed;
        baseProjectileSpeed = projectileSpeed;

        SetCustomCursor();
    }

    void OnEnable()
    {
        GameManager.OnPlayerLevelUp += HandleLevelUp;
    }

    void OnDisable()
    {
        GameManager.OnPlayerLevelUp -= HandleLevelUp;
    }

    void SetCustomCursor()
    {
        if (crosshairTexture != null)
        {
            // Center the "click point" in the exact middle of your image
            Vector2 hotspot = new Vector2(crosshairTexture.width / 2f, crosshairTexture.height / 2f);
            Cursor.SetCursor(crosshairTexture, hotspot, CursorMode.Auto);
        }
        
        // Locks the cursor inside the game window so you don't accidentally click off-screen!
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y);
        transform.up = direction;

        // Tick down the normal fire timer
        if (fireTimer > 0f) fireTimer -= Time.deltaTime;

        // FIRING LOGIC ROUTER
        if (railGunAmmo > 0f)
        {
            HandleRailGunFiring();
        }
        else 
        {
            HandleNormalFiring();
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleNormalFiring()
    {
        // Continuous auto-fire while holding the button
        if (shootAction.IsPressed() && fireTimer <= 0f)
        {
            if (GameManager.Instance != null) GameManager.Instance.RegisterAction();
            
            FireWeaponLevel();
            
            // Reset the timer based on our fire rate
            fireTimer = fireRate;
        }
    }

    void FireWeaponLevel()
    {
        onFireProjectile?.Invoke();
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.playerShootSound, 0.5f);

        // Geometry Wars Style Upgrades!
        switch (weaponLevel)
        {
            case 1:
                // Level 1: Standard Single Shot
                SpawnBullet(firePoint.position, firePoint.rotation);
                break;

            case 2:
                // Level 2: Twin Parallel Shot (Two bullets side-by-side)
                SpawnBullet(firePoint.position + firePoint.right * 0.2f, firePoint.rotation);
                SpawnBullet(firePoint.position - firePoint.right * 0.2f, firePoint.rotation);
                break;

            case 3:
                // Level 3: Triple Spread (Forward, angled left, angled right)
                SpawnBullet(firePoint.position, firePoint.rotation);
                SpawnBullet(firePoint.position, firePoint.rotation * Quaternion.Euler(0, 0, 15f));
                SpawnBullet(firePoint.position, firePoint.rotation * Quaternion.Euler(0, 0, -15f));
                break;

            case 4:
                // Level 4: Quad Spread (Triple spread PLUS one shooting directly behind you!)
                SpawnBullet(firePoint.position + firePoint.right * 0.1f, firePoint.rotation);
                SpawnBullet(firePoint.position - firePoint.right * 0.1f, firePoint.rotation);
                SpawnBullet(firePoint.position, firePoint.rotation * Quaternion.Euler(0, 0, 25f));
                SpawnBullet(firePoint.position, firePoint.rotation * Quaternion.Euler(0, 0, -25f));
                SpawnBullet(firePoint.position, firePoint.rotation * Quaternion.Euler(0, 0, 180f)); // Covers your back!
                break;
        }
    }

    void SpawnBullet(Vector3 position, Quaternion rotation)
    {
        GameObject bullet = Instantiate(projectile, position, rotation);
        Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();
        if (bulletRB != null)
        {
            bulletRB.AddForce(bullet.transform.up * projectileSpeed, ForceMode2D.Impulse);
        }
    }

    void HandleRailGunFiring()
    {
        if (shootAction.WasPressedThisFrame())
        {
            railGunAmmo -= 0.5f;
            forcedFireTimer = 0.5f;
            if (railGunBeamVisual != null) railGunBeamVisual.SetActive(true);
            if (GameManager.Instance != null) GameManager.Instance.RegisterAction();
        }
        else if (shootAction.IsPressed())
        {
            if (forcedFireTimer <= 0f) railGunAmmo -= Time.deltaTime;
        }
        else
        {
            if (forcedFireTimer <= 0f && railGunBeamVisual != null) railGunBeamVisual.SetActive(false);
        }

        if (forcedFireTimer > 0f) forcedFireTimer -= Time.deltaTime;

        if (railGunAmmo <= 0f && forcedFireTimer <= 0f)
        {
            railGunAmmo = 0f;
            if (railGunBeamVisual != null) railGunBeamVisual.SetActive(false);
        }
    }

    void HandleMovement()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

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

        float edgePadding = 0.5f; 
        newPlayerPos.x = Mathf.Clamp(newPlayerPos.x, cam.transform.position.x - camWidth + edgePadding, cam.transform.position.x + camWidth - edgePadding);
        newPlayerPos.y = Mathf.Clamp(newPlayerPos.y, cam.transform.position.y - camHeight + edgePadding, cam.transform.position.y + camHeight - edgePadding);

        transform.position = newPlayerPos;

        if (worldShiftAmount != Vector3.zero)
        {
            OnWorldShift?.Invoke(-worldShiftAmount);
        }
    }

    void HandleLevelUp(int newLevel)
    {
        float speedMultiplier = 1f + (speedGrowthFactor * Mathf.Log(newLevel));
        speed = baseSpeed * speedMultiplier;
        projectileSpeed = baseProjectileSpeed * speedMultiplier;

        // Optional: Automatically upgrade the weapon level as the player levels up!
        if (newLevel >= 5) weaponLevel = 2;
        if (newLevel >= 10) weaponLevel = 3;
        if (newLevel >= 15) weaponLevel = 4;
    }

    public float GetDamageMultiplier()
    {
        if (GameManager.Instance == null) return 1f;
        return 1f + (damageGrowthFactor * Mathf.Log(GameManager.Instance.playerLevel));
    }

    public void ActivateRailGun()
    {
        railGunAmmo = 2f; 
        Debug.Log("RAILGUN ARMED!");
    }
}