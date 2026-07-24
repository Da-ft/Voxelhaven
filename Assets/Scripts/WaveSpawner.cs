using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    public GameObject[] nightEnemies;
    public GameObject[] dayEnemies;

    private Coroutine currentWaveCoroutine;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }
    }

    private void HandlePhaseChanged(GameManager.GamePhase newPhase)
    {
        // alte welle stoppen wenn noch eine läuft
        if (currentWaveCoroutine != null)
        {
            StopCoroutine(currentWaveCoroutine);
            currentWaveCoroutine = null;
        }

        switch (newPhase)
        {
            case GameManager.GamePhase.Night:
                currentWaveCoroutine = StartCoroutine(SpawnWave(nightEnemies, 20));
                break;
            case GameManager.GamePhase.Day:
                currentWaveCoroutine = StartCoroutine(SpawnWave(dayEnemies, 20));
                break;
            case GameManager.GamePhase.Rest:
                // Nichts Spawnen! Shop/UI öffnen!
                break;
        }
    }

    private IEnumerator SpawnWave(GameObject[] enemyPool, int amountToSpawn)
    {
        for (int i = 0; i < amountToSpawn; i++)
        {
            // Einen zufälligen Gegner aus dem passenden Pool wählen
            GameObject randomEnemy = enemyPool[Random.Range(0, enemyPool.Length)];

            // TODO: Spawnpunkt berechnen
            Vector3 spawnPos = transform.position; // Platzhalter

            // Gegner spawnen
            Instantiate(randomEnemy, spawnPos, Quaternion.identity);

            // Kurz warten bis zum nächsten Gegner
            yield return new WaitForSeconds(1.5f);
        }

        // Wenn alle gespawnt und tot sind -> Phase wechseln!
        // (Das machst du idealerweise in einem separaten Check, wenn alle Gegner besiegt wurden)
    }
}