using UnityEngine;

public class ThiefEscapeState : IEnemyState
{
    private Transform targetDropZone;

    public void Enter(EnemyBrain enemy)
    {
        enemy.agent.isStopped = false;

        // Priority 1: Seine eigene Heimat-Zone
        if (enemy.HomeZone != null)
        {
            targetDropZone = enemy.HomeZone;
        }
        // Priority 2: Nächstgelegene Zone auf der Map suchen
        else
        {
            ScrapDropZone closestZone = ScrapDropZone.GetClosestZone(enemy.transform.position);
            if (closestZone != null)
            {
                targetDropZone = closestZone.transform;
            }
        }

        enemy.CurrentTarget = targetDropZone;
    }

    public void UpdateState(EnemyBrain enemy)
    {
        if (targetDropZone == null)
        {
            // Fallback, falls gar keine Zone existiert
            Despawn(enemy);
            return;
        }

        enemy.agent.SetDestination(targetDropZone.position);

        float sqrDistance = (targetDropZone.position - enemy.transform.position).sqrMagnitude;
        if (sqrDistance <= 1.5f * 1.5f)
        {
            // TODO: Wave-Budget im WaveManager erhöhen
            Debug.Log($"[Dieb Entkommen] {enemy.CarriedScrap} Schrott entwendet!");

            Despawn(enemy);
        }
    }

    private void Despawn(EnemyBrain enemy)
    {
        ObjectPoolManager.ReturnObjectToPool(enemy.gameObject, ObjectPoolManager.PoolType.GameObjects);
    }

    public void Exit(EnemyBrain enemy) { }
}