using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(PlayerController), typeof(PlayerInput))]
public class PlayerSetup : MonoBehaviour
{
    private void Awake()
    {
        PlayerController controller = GetComponent<PlayerController>();

        // Controller Init, snatch cam
        if (Camera.main != null)
        {
            controller.Initialize(Camera.main.transform);
        }
        else
        {
            Debug.LogError("[PlayerSetup] Keine Main Camera in der Szene gefunden!");
        }

        // Subscribe Avatar to Player Singleton
        Player playerSingleton = Player.Instance;

        // Fallback
        if (playerSingleton == null)
        {
            playerSingleton = FindAnyObjectByType<Player>();
        }

        if (playerSingleton != null)
        {
            playerSingleton.BindAvatar(transform, controller);
        }
        else
        {
            Debug.LogError("[PlayerSetup] Player-Singleton fehlt komplett in der Szene!");
        }
    }

    private void Start()
    {
        // Attach Camera
        PlayerCameraController camController = FindAnyObjectByType<PlayerCameraController>();

        if (camController != null)
        {
            PlayerInput input = GetComponent<PlayerInput>();
            camController.Initialize(input);

            if (camController.TryGetComponent(out CinemachineCamera cineCam))
            {
                cineCam.Follow = transform;
            }
        }
        else
        {
            Debug.LogWarning("[PlayerSetup] Kein PlayerCameraController in der Szene gefunden. Kamera folgt dem Spieler nicht automatisch.");
        }
    }
}