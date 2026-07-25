using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUDUI : MonoBehaviour
{
    [Header("Player Health UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;

    [Header("Phase & Timer UI")]
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text cycleText;

    [Header("Resource UI")]
    [SerializeField] private TMP_Text scrapText;
    [SerializeField] private TMP_Text manaText;

    // trennen Subscriptions, falls ein Singleton mal schneller lädt als das andere
    private bool isPlayerSubscribed = false;
    private bool isGameManagerSubscribed = false;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Update()
    {
        // Timer Logik
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.CurrentPhase == GameManager.GamePhase.Night ||
            GameManager.Instance.CurrentPhase == GameManager.GamePhase.Day)
        {
            float time = Mathf.Max(0f, GameManager.Instance.TimeRemaining);
            int seconds = Mathf.CeilToInt(time);

            if (timerText != null)
                timerText.text = $"{seconds}s";
        }
        else
        {
            if (timerText != null)
                timerText.text = "--";
        }
    }

    private void TrySubscribe()
    {
        // 1. Am Player anmelden
        if (!isPlayerSubscribed && Player.Instance != null)
        {
            Player.Instance.OnHealthChanged += UpdateHealthBar;
            isPlayerSubscribed = true;

            // Initialen HP-Wert direkt setzen
            UpdateHealthBar(Player.Instance.CurrentHealth, Player.Instance.MaxHealth);
        }

        // 2. Am GameManager anmelden
        if (!isGameManagerSubscribed && GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged += UpdatePhaseUI;
            GameManager.Instance.OnScrapChanged += UpdateScrapUI;
            GameManager.Instance.OnManaChanged += UpdateManaUI;

            isGameManagerSubscribed = true;

            // Initiale Werte direkt setzen
            UpdatePhaseUI(GameManager.Instance.CurrentPhase);
            UpdateScrapUI(GameManager.Instance.Scrap);
            UpdateManaUI(GameManager.Instance.Mana);
        }
    }

    private void Unsubscribe()
    {
        // Memory Leaks verhindern
        if (isPlayerSubscribed && Player.Instance != null)
        {
            Player.Instance.OnHealthChanged -= UpdateHealthBar;
            isPlayerSubscribed = false;
        }

        if (isGameManagerSubscribed && GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged -= UpdatePhaseUI;
            GameManager.Instance.OnScrapChanged -= UpdateScrapUI;
            GameManager.Instance.OnManaChanged -= UpdateManaUI;

            isGameManagerSubscribed = false;
        }
    }

    // --- EVENT HANDLER ---

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthSlider != null && maxHealth > 0f)
        {
            healthSlider.value = currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            int currentInt = Mathf.CeilToInt(currentHealth);
            int maxInt = Mathf.CeilToInt(maxHealth);
            healthText.text = $"{currentInt} / {maxInt}";
        }
    }

    private void UpdatePhaseUI(GameManager.GamePhase phase)
    {
        if (phaseText != null)
        {
            switch (phase)
            {
                case GameManager.GamePhase.Night:
                    phaseText.text = "Night";
                    break;
                case GameManager.GamePhase.Day:
                    phaseText.text = "Day";
                    break;
                case GameManager.GamePhase.Rest:
                    phaseText.text = "Rest";
                    break;
            }
        }

        if (cycleText != null && GameManager.Instance != null)
        {
            cycleText.text = $"{GameManager.Instance.CycleCounter}";
        }
    }

    private void UpdateScrapUI(int amount)
    {
        if (scrapText != null)
            scrapText.text = $"Scrap: {amount}";
    }

    private void UpdateManaUI(int amount)
    {
        if (manaText != null)
            manaText.text = $"Mana: {amount}";
    }
}