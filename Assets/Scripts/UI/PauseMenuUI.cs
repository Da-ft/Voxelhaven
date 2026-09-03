using System;
using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Main Panel des Pause Menus, das ein und ausgeblendet wird.")]
    [SerializeField] private GameObject pausePanel;

    private PlayerControls controls;
    private bool isPaused = false;

    public static event Action<bool> OnPauseStateChanged;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.UI.PauseMenu.performed += ctx => TogglePause();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Start()
    {
        pausePanel.SetActive(false);
    }

    private void TogglePause()
    {
        Debug.Log($"[PauseMenu] TogglePause aufgerufen! War pausiert: {isPaused}");

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);

        // Call all Scripts: Game is Paused!
        OnPauseStateChanged?.Invoke(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);

        // Call all Scripts: Game is Resumed!
        OnPauseStateChanged?.Invoke(false);
    }

    public void SaveGameButton()
    {
        if (DataPersistenceManager.instance != null)
        {
            DataPersistenceManager.instance.SaveGame();
        }
        else
        {
            Debug.LogWarning("[PauseMenu] DataPersistenceManager fehlt in der Szene!");
        }
    }

    public void QuitGameButton()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
