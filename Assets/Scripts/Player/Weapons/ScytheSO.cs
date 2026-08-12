using UnityEngine;

[CreateAssetMenu(fileName = "Scythe", menuName = "ScriptableObjects/Weapons/Scythe")]
public class ScytheSO : PlayerWeaponSO
{
    [Header("Scythe Arc Settings")]
    [Tooltip("Der Winkel des Halbmond-Schwungs in Grad (z. B. 140° vor dem Spieler)")]
    [Range(30f, 360f)] public float slashAngle = 140f;

    [Header("Visual Effects")]
    [Tooltip("Das VFX-Prefab für den Halbmond-Slash (z. B. ein Partikelsystem)")]
    public GameObject slashVfxPrefab;

    public override void ExecuteAttack(PlayerController player, WeaponInstance instance, Transform currentTarget = null)
    {
        WeaponStats stats = instance.GetCurrentStats();

        // 1. Visual Effect (Mond-Schwung) abfeuern
        SpawnSlashVfx(player);

        // 2. Schadensberechnung
        bool isCrit = Random.value <= stats.critRate;
        float finalDamage = isCrit ? (stats.damage * stats.critDamage) : stats.damage;

        // Alle Gegner im maximalen Radius (stats.Range) suchen
        Collider[] hitEnemies = Physics.OverlapSphere(player.transform.position, stats.range, player.enemyLayer);

        int hitCount = 0;

        foreach (Collider hit in hitEnemies)
        {
            // Richtungsvektor vom Spieler zum Gegner (Höhenunterschied ignorieren)
            Vector3 dirToEnemy = (hit.transform.position - player.transform.position);
            dirToEnemy.y = 0f;

            // Winkel zwischen Spieler-Blickrichtung und Gegner berechnen
            float angleToEnemy = Vector3.Angle(player.transform.forward, dirToEnemy.normalized);

            // Liegt der Gegner innerhalb der Hälfte unseres Halbmond-Winkels?
            if (angleToEnemy <= slashAngle / 2f)
            {
                if (hit.TryGetComponent(out EnemyBrain enemy))
                {
                    enemy.TakeDamage(finalDamage);
                    hitCount++;
                }
            }
        }

        Debug.Log($"[SENSE] Halbmond-Schwung ({slashAngle}°) ausgeführt! {hitCount} Gegner für {finalDamage} Dmg getroffen.");
    }

    private void SpawnSlashVfx(PlayerController player)
    {
        if (slashVfxPrefab == null) return;

        // Spawn am weaponSpawnPoint (falls zugewiesen), sonst direkt am Player
        Transform spawnPoint = player.weaponSpawnPoint != null ? player.weaponSpawnPoint : player.transform;

        // VFX in Ausrichtung des Spielers spawnen
        GameObject vfx = Instantiate(slashVfxPrefab, spawnPoint.position, player.transform.rotation);

        // Fallback: Zerstört das VFX nach 1.5 Sekunden, falls es sich nicht selbst aufräumt
        Destroy(vfx, 1.5f);
    }
}