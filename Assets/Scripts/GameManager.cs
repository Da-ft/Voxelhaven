using System;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GamePhase { Night, Day, Rest }

    [Header("Phase Settings")]
    public GamePhase CurrentPhase { get; private set; }

    [Tooltip("Length of Day/Nightcycle in seconds.")]
    [SerializeField] private float phaseDuration = 60f;

    // TODO: Link with UI
    public float TimeRemaining { get; private set; }

    [Header("Difficulty")]
    public int CycleCounter { get; private set; } = 1;

    [Header("Resources")]
    private int scrapAmount;
    private int manaAmount;

    [Header("Run Totals")]
    private int totalScrapEarnedThisRun = 0;
    private int totalManaEarnedThisRun = 0;

    [Header("Meta-Progression Conversion Rates")]
    [Tooltip("Wie viel 1 Scrap in Meta-Währung wert ist (z.B. 0.1 = 100 Scrap -> 10 Meta-Punkte)")]
    [SerializeField] private float scrapToMetaRatio = 0.1f;
    [Tooltip("Wie viel 1 Mana in Meta-Währung wert ist (z.B. 0.5 = 20 Mana -> 10 Meta-Punkte)")]
    [SerializeField] private float manaToMetaRatio = 0.5f;

    // ReadOnly Properties für UI
    public int Scrap => scrapAmount;
    public int Mana => manaAmount;

    public event Action<GamePhase> OnPhaseChanged;
    public event Action<int> OnScrapChanged;
    public event Action<int> OnManaChanged;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Game-Loop starts at Night!
        StartPhase(GamePhase.Night);
    }

    private void Update()
    {
        if (CurrentPhase == GamePhase.Night || CurrentPhase == GamePhase.Day)
        {
            TimeRemaining -= Time.deltaTime;
            if (TimeRemaining <= 0f)
            {
                EndCurrentPhase();
            }
        }

#if UNITY_EDITOR
        // Schneller Debug-Cheat auf der Taste F1
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.f1Key.wasPressedThisFrame)
        {
            // Passe ggf. 'AddScrap' an deine tatsächliche Methode im GameManager an
            GameManager.Instance.AddMana(100);
            GameManager.Instance.AddScrap(100);

            Debug.Log("<color=yellow>[DEBUG] 100 Scrap & Mana ercheatet!</color>");
        }
#endif
    }

    private void OnEnable()
    {
        if (DataPersistenceManager.instance != null)
        {
            DataPersistenceManager.instance.OnLoadData += LoadData;
            DataPersistenceManager.instance.OnSaveData += SaveData;
        }
    }

    private void OnDisable()
    {
        if (DataPersistenceManager.instance != null)
        {
            DataPersistenceManager.instance.OnLoadData -= LoadData;
            DataPersistenceManager.instance.OnSaveData -= SaveData;
        }
    }

    private void LoadData(GameData data)
    {
        CycleCounter = data.cycleCount;
    }

    private void SaveData(GameData data)
    {
        data.cycleCount = CycleCounter;
    }

    private void StartPhase(GamePhase newPhase)
    {
        CurrentPhase = newPhase;

        if (CurrentPhase == GamePhase.Night || CurrentPhase == GamePhase.Day)
        {
            TimeRemaining = phaseDuration;
        }

        Debug.Log($"[GameManager] Start Phase: {CurrentPhase} (Cycle: {CycleCounter}");
        OnPhaseChanged?.Invoke(CurrentPhase);
    }

    private void EndCurrentPhase()
    {
        CleanupActiveEnemies();

        switch (CurrentPhase)
        {
            case GamePhase.Night:
                StartPhase(GamePhase.Day);
                break;
            case GamePhase.Day:
                StartPhase(GamePhase.Rest);
                break;
            case GamePhase.Rest:
                CycleCounter++;
                StartPhase(GamePhase.Night);
                break;
        }
    }

    public void LeaveRestPhase()
    {
        if (CurrentPhase == GamePhase.Rest)
        {
            EndCurrentPhase();
        }
    }

    private void CleanupActiveEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            // Ab in den Pool statt Destroy
            ObjectPoolManager.ReturnObjectToPool(enemy, ObjectPoolManager.PoolType.GameObjects);
        }

        if (enemies.Length > 0)
        {
            Debug.Log($"[GameManager] {enemies.Length} verbleibende Gegner in den Pool geschickt!");
        }
    }

    // Resource Logic Stuff
    public void AddScrap(int amount)
    {
        if (amount <= 0) return;

        scrapAmount += amount;
        totalScrapEarnedThisRun += amount;
        OnScrapChanged?.Invoke(scrapAmount);
    }

    public void AddMana(int amount)
    {
        if (amount <= 0) return;

        manaAmount += amount;
        totalManaEarnedThisRun += amount;
        OnManaChanged?.Invoke(manaAmount);
    }

    // Called when Scrap gets stolen
    public int StealScrap(int requestedAmount)
    {
        int amountToSteal = Mathf.Min(scrapAmount, requestedAmount);

        if (amountToSteal > 0)
        {
            scrapAmount -= amountToSteal;
            OnScrapChanged?.Invoke(scrapAmount);
        }

        return amountToSteal;
    }

    // Call on run end or death
    public void EndRunAndCalculateMetaCurrency()
    {
        // Formula: (Scrap Total * ConversionRatio) + (Mana Total * ConversionRatio)
        int earnedMetaCurrency = Mathf.RoundToInt(
            (totalScrapEarnedThisRun * scrapToMetaRatio) +
            (totalManaEarnedThisRun * manaToMetaRatio)
        );

        // Add Currency to UpgradeManager
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.AddMetaCurrency(earnedMetaCurrency);
        }

        Debug.Log($"[Run-Ende] Gesamter Einnahmen: {totalScrapEarnedThisRun} Scrap, {totalManaEarnedThisRun} Mana. " +
                  $"-> Umgewandelt in {earnedMetaCurrency} Meta-Ressourcen!");

        // Reset Stuff for next Run
        totalScrapEarnedThisRun = 0;
        totalManaEarnedThisRun = 0;
        scrapAmount = 0;
        manaAmount = 0;

        UpgradeManager.Instance.ResetRunUpgrades();
    }
}
