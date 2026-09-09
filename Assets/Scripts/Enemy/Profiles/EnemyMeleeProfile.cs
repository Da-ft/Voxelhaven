using UnityEngine;

[CreateAssetMenu(fileName = "EnemyProfile", menuName = "ScriptableObjects/Enemy Melee Profile")]
public class EnemyMeleeProfile : EnemyProfileSO
{
    public override void ExecuteAttack(EnemyBrain enemy)
    {
        DamageInfo damageInfo = new DamageInfo
        {
            amount = damage,
            isCritical = false,
            knockback = 0f,
            source = enemy.gameObject
        };

        if (Player.Instance != null)
        {
            Player.Instance.TakeDamage(damageInfo);
        }
    }
}