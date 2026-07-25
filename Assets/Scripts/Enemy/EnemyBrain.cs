using UnityEngine;
using UnityEngine.AI;

public class EnemyBrain : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Place correct Enemy Scriptable Object!")]
    public EnemyProfileSO enemyProfile;
    public NavMeshAgent agent;
    public Transform PlayerTarget { get; private set; }

    [Header("Runtime Variables (Read Only)")]
    public float currentHealth;
    public float currentAttackCooldown;
    public bool IsActionLocked = false;

    // FSM
    private IEnemyState currentState;

    // Vordefinierte States, verhindert ständige "new" instanziierung
    public EnemyChaseState ChaseState { get; private set; } = new EnemyChaseState();
    public EnemyAttackState AttackState { get; private set; } = new EnemyAttackState();

    private void Start()
    {
        // Load Scriptable Object Stats
        if (enemyProfile != null)
        {
            currentHealth = enemyProfile.maxHealth;
        }

        if (agent != null && enemyProfile != null)
        {
            agent.speed = enemyProfile.moveSpeed;
            agent.stoppingDistance = enemyProfile.attackRange; // Stoppt den Agenten auf Angriffsdistanz
        }

        // Automatische Initialisierung beim Spawnen!
        Initialize();
    }

    private void Update()
    {
        if (currentAttackCooldown > 0)
        {
            currentAttackCooldown -= Time.deltaTime;
        }

        currentState?.UpdateState(this);
    }

    public void ChangeState(IEnemyState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState?.Enter(this);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // TODO: Death Logic, Pooling, Dropping XP etc.
        enemyProfile.ExecuteDeath(this);
    }

    public void Initialize()
    {
        if (Player.Instance != null && Player.Instance.AvatarTransform != null)
        {
            PlayerTarget = Player.Instance.AvatarTransform;
            ChangeState(ChaseState);
        }
        else
        {
            Debug.LogError("Enemy konnte Avatar nicht finden! Ist Scenebootstrapper durchgelaufen?");
        }
    }
}
