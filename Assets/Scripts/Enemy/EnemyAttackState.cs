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
        if (enemy.IsActionLocked) return;
        if (enemy.PlayerTarget == null) return;

        Vector3 lookDirection = enemy.PlayerTarget.position - enemy.transform.position;
        lookDirection.y = 0f; // Verhindert, dass der Gegner sich nach oben/unten neigt
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            // Geschmeidiges Mitdrehen
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

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
