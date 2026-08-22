using UnityEngine;

public class ThiefStealState : IEnemyState
{
    private float timer;

    public void Enter(EnemyBrain enemy)
    {
        enemy.agent.isStopped = true;

        if (enemy.enemyProfile is EnemyThiefProfile profile)
        {
            timer = profile.stealDuration;
        }
        else
        {
            timer = 1f;
        }
    }

    public void UpdateState(EnemyBrain enemy)
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            if (enemy.enemyProfile is EnemyThiefProfile profile)
            {
                // Steal scrap from gamemanager
                int stolen = GameManager.Instance.StealScrap(profile.maxScrapCapacity);
                enemy.CarriedScrap = stolen;
            }

            // Flee
            enemy.ChangeState(new ThiefEscapeState());
        }
    }

    public void Exit(EnemyBrain enemy)
    {
        enemy.agent.isStopped = false;
    }
}