using System;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GamePhase { Night, Day, Rest }

    [Header("Current Phase")]
    public GamePhase CurrentPhase;

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
        ChangePhase(GamePhase.Night);
    }

    public void ChangePhase(GamePhase newPhase)
    {
        CurrentPhase = newPhase;
        Debug.Log($"[GameManager] Phase gewechselt zu: {newPhase}");

        if (newPhase == GamePhase.Day)
        {
            // TODO: Scrap Pile instantiate! Feed with "scrapAmount"
            Debug.Log($"Scrap Pile with {scrapAmount} was created!");
        }

        // Inform Abos
        OnPhaseChanged?.Invoke(CurrentPhase);
    }

    // TODO: Clean later! Testing Buttons!
    public void StartDayPhase() => ChangePhase(GamePhase.Day);
    public void StartRestPhase() => ChangePhase(GamePhase.Rest);
    public void StartNightPhase() => ChangePhase(GamePhase.Night);

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
}
