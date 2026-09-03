using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private PlayerControls controls;
    private float pendingMouseZoom;

    public Vector2 MoveInput { get; private set; }
    public float ZoomInput { get; private set; }

    public bool IsFiring  { get; private set; }
    public bool DashTriggered { get; private set; }
    public bool AutoFireToggleTriggered { get; private set; }

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

        controls.Player.Fire.performed += ctx => IsFiring = true;
        controls.Player.Fire.canceled += ctx => IsFiring = false;

        controls.Player.Dash.performed += ctx => DashTriggered = true;
        controls.Player.ToggleAutoFire.performed += ctx => AutoFireToggleTriggered = true;

        PauseMenuUI.OnPauseStateChanged += HandlePauseState;
    }

    private void OnDisable()
    {
        controls.Player.Move.performed -= HandleMove;
        controls.Player.Move.canceled -= HandleMove;
        controls.CameraControls.MouseZoom.performed -= HandleMouseZoom;

        controls.Player.Fire.performed -= ctx => IsFiring = true;
        controls.Player.Fire.canceled -= ctx => IsFiring = false;

        controls.Player.Dash.performed -= ctx => DashTriggered = true;
        controls.Player.ToggleAutoFire.performed-= ctx => AutoFireToggleTriggered = true;

        PauseMenuUI.OnPauseStateChanged -= HandlePauseState;

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

    private void HandlePauseState(bool isPaused)
    {
        if (isPaused)
        {
            // Deactivate Inputs
            controls.Disable();

            // fallback against stuck inputs
            MoveInput = Vector2.zero;
            ZoomInput = 0f;
            IsFiring = false;
            DashTriggered = false;
            AutoFireToggleTriggered = false;
        }
        else
        {
            // restart input system
            controls.Enable();
        }
    }

    private void Update()
    {
        // Mouse scroll takes priority; gamepad is a fallback continuous axis.
        float gamepadZoom = controls.CameraControls.GamepadZoom.ReadValue<float>();
        ZoomInput = pendingMouseZoom != 0f ? pendingMouseZoom : gamepadZoom;
        pendingMouseZoom = 0f;
    }

    public void ConsumeTriggers()
    {
        DashTriggered = false;
        AutoFireToggleTriggered = false;
    }
}
