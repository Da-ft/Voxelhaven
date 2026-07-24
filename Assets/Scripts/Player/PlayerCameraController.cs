using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomLerpSpeed = 10f;
    [SerializeField] private float minDistance = 8f;  // F�r Isometrie oft etwas h�her ansetzen
    [SerializeField] private float maxDistance = 25f;

    private PlayerInput playerInput;
    private CinemachineFollow cinemachineFollow;
    private Vector3 normalizedOffset;

    private float targetZoom;
    private float currentZoom;

    // Lifecycle

    private void Awake()
    {
        // Only resolve local Cinemachine components here.
        CinemachineCamera cam = GetComponent<CinemachineCamera>();
        cinemachineFollow = cam.GetComponent<CinemachineFollow>();

        if (cinemachineFollow != null)
        {
            // save normalized offset dir
            normalizedOffset = cinemachineFollow.FollowOffset.normalized;
            // Start Zoom based on var in inspector
            targetZoom = currentZoom = cinemachineFollow.FollowOffset.magnitude;
        }

        else
        {
            Debug.LogWarning("[PlayerCameraController] CinemachineFollow component missing!");
        }
    }

    private void Update()
    {
        // Guard: do nothing until Initialize() has been called.
        if (playerInput == null || cinemachineFollow == null) return;

        float zoomInput = playerInput.ZoomInput;

        if (zoomInput != 0f)
        {
            targetZoom = Mathf.Clamp(targetZoom - zoomInput * zoomSpeed, minDistance, maxDistance);
        }

        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomLerpSpeed);

        cinemachineFollow.FollowOffset = normalizedOffset * currentZoom;
    }

    // Called by SceneBootstrapper once the avatar has been instantiated.
    public void Initialize(PlayerInput input)
    {
        playerInput = input;
    }
}