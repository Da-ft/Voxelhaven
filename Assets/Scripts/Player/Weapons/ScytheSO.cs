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

        // Visual Effect
        SpawnSlashVfx(player);

        // Damage Calc
        bool isCrit = Random.value <= stats.critRate;
        float finalDamage = isCrit ? (stats.damage * stats.critDamage) : stats.damage;

        // Find Enemy in Radius
        Collider[] hitEnemies = Physics.OverlapSphere(player.transform.position, stats.range, player.enemyLayer);

        int hitCount = 0;

        foreach (Collider hit in hitEnemies)
        {
            Vector3 dirToEnemy = (hit.transform.position - player.transform.position);
            dirToEnemy.y = 0f;

            float angleToEnemy = Vector3.Angle(player.transform.forward, dirToEnemy.normalized);

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

        // Set Spawnpoint, Fallback Center Player
        Transform spawnPoint = player.weaponSpawnPoint != null ? player.weaponSpawnPoint : player.transform;

        // VFX in Player Dir
        GameObject vfx = Instantiate(slashVfxPrefab, spawnPoint.position, player.transform.rotation);

        // Fallback: Destroy VFX
        // TODO: Pooling!
        Destroy(vfx, 1.5f);
    }
}