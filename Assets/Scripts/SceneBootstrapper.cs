using UnityEngine;
using Unity.Cinemachine;

public class SceneBootstrapper : MonoBehaviour
{
    [Header("Avatar")]
    [SerializeField] private GameObject avatarPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Camera")]
    [SerializeField] private CinemachineCamera cinemachineCamera;

    private void Start()
    {
        if (avatarPrefab == null)
        {
            Debug.LogWarning("[SceneBootstrapper] Avatar prefab is not assigned.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("[SceneBootstrapper] Spawn point is not assigned.");
            return;
        }

        // Instantiate avatar at spawn point position and rotation
        GameObject avatar = Instantiate(avatarPrefab, spawnPoint.position, spawnPoint.rotation);

        PlayerInput playerInput = avatar.GetComponent<PlayerInput>();
        PlayerController playerController = avatar.GetComponent<PlayerController>();

        if (cinemachineCamera == null)
        {
            Debug.LogWarning("[SceneBootstrapper] CinemachineCamera reference is missing. Camera will not be initialized.");
            return;
        }

        PlayerCameraController playerCameraController = cinemachineCamera.GetComponent<PlayerCameraController>();

        if (playerCameraController == null)
        {
            Debug.LogWarning("[SceneBootstrapper] PlayerCameraController not found on CinemachineCamera.");
            return;
        }

        if (Camera.main == null)
        {
            Debug.LogWarning("[SceneBootstrapper] No main camera found in scene.");
            return;
        }

        // Wire up cross-object dependencies.
        playerCameraController.Initialize(playerInput);
        playerController.Initialize(Camera.main.transform);

        // Verbindet das persistente Player-Singleton mit dem Avatar dieser Szene
        if (Player.Instance != null)
            Player.Instance.BindAvatar(avatar.transform, playerController);
        
        // Set Cinemachine follow and look-at targets to the avatar.
        cinemachineCamera.Follow = avatar.transform;
        cinemachineCamera.LookAt = avatar.transform;
    }
}