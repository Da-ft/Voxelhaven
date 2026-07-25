using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Spawn Pools")]
    public EnemySpawnConfig[] nightEnemies;
    public EnemySpawnConfig[] dayEnemies;

    [Header("Spawn Points")]
    [Tooltip("Zufällige Punkte, an denen Gegner spawnen können")]
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
        // 1. Budget für die gesamte Phase berechnen (skaliert mit dem Cycle)
        int totalBudget = baseBudget * GameManager.Instance.CycleCounter;

        // 2. Budget pro Sub-Wave berechnen
        int budgetPerSubWave = totalBudget / Mathf.Max(1, subWavesPerPhase);
        int remainingTotalBudget = totalBudget;

        // 3. Sub-Waves abarbeiten
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
                Transform randomSp = spawnPoints[Random.Range(0, spawnPoints.Length)];

                // 1. Zufälligen Offset im 2-Meter-Radius um den Spawnpunkt berechnen
                Vector3 randomOffset = Random.insideUnitSphere * 2f;
                randomOffset.y = 0f; // Offset nur auf der XZ-Ebene (Boden)!
                Vector3 spawnPos = randomSp.position + randomOffset;

                // 2. Sicherstellen, dass die Position auch WIRKLICH auf dem NavMesh liegt
                if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out UnityEngine.AI.NavMeshHit hit, 3f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    spawnPos = hit.position;
                }

                // 3. Gegner an der berechneten Position spawnen
                GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPos, randomSp.rotation);

                if (spawnedEnemy.TryGetComponent(out EnemyBrain brain))
                {
                    brain.Initialize();
                }

                // 4. WINZIGE PAUSE (0.08s): Verhindert das Ineinander-Stapeln der Agenten!
                yield return new WaitForSeconds(0.08f);
            }

            // 4. CLEAR OR TIMEOUT - Warten bis Gegner tot sind oder der Timer abläuft
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

        // Aktuellen Zyklus aus dem GameManager holen
        int currentCycle = GameManager.Instance.CycleCounter;

        while (currentBudget > 0)
        {
            // Wir nutzen hier ein Tuple, um den Gegner an sein live berechnetes Gewicht zu binden
            List<(EnemySpawnConfig config, int dynWeight)> validOptions = new List<(EnemySpawnConfig, int)>();
            int totalWeight = 0;

            foreach (var enemy in pool)
            {
                // 1. PROGRESSION CHECK: Ist der Gegner schon freigeschaltet?
                if (currentCycle < enemy.minCycleToSpawn) continue;

                spawnedCounts.TryGetValue(enemy, out int currentCount);
                bool underLimit = enemy.maxPerSubWave <= 0 || currentCount < enemy.maxPerSubWave;

                if (enemy.cost <= currentBudget && underLimit)
                {
                    // 2. DYNAMISCHES GEWICHT BERECHNEN
                    // Wie viele Zyklen ist dieser Gegner schon aktiv?
                    int cyclesActive = currentCycle - enemy.minCycleToSpawn;

                    // Basis-Gewicht + (Zuwachs * aktive Zyklen)
                    int dynamicWeight = enemy.baseWeight + (enemy.weightIncreasePerCycle * cyclesActive);

                    // Sicherheitshalber: Ein Gewicht darf nie 0 oder negativ sein, falls man negative Zuwächse nutzt
                    dynamicWeight = Mathf.Max(1, dynamicWeight);

                    validOptions.Add((enemy, dynamicWeight));
                    totalWeight += dynamicWeight;
                }
            }

            if (validOptions.Count == 0) break; // Keine gültigen Gegner mehr für das Restbudget

            // 3. ZUFALL MIT DYNAMISCHER GEWICHTUNG
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
        // Da der GameManager am Ende der Phase alle Enemy-Tags löscht, ist diese Abfrage extrem verlässlich
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        return enemies.Length > 0;
    }
}