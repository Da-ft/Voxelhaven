public interface IEnemyState
{
    void Enter(EnemyBrain enemy);
    void UpdateState(EnemyBrain enemy);
    void Exit(EnemyBrain enemy);
}
