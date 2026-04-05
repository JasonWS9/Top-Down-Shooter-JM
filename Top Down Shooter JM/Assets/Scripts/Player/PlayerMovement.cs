using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

    public float speed;

    public float projectileSpeed;

    private InputAction moveAction;
    private InputAction shootAction;

    [SerializeField] public GameObject projectile;
    [SerializeField] public Transform firePoint;

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

        if (moveInput.magnitude > 0.01)
        {
            AiUpdater.Instance.PlayerMoving();
        }

        float moveX = moveInput.x;
        float moveY = moveInput.y;

        Vector2 finalMove = new Vector2(moveX, moveY) * speed * Time.deltaTime;
        transform.Translate(finalMove, Space.World);

    }



    void FireProjectile()
    {
        Debug.Log("firing");
        GameObject bullet = Instantiate(projectile, firePoint.position, firePoint.rotation);

        Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();

        bulletRB.AddForce(firePoint.up * projectileSpeed, ForceMode2D.Impulse);

        AiUpdater.Instance.Fire();
    }
}
