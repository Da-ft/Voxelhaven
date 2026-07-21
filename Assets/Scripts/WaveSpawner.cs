using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject enemyPrefab;

    [Header("Test Wave Settings")]
    public int amountToSpawn = 20;
    public float spawnRadius = 15f; // Wie weit vom Spawner entfernt?
    public float timeBetweenSpawns = 0.2f; // Kurze Pause zwischen jedem Spawn

    private void Start()
    {
        // Startet den Test-Spawn direkt beim Spielstart
        StartCoroutine(SpawnTestWave());
    }

    private IEnumerator SpawnTestWave()
    {
        while (Player.Instance == null || Player.Instance.AvatarTransform == null)
        {
            yield return null;
        }

        Debug.Log($"Starte Test-Welle: {amountToSpawn} Gegner werden gespawnt.");

        for (int i = 0; i < amountToSpawn; i++)
        {
            // 1. Zufällige Position auf einem Kreis um den Spawner berechnen
            // (insideUnitCircle.normalized sorgt dafür, dass sie wirklich am Rand des Radius spawnen)
            Vector2 randomPoint = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 spawnPosition = new Vector3(randomPoint.x, transform.position.y, randomPoint.y) + transform.position;

            // 2. Den eigentlichen Spawn-Befehl ausführen
            SpawnEnemy(spawnPosition);

            // 3. Kurz warten, bevor der nächste Gegner spawnt (verhindert Lag-Spikes)
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    public void SpawnEnemy(Vector3 spawnPosition)
    {
        // Gegner instanziieren
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // Das Gehirn holen und das Ziel direkt übergeben (Dependency Injection)
        if (newEnemy.TryGetComponent(out EnemyBrain brain))
        {
            brain.Initialize();
        }
    }
}