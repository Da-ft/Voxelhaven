using UnityEngine;

[CreateAssetMenu(fileName = "Scythe", menuName = "Weapons/Scythe")]
public class ScytheSO : PlayerWeaponSO
{
    [Header("Scythe Arc Settings")]
    [Tooltip("Der Winkel des Halbmond-Schwungs in Grad (z. B. 140° vor dem Spieler)")]
    [Range(30f, 360f)] public float slashAngle = 140f;

    [Header("Visual Effects")]
    [Tooltip("Das VFX-Prefab für den Halbmond-Slash (z. B. ein Partikelsystem)")]
    public GameObject slashVfxPrefab;
    public Vector3 vfxRotationOffset;

    public override void ExecuteAttack(PlayerController player, WeaponInstance instance, Transform currentTarget = null)
    {
        WeaponStats stats = instance.GetCurrentStats();

        // Visual Effect
        SpawnSlashVfx(player);

        // Damage Calc
        bool isCrit = Random.value <= stats.critRate;
        float finalDamage = isCrit ? (stats.damage * stats.critDamage) : stats.damage;

        DamageInfo damageInfo = new DamageInfo
        {
            amount = finalDamage,
            isCritical = isCrit,
            knockback = stats.knockback,
            source = player.gameObject
        };

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
                    enemy.TakeDamage(damageInfo);
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

        Quaternion finalRotation = player.transform.rotation * Quaternion.Euler(vfxRotationOffset);

        // VFX in Player Dir

        GameObject vfx = Instantiate(slashVfxPrefab, spawnPoint.position, finalRotation);

        // Fallback: Destroy VFX
        // TODO: Pooling!
        Destroy(vfx, 1.5f);
    }

    public override void DrawGizmos(PlayerController player, WeaponStats stats)
    {
        Gizmos.color = Color.cyan;
        Vector3 position = player.transform.position + Vector3.up * 0.1f;

        float range = stats.range;
        float angle = slashAngle;

        int segments = 25;
        float halfAngle = angle / 2f;
        Vector3 forward = player.transform.forward;

        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;

        Gizmos.DrawLine(position, position + leftDir * range);
        Gizmos.DrawLine(position, position + rightDir * range);

        Vector3 lastPoint = position + leftDir * range;
        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = -halfAngle + (angle / segments) * i;
            Vector3 dir = Quaternion.Euler(0, currentAngle, 0) * forward;
            Vector3 nextPoint = position + dir * range;
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }
}