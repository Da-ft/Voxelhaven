using UnityEngine;

public class EnemyChaseState : IEnemyState
{
    // variable controlling the recalculation for pathfinding
    private float pathUpdateDelay = 0.2f;
    private float pathUpdateTimer;

    public void Enter(EnemyBrain enemy)
    {
        enemy.agent.isStopped = false;

        // Time Slicer: Set Timer between 0 and 0.2
        pathUpdateTimer = Random.Range(0f, pathUpdateDelay);
    }

    public void UpdateState(EnemyBrain enemy)
    {
        if (enemy.PlayerTarget == null) return;

        // Time Slicing
        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer <= 0f)
        {
            enemy.agent.SetDestination(enemy.PlayerTarget.position);

            pathUpdateTimer = pathUpdateDelay;
        }

        CheckAttackRange(enemy);
    }

    private void CheckAttackRange(EnemyBrain enemy)
    {
        // sqrMagnitude instead of a Mathf.Sqrt from vector3 for performance
        Vector3 vectorToPlayer = enemy.PlayerTarget.position - enemy.transform.position;
        float sqrDistance = vectorToPlayer.sqrMagnitude;

        float attackRange = enemy.enemyProfile.attackRange;
        float sqrAttackRange = attackRange * attackRange;

        if (sqrDistance <= sqrAttackRange)
        {
            enemy.ChangeState(enemy.AttackState);
        }
    }

    public void Exit(EnemyBrain enemy)
    {
        enemy.agent.isStopped = true;
    }
}
