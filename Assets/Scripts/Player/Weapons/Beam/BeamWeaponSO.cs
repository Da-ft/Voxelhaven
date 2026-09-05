using UnityEngine;

[CreateAssetMenu(fileName = "BeamWeapon", menuName = "Weapons/BeamWeapon")]
public class BeamWeaponSO : PlayerWeaponSO
{
    [Header("Beam Settings")]
    [Tooltip("Dauer in Sekunden, bevor der Strahl schaden macht.")]
    public float windupTime = 0.5f;

    [Header("Beam Dimensions")]
    public float beamWidth = 1.0f;

    public override void ExecuteAttack(PlayerController player, WeaponInstance instance, Transform currentTarget = null)
    {
        // get controller from equipped weapon
        BeamWeaponController beamCtrl = player.GetComponentInChildren<BeamWeaponController>();
        if (beamCtrl == null) return;

        // subscribe "tick"
        beamCtrl.RegisterAttackTick(windupTime);

        // return through windup
        if (!beamCtrl.IsReadyToFire) return;

        // Beam active, fire away
        WeaponStats stats = instance.GetCurrentStats();
        float currentRange = stats.range;
        float tickDamage = stats.damage;

        Transform spawnPoint = player.weaponSpawnPoint != null ? player.weaponSpawnPoint : player.transform;
        Vector3 origin = spawnPoint.position;
        Vector3 direction = player.transform.forward;

        Vector3 halfExtents = new Vector3(beamWidth * 0.5f, 0.1f, 0.05f);
        Quaternion orientation = Quaternion.LookRotation(direction);

        // raycast hits "everything" to stop ray from fireing through walls
        RaycastHit[] hits = Physics.BoxCastAll(origin, halfExtents, direction, orientation, currentRange);

        if (hits.Length > 0)
        {
            float closestDistance = currentRange;

            foreach (var hit in hits)
            {
                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                }
            }

            currentRange = closestDistance;

            foreach (var hit in hits)
            {
                // Kleine Toleranz, damit nebeneinander stehende Ziele im breiten Strahl gleichzeitig getroffen werden
                if (hit.distance <= currentRange + 0.1f)
                {
                    if (hit.collider.TryGetComponent(out EnemyBrain enemy))
                    {
                        // TODO: Crits
                        enemy.TakeDamage(tickDamage);
                    }
                }
            }
        }


        // give length back to vfx graph
        beamCtrl.UpdateBeamVisuals(currentRange);
    }

    public override void DrawGizmos(PlayerController player, WeaponStats stats)
    {
        Gizmos.color = Color.red;
        Transform spawn = player.weaponSpawnPoint != null ? player.weaponSpawnPoint : player.transform;
        Vector3 origin = spawn.position;
        Vector3 direction = player.transform.forward;

        Gizmos.matrix = Matrix4x4.TRS(origin + direction * (stats.range * 0.5f), Quaternion.LookRotation(direction), new Vector3(beamWidth, 0.2f, stats.range));
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
