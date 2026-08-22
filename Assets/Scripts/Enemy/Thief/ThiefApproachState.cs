using UnityEngine;

public class ThiefApproachState : IEnemyState
{
    private Transform scrapPileTransform;

    public void Enter(EnemyBrain enemy)
    {
        enemy.agent.isStopped = false;

        // Find Pile
        ScrapPile pile = Object.FindAnyObjectByType<ScrapPile>();
        if (pile != null)
        {
            scrapPileTransform = pile.transform;
            enemy.CurrentTarget = scrapPileTransform;
        }
    }

    public void UpdateState(EnemyBrain enemy)
    {
        // If no Scrap => Run!
        if (GameManager.Instance.Scrap <= 0 || scrapPileTransform == null)
        {
            enemy.ChangeState(new ThiefEscapeState());
            return;
        }

        enemy.agent.SetDestination(scrapPileTransform.position);
    }

    public void Exit(EnemyBrain enemy) { }
}