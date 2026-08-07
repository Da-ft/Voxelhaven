using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    // Singleton-Instanz für einfachen Zugriff von außen (z.B. durch die Diebe)
    public static WaveManager Instance { get; private set; }

    [Header("Spawn Pools")]
    public EnemySpawnConfig[] nightEnemies;
    public EnemySpawnConfig[] dayEnemies;

    [Header("Spawn Points")]
    [Tooltip("Zufällige Punkte, an denen normale Gegner spawnen können")]
    public Transform[] spawnPoints;

    [Header("Pacing Settings")]
    [Tooltip("Basis-Budget für Zyklus 1")]
    public int baseBudget = 100;
    [Tooltip("In wie viele Gruppen (Sub-Waves) soll das Budget aufgeteilt werden?")]
    public int subWavesPerPhase = 3;
    [Tooltip("Max. Wartezeit (Sekunden) bis die nächste Sub-Wave spawnt, auch wenn noch Gegner leben.")]
    public float subWaveTimeout = 20f;

    private Coroutine currentWaveRoutine;
    private bool isSubscribed = false;

    // Speichert das von Dieben gestohlene Budget für die nächste Welle
    private int bonusBudget = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

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
        if (isSubscribed && GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
            isSubscribed = false;
        }
    }

    /// <summary>
    /// Wird von entkommenen Dieben aufgerufen, um das Welle-Budget zu erhöhen.
    /// </summary>
    public void AddBonusBudget(int amount)
    {
        bonusBudget += amount;
        Debug.Log($"[WaveManager] Bonus-Budget um {amount} erhöht! Aktuelles Bonus-Budget für nächste Welle: {bonusBudget}");
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
            isSubscribed = true;

            // Falls wir direkt beim Start schon in einer Kampfphase sind, Welle starten!
            HandlePhaseChanged(GameManager.Instance.CurrentPhase);
        }
    }

    private void HandlePhaseChanged(GameManager.GamePhase newPhase)
    {
        // Alte Routine sicherheitshalber stoppen
        if (currentWaveRoutine != null)
        {
            StopCoroutine(currentWaveRoutine);
            currentWaveRoutine = null;
        }

        // Neue Routine starten, falls wir im Kampf sind
        if (newPhase == GameManager.GamePhase.Night && nightEnemies.Length > 0)
        {
            currentWaveRoutine = StartCoroutine(SpawnWaveRoutine(nightEnemies));
        }
        else if (newPhase == GameManager.GamePhase.Day && dayEnemies.Length > 0)
        {
            currentWaveRoutine = StartCoroutine(SpawnWaveRoutine(dayEnemies));
        }
    }

    private IEnumerator SpawnWaveRoutine(EnemySpawnConfig[] pool)
    {
        // Budget für die gesamte Phase berechnen (Basis-Budget * Cycle + gestohlenes Bonus-Budget)
        int totalBudget = (baseBudget * GameManager.Instance.CycleCounter) + bonusBudget;

        // Bonus-Budget zurücksetzen, da es für diese Welle aufgebraucht wurde
        bonusBudget = 0;

        // Budget pro Sub-Wave berechnen
        int budgetPerSubWave = totalBudget / Mathf.Max(1, subWavesPerPhase);
        int remainingTotalBudget = totalBudget;

        // Sub-Waves abarbeiten
        while (remainingTotalBudget > 0)
        {
            int currentSubWaveBudget = Mathf.Min(budgetPerSubWave, remainingTotalBudget);

            // Squad berechnen und einkaufen
            List<GameObject> squadToSpawn = BuySquad(pool, currentSubWaveBudget, out int spentBudget);
            remainingTotalBudget -= spentBudget;

            if (squadToSpawn.Count == 0) break; // Nichts mehr einkaufbar

            // Squad physisch spawnen
            foreach (GameObject enemyPrefab in squadToSpawn)
            {
                Transform selectedSpawnPoint = null;
                ScrapDropZone chosenDropZone = null;

                // Prüfen, ob der Gegner ein Dieb ist
                bool isThief = false;
                if (enemyPrefab.TryGetComponent(out EnemyBrain prefabBrain))
                {
                    if (prefabBrain.enemyProfile is EnemyThiefProfile)
                    {
                        isThief = true;
                    }
                }

                // SPREAD-LOGIK FOR DIEBE VS. NORMALE GEGNER
                if (isThief && ScrapDropZone.AllZones.Count > 0)
                {
                    // Diebe spawnen an einer zufälligen Diebes-Zone (ScrapDropZone)
                    chosenDropZone = ScrapDropZone.AllZones[Random.Range(0, ScrapDropZone.AllZones.Count)];
                    selectedSpawnPoint = chosenDropZone.transform;
                }
                else if (spawnPoints.Length > 0)
                {
                    // Normale Gegner spawnen an den normalen SpawnPoints
                    selectedSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                }

                if (selectedSpawnPoint == null) continue;

                // Zufälligen Offset im 2-Meter-Radius um den Spawnpunkt berechnen
                Vector3 randomOffset = Random.insideUnitSphere * 2f;
                randomOffset.y = 0f;
                Vector3 spawnPos = selectedSpawnPoint.position + randomOffset;

                // Sicherstellen, dass die Position auch WIRKLICH auf dem NavMesh liegt
                if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out UnityEngine.AI.NavMeshHit hit, 3f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    spawnPos = hit.position;
                }

                // Gegner an der berechneten Position spawnen
                GameObject spawnedEnemy = ObjectPoolManager.SpawnObject(enemyPrefab, spawnPos, selectedSpawnPoint.rotation, ObjectPoolManager.PoolType.GameObjects);

                if (spawnedEnemy.TryGetComponent(out EnemyBrain brain))
                {
                    // Falls es ein Dieb ist, weisen wir ihm seine Heimatadresse zu
                    if (isThief && chosenDropZone != null)
                    {
                        brain.HomeZone = chosenDropZone.transform;
                    }

                    brain.Initialize();
                }

                // WINZIGE PAUSE (0.08s): Verhindert das Ineinander-Stapeln der Agenten!
                yield return new WaitForSeconds(0.08f);
            }

            // CLEAR OR TIMEOUT - Warten bis Gegner tot sind oder der Timer abläuft
            float timer = subWaveTimeout;
            while (timer > 0f && AreEnemiesAlive())
            {
                timer -= Time.deltaTime;
                yield return null; // Einen Frame warten
            }

            // Schleife geht weiter -> nächste Sub-Wave!
        }
    }

    private List<GameObject> BuySquad(EnemySpawnConfig[] pool, int maxBudget, out int spent)
    {
        List<GameObject> squad = new List<GameObject>();
        Dictionary<EnemySpawnConfig, int> spawnedCounts = new Dictionary<EnemySpawnConfig, int>();
        spent = 0;
        int currentBudget = maxBudget;

        int currentCycle = GameManager.Instance.CycleCounter;

        while (currentBudget > 0)
        {
            List<(EnemySpawnConfig config, int dynWeight)> validOptions = new List<(EnemySpawnConfig, int)>();
            int totalWeight = 0;

            foreach (var enemy in pool)
            {
                if (currentCycle < enemy.minCycleToSpawn) continue;

                spawnedCounts.TryGetValue(enemy, out int currentCount);
                bool underLimit = enemy.maxPerSubWave <= 0 || currentCount < enemy.maxPerSubWave;

                if (enemy.cost <= currentBudget && underLimit)
                {
                    int cyclesActive = currentCycle - enemy.minCycleToSpawn;
                    int dynamicWeight = enemy.baseWeight + (enemy.weightIncreasePerCycle * cyclesActive);
                    dynamicWeight = Mathf.Max(1, dynamicWeight);

                    validOptions.Add((enemy, dynamicWeight));
                    totalWeight += dynamicWeight;
                }
            }

            if (validOptions.Count == 0) break;

            int randomValue = Random.Range(0, totalWeight);
            int cumulativeWeight = 0;
            EnemySpawnConfig chosenEnemy = null;

            foreach (var option in validOptions)
            {
                cumulativeWeight += option.dynWeight;
                if (randomValue < cumulativeWeight)
                {
                    chosenEnemy = option.config;
                    break;
                }
            }

            if (chosenEnemy != null)
            {
                squad.Add(chosenEnemy.enemyPrefab);
                currentBudget -= chosenEnemy.cost;
                spent += chosenEnemy.cost;

                if (!spawnedCounts.ContainsKey(chosenEnemy)) spawnedCounts[chosenEnemy] = 0;
                spawnedCounts[chosenEnemy]++;
            }
        }

        return squad;
    }

    private bool AreEnemiesAlive()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        return enemies.Length > 0;
    }
}