using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController controller;
    private PlayerInput playerInput;
    private Transform cameraTransform;
    private Vector3 velocity;

    // Read-only state exposed for other systems (e.g. fall damage, stamina drain)
    public Vector3 Velocity => velocity;
    public bool IsGrounded => controller.isGrounded;

    // Lifecycle

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
    }

    // Called by SceneBootstrapper once the main camera is confirmed present.
    public void Initialize(Transform camTransform)
    {
        cameraTransform = camTransform;
    }

    private void Update()
    {
        Move();
        ApplyGravity();
    }

    // Private Movement Logic

    private void Move()
    {
        // Guard: do nothing until Initialize() has been called.
        if (cameraTransform == null) return;

        Vector2 input = playerInput.MoveInput;
        Vector3 inputDir = new Vector3(input.x, 0f, input.y);

        if (inputDir.sqrMagnitude <= 0.01f) return;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * inputDir.z + camRight * inputDir.x;
        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        transform.rotation = Quaternion.LookRotation(moveDir);
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}