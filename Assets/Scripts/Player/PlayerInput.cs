using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private PlayerControls controls;
    private float pendingMouseZoom;

    public Vector2 MoveInput { get; private set; }
    public float ZoomInput { get; private set; }

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Player.Move.performed += HandleMove;
        controls.Player.Move.canceled += HandleMove;
        controls.CameraControls.MouseZoom.performed += HandleMouseZoom;
    }

    private void OnDisable()
    {
        controls.Player.Move.performed -= HandleMove;
        controls.Player.Move.canceled -= HandleMove;
        controls.CameraControls.MouseZoom.performed -= HandleMouseZoom;
        controls.Disable();
    }

    private void OnDestroy()
    {
        controls.Dispose();
    }

    private void HandleMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    private void HandleMouseZoom(InputAction.CallbackContext context)
    {
        // Scroll events fire once per frame; pendingMouseZoom is consumed in Update().
        pendingMouseZoom = context.ReadValue<Vector2>().y;
    }

    private void Update()
    {
        // Mouse scroll takes priority; gamepad is a fallback continuous axis.
        float gamepadZoom = controls.CameraControls.GamepadZoom.ReadValue<float>();
        ZoomInput = pendingMouseZoom != 0f ? pendingMouseZoom : gamepadZoom;
        pendingMouseZoom = 0f;
    }
}
