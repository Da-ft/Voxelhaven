using UnityEngine;

[RequireComponent(typeof(EnemyTargeting))]
public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private EnemyDefinitionSO definition;
    [SerializeField] private float damage = 10f;

    private EnemyTargeting targeting;
    private float cooldown;
    private float cooldownTimer;

    private void Awake()
    {
        targeting = GetComponent<EnemyTargeting>();

        float attackSpeed = definition != null ? definition.AttackSpeed : 1f;
        cooldown = attackSpeed > 0f ? 1f / attackSpeed : 1f;
    }

    private void Update()
    {
        if (!targeting.HasTarget || definition == null) return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer > 0f) return;

        if (targeting.DistanceToPlayer <= definition.AttackRange)
        {
            Attack();
            cooldownTimer = cooldown;
        }
    }

    private void Attack()
    {
        if (Player.Instance == null) return;

        // HitDirection wird u.a. von Player.TakeDamage für den Knockback-Impuls genutzt
        DamageInfo info = new DamageInfo(
            amount: damage,
            source: gameObject,
            type: DamageType.Physical,
            isCritical: false,
            hitPoint: transform.position,
            hitDirection: targeting.DirectionToPlayer
        );

        Player.Instance.TakeDamage(info);
    }
}
