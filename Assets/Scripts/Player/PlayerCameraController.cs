using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomLerpSpeed = 10f;
    [SerializeField] private float minDistance = 8f;  // Für Isometrie oft etwas höher ansetzen
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
            // Wir speichern die normalisierte Richtung des Offsets (den "isometrischen Winkel").
            // So können wir später einfach die Distanz (magnitude) skalieren, ohne den Winkel zu verändern.
            normalizedOffset = cinemachineFollow.FollowOffset.normalized;

            // Start-Zoom basierend auf dem im Inspector eingestellten Offset
            targetZoom = currentZoom = cinemachineFollow.FollowOffset.magnitude;
        }
        else
        {
            Debug.LogWarning("[PlayerCameraController] CinemachineFollow component missing!");
        }
    }

    private void Update()
    {
        // Guard: do nothing until Initialize() has been called or component is missing.
        if (playerInput == null || cinemachineFollow == null) return;

        float zoomInput = playerInput.ZoomInput;

        if (zoomInput != 0f)
        {
            // Bei isometrischen Cams zieht man den Input meist ab, damit Scrollrad VOR = Reinzoomen bedeutet
            targetZoom = Mathf.Clamp(targetZoom - zoomInput * zoomSpeed, minDistance, maxDistance);
        }

        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomLerpSpeed);

        // Den neuen Zoom-Wert auf die Follow-Komponente anwenden
        cinemachineFollow.FollowOffset = normalizedOffset * currentZoom;
    }

    // Called by SceneBootstrapper once the avatar has been instantiated.
    public void Initialize(PlayerInput input)
    {
        playerInput = input;
    }
}