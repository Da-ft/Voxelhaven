using System;
using UnityEditor;
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

    // ReadOnly Properties für UI
    public int Scrap => scrapAmount;
    public int Mana => manaAmount;

    // Events
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
        // Loop starts at Night!
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

    // TODO: Link with UI Button to end Rest Phase!
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
        scrapAmount += amount;
        OnScrapChanged?.Invoke(scrapAmount);
    }

    public void AddMana(int amount)
    {
        manaAmount += amount;
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
}
