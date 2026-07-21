using UnityEngine;

public class EnemyAttackState : IEnemyState
{
    public void Enter(EnemyBrain enemy)
    {
        // stopp movement while attacking
        enemy.agent.isStopped = true;
    }

    public void UpdateState(EnemyBrain enemy)
    {
        if (enemy.PlayerTarget == null) return;

        // rangecheck, if not in range go back to chasing
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.PlayerTarget.position);

        // add a little padding to the range parameter to minimize glitching between states
        if (distanceToPlayer > enemy.enemyProfile.attackRange + 0.2f)
        {
            enemy.ChangeState(enemy.ChaseState);
            return;
        }

        // Attack on Cooldown
        if(enemy.currentAttackCooldown <= 0f)
        {
            // call Attack Logic from specific SO
            enemy.enemyProfile.ExecuteAttack(enemy);

            // reset cd
            enemy.currentAttackCooldown = enemy.enemyProfile.attackSpeed;
        }
    }

    public void Exit(EnemyBrain enemy)
    {
        // Reset Logic if needed
    }
}
