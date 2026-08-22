using UnityEngine;

[CreateAssetMenu(fileName = "EnemyProfile", menuName = "ScriptableObjects/Enemy Range Profile")]
public class EnemyRangedProfile : EnemyProfileSO
{
    [Header("Ranged Settings")]
    [Tooltip("Das Prefab für den Schuss (z.B. ein Feuerball oder Pfeil)")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 15f;

    public override void ExecuteAttack(EnemyBrain enemy)
    {
        if (projectilePrefab == null) return;

        // startposition, slightly above waist
        Vector3 spawnPos = enemy.transform.position + Vector3.up * 1.5f;

        // Aim at Player
        Vector3 targetPos = enemy.PlayerTarget.position + Vector3.up * 1.5f; // Zielt auf die Brust des Spielers
        Vector3 direction = (targetPos - spawnPos).normalized;

        // Spawn projectile, rotate towards player
        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(direction));

        // Call projectile
        if (projectileObj.TryGetComponent(out EnemyProjectile projectile))
        {
            projectile.Initialize(damage, projectileSpeed);
        }
    }
}