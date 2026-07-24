using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 24f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Combat")]
    [SerializeField] private PlayerWeaponSO equippedWeapon;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] public Transform weaponSpawnPoint;

    private CharacterController controller;
    private PlayerInput playerInput;
    private Transform cameraTransform;

    private Vector3 verticalVelocity;
    private Vector3 currentHorizontalVelocity;

    // Dash State
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector3 dashDirection;

    // Combat State
    private bool isAutoFireActive = false;
    private float currentWeaponCooldown;
    private Transform currentTarget;

    public Vector3 Velocity => verticalVelocity + currentHorizontalVelocity;
    public bool IsGrounded => controller.isGrounded;
    public Vector3 HorizontalVelocity => currentHorizontalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
    }

    public void Initialize(Transform camTransform)
    {
        cameraTransform = camTransform;
    }

    private void Update()
    {
        HandleTimers();
        HandleAutoFireToggle();

        HandleRotation();
        HandleMovementAndDash();
        HandleCombat();

        playerInput.ConsumeTriggers();
    }

    private void HandleTimers()
    {
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;
        if (currentWeaponCooldown > 0) currentWeaponCooldown -= Time.deltaTime;
    }

    private void HandleAutoFireToggle()
    {
        if (playerInput.AutoFireToggleTriggered)
        {
            isAutoFireActive = !isAutoFireActive;
            Debug.Log($"Auto-Fire ist jetzt: {(isAutoFireActive ? "AN" : "AUS")}");
        }
    }

    // --- ROTATION (Maus oder Auto-Aim) ---
    private void HandleRotation()
    {
        if (isAutoFireActive)
        {
            FindNearestEnemy();
            if (currentTarget != null)
            {
                Vector3 lookDir = currentTarget.position - transform.position;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 15f);

                return;
            }
        }

        if (Camera.main != null && Mouse.current != null)
        {
            Plane groundPlane = new Plane(Vector3.up, transform.position);
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (groundPlane.Raycast(ray, out float hitDistance))
            {
                Vector3 targetPoint = ray.GetPoint(hitDistance);
                Vector3 lookDir = targetPoint - transform.position;
                lookDir.y = 0f;

                if (lookDir.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.LookRotation(lookDir);
                }
            }
        }
    }

    // --- MOVEMENT & DASH ---
    private void HandleMovementAndDash()
    {
        ApplyGravity();

        // Dash Starten
        if (playerInput.DashTriggered && dashCooldownTimer <= 0f && !isDashing)
        {
            StartDash();
        }

        Vector3 moveVelocity = Vector3.zero;

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            moveVelocity = dashDirection * dashSpeed;

            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
        else
        {
            moveVelocity = GetInputMoveVelocity();
        }

        currentHorizontalVelocity = moveVelocity;

        // Die Bewegung besteht jetzt nur noch aus horizontaler Eingabe + Gravitation
        Vector3 combined = currentHorizontalVelocity + verticalVelocity;
        controller.Move(combined * Time.deltaTime);
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        Vector2 input = playerInput.MoveInput;
        if (input.sqrMagnitude > 0.01f)
        {
            Vector3 inputDir = new Vector3(input.x, 0f, input.y);
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            dashDirection = (camForward * inputDir.z + camRight * inputDir.x).normalized;
        }
        else
        {
            dashDirection = transform.forward;
        }
    }

    private Vector3 GetInputMoveVelocity()
    {
        if (cameraTransform == null) return Vector3.zero;

        Vector2 input = playerInput.MoveInput;
        Vector3 inputDir = new Vector3(input.x, 0f, input.y);

        if (inputDir.sqrMagnitude <= 0.01f) return Vector3.zero;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        return (camForward * inputDir.z + camRight * inputDir.x).normalized * moveSpeed;
    }

    // --- COMBAT LOGIC ---
    private void HandleCombat()
    {
        if (equippedWeapon == null || isDashing) return;

        bool shouldFire = false;

        if (isAutoFireActive)
        {
            if (currentTarget != null)
            {
                float sqrDistance = (currentTarget.position - transform.position).sqrMagnitude;
                if (sqrDistance <= equippedWeapon.range * equippedWeapon.range)
                {
                    shouldFire = true;
                }
            }
        }
        else
        {
            shouldFire = playerInput.IsFiring;
        }

        if (shouldFire && currentWeaponCooldown <= 0f)
        {
            equippedWeapon.ExecuteAttack(this, currentTarget);
            currentWeaponCooldown = equippedWeapon.attackCooldown;
        }
    }

    private void FindNearestEnemy()
    {
        currentTarget = null;
        if (equippedWeapon == null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, equippedWeapon.range, enemyLayer);
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            float sqrDistance = (hit.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance < closestDistanceSqr)
            {
                closestDistanceSqr = sqrDistance;
                currentTarget = hit.transform;
            }
        }
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity.y < 0f)
            verticalVelocity.y = -2f;
        verticalVelocity.y += gravity * Time.deltaTime;
    }
}