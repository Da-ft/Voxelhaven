using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Knockback")]
    [SerializeField] private float maxPushSpeed = 12f;
    [SerializeField] private float pushDecay = 4f;
    [SerializeField, Range(0f, 1f)] private float pushMoveControl = 0.35f;
    // pushMoveControl: wie stark Spieler-Eingabe während eines aktiven Pushs noch wirkt.
    // 0 = keine Kontrolle während des Pushs, 1 = Push hat keinerlei Einfluss auf die Steuerung.
    // Bewusst additiv statt Kontrollverlust, damit man während des Schubs noch lenken kann.

    private CharacterController controller;
    private PlayerInput playerInput;
    private Transform cameraTransform;
    private Vector3 verticalVelocity;
    private Vector3 pushVelocity;

    public Vector3 Velocity => verticalVelocity + pushVelocity;
    public bool IsGrounded => controller.isGrounded;
    public bool IsBeingPushed => pushVelocity.sqrMagnitude > 0.01f;

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

    // Wird von Player.TakeDamage aufgerufen, wenn ein Treffer eine Knockback-Richtung mitbringt
    // Additiv, damit mehrere Treffer kurz hintereinander sich aufsummieren, bis die Deckelung greift
    public void ApplyPush(Vector3 impulse)
    {
        pushVelocity += impulse;
        if (pushVelocity.magnitude > maxPushSpeed)
            pushVelocity = pushVelocity.normalized * maxPushSpeed;
    }

    private void Update()
    {
        Vector3 inputVelocity = GetInputMoveVelocity();
        DecayPush();
        ApplyGravity();

        // Alle drei Bewegungsquellen (Eingabe, Push, Gravitation) werden zu EINEM Move()-Aufruf pro Frame kombiniert, statt mehrfach unabhängig zu bewegen - sonst ist die gegenseitige Beeinflussung nicht mehr nachvollziehbar
        float controlFactor = IsBeingPushed ? pushMoveControl : 1f;
        Vector3 combined = inputVelocity * controlFactor + pushVelocity + verticalVelocity;

        controller.Move(combined * Time.deltaTime);
    }

    // Private Movement Logic

    private Vector3 GetInputMoveVelocity()
    {
        // Guard: do nothing until Initialize() has been called.
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

        Vector3 moveDir = camForward * inputDir.z + camRight * inputDir.x;
        transform.rotation = Quaternion.LookRotation(moveDir);

        return moveDir * moveSpeed;
    }

    private void DecayPush()
    {
        if (pushVelocity.sqrMagnitude <= 0.0001f)
        {
            pushVelocity = Vector3.zero;
            return;
        }

        pushVelocity = Vector3.MoveTowards(pushVelocity, Vector3.zero, pushDecay * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity.y < 0f)
            verticalVelocity.y = -2f;

        verticalVelocity.y += gravity * Time.deltaTime;
    }
}