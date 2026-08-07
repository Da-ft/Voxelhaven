using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(PlayerController), typeof(PlayerInput))]
public class PlayerSetup : MonoBehaviour
{
    private void Awake()
    {
        PlayerController controller = GetComponent<PlayerController>();

        // 1. Controller initialisieren (Main Camera zuweisen)
        if (Camera.main != null)
        {
            controller.Initialize(Camera.main.transform);
        }
        else
        {
            Debug.LogError("[PlayerSetup] Keine Main Camera in der Szene gefunden!");
        }

        // 2. Avatar beim Player-Singleton anmelden (Kugelsicher!)
        Player playerSingleton = Player.Instance;

        // Fallback: Falls PlayerSetup VOR dem Player-Singleton geladen wird
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
        // 3. Kamera-Setup: Die Kamera in der Szene suchen und verkabeln
        PlayerCameraController camController = FindAnyObjectByType<PlayerCameraController>();

        if (camController != null)
        {
            // Input an die Kamera weitergeben
            PlayerInput input = GetComponent<PlayerInput>();
            camController.Initialize(input);

            // Cinemachine befehlen, diesen Avatar zu verfolgen
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