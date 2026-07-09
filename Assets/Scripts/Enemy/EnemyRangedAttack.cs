using UnityEngine;

[RequireComponent(typeof(EnemyTargeting))]
public class EnemyRangedAttack : MonoBehaviour
{
    [SerializeField] private EnemyDefinitionSO definition;
    [SerializeField] private float damage = 8f;
    [SerializeField] private Transform muzzlePoint; // optional, sonst wird transform.position genutzt

    private EnemyTargeting targeting;
    private float cooldown;
    private float cooldownTimer;

    private void Awake()
    {
        targeting = GetComponent<EnemyTargeting>();

        float attackSpeed = definition != null ? definition.AttackSpeed : 1f;
        cooldown = attackSpeed > 0f ? 1f / attackSpeed : 1f;

        if (definition != null && definition.ProjectilePrefab == null)
            Debug.LogWarning($"[EnemyRangedAttack] Kein Projectile-Prefab in EnemyDefinitionSO von {name} zugewiesen.");
    }

    private void Update()
    {
        if (!targeting.HasTarget || definition == null) return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer > 0f) return;

        if (targeting.DistanceToPlayer <= definition.AttackRange)
        {
            Fire();
            cooldownTimer = cooldown;
        }
    }

    private void Fire()
    {
        if (definition.ProjectilePrefab == null || Player.Instance == null) return;

        if (ProjectilePool.Instance == null)
        {
            Debug.LogWarning("[EnemyRangedAttack] Keine ProjectilePool-Instanz in der Szene gefunden.");
            return;
        }

        Vector3 spawnPosition = muzzlePoint != null ? muzzlePoint.position : transform.position;
        Vector3 aimPoint = CalculateLeadPoint(spawnPosition);
        Vector3 direction = (aimPoint - spawnPosition).normalized;

        Projectile projectile = ProjectilePool.Instance.Get(definition.ProjectilePrefab, spawnPosition, Quaternion.LookRotation(direction));
        projectile.Launch(direction, definition.ProjectileSpeed, damage, gameObject);
    }

    private Vector3 CalculateLeadPoint(Vector3 spawnPosition)
    {
        Vector3 playerPosition = Player.Instance.AvatarTransform.position;
        Vector3 playerVelocity = Player.Instance.AvatarVelocity;

        float distance = Vector3.Distance(spawnPosition, playerPosition);
        float estimatedTravelTime = definition.ProjectileSpeed > 0f ? distance / definition.ProjectileSpeed : 0f;

        // Einstufige Näherung (keine Iteration): Zielpunkt = aktuelle Position + Spielergeschwindigkeit * geschätzte Flugzeit Reagiert direkt auf Richtungswechsel des Spielers - abruptes Ausweichen unterläuft die Vorhersage zuverlässig.
        return playerPosition + playerVelocity * estimatedTravelTime;
    }
}