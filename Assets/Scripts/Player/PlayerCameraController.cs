using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomLerpSpeed = 10f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 15f;

    private PlayerInput playerInput;
    private CinemachineOrbitalFollow orbital;

    private float targetZoom;
    private float currentZoom;

    // Lifecycle

    private void Awake()
    {
        // Only resolve local Cinemachine components here.
        // PlayerInput is injected later via Initialize().
        CinemachineCamera cam = GetComponent<CinemachineCamera>();
        orbital = cam.GetComponent<CinemachineOrbitalFollow>();

        targetZoom = currentZoom = orbital.Radius;
    }
    private void Update()
    {
        // Guard: do nothing until Initialize() has been called.
        if (playerInput == null) return;

        float zoomInput = playerInput.ZoomInput;

        if (zoomInput != 0f)
            targetZoom = Mathf.Clamp(orbital.Radius - zoomInput * zoomSpeed, minDistance, maxDistance);

        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomLerpSpeed);
        orbital.Radius = currentZoom;
    }

    // Called by SceneBootstrapper once the avatar has been instantiated.
    public void Initialize(PlayerInput input)
    {
        playerInput = input;
    }
}
