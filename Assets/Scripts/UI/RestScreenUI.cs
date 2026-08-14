using UnityEngine;

public class RestScreenUI : MonoBehaviour
{
    [Header("Main UI Container")]
    [Tooltip("Attach to Rest-UI-Container")]
    [SerializeField] private GameObject restScreenContainer;

    [Header("Sub-Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject upgradesPanel;
    [SerializeField] private GameObject metaProgressionPanel;

    private void Start()
    {
        // UI needs to be invis
        restScreenContainer.SetActive(false);

        // Subscribe to GameManager Event
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe when destroyed, no memory leaks!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }
    }

    private void HandlePhaseChanged(GameManager.GamePhase newPhase)
    {
        if (newPhase == GameManager.GamePhase.Rest)
        {
            OpenRestScreen();
        }
        else
        {
            CloseRestScreen();
        }       
    }

    private void OpenRestScreen()
    {
        restScreenContainer.SetActive(true);
        ShowMainMenu();
        Time.timeScale = 0f;
    }

    private void CloseRestScreen()
    {
        restScreenContainer.SetActive(false);
        Time.timeScale = 1f;
    }

    #region Button-Funcs

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        upgradesPanel.SetActive(false);
        metaProgressionPanel.SetActive(false);
    }

    public void ShowUpgrades()
    {
        mainMenuPanel.SetActive(false);
        upgradesPanel.SetActive(true);
        metaProgressionPanel.SetActive(false);

        // TODO: Enable Stat Upgrade Buttons
    }

    public void ShowMetaProgression()
    {
        mainMenuPanel.SetActive(false);
        upgradesPanel.SetActive(false);
        metaProgressionPanel.SetActive(true);

        // TODO: Read Traits and SkillTreeManager display (Read-Only)
    }

    public void LeaveRestPhase()
    {
        // Call to GameManager, Close UI
        GameManager.Instance.LeaveRestPhase();
    }

    #endregion
}
