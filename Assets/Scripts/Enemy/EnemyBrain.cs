using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBrain : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Place correct Enemy Scriptable Object!")]
    public EnemyProfileSO enemyProfile;
    public NavMeshAgent agent;
    public GameObject floatingTextPrefab;
    public Transform PlayerTarget { get; private set; }

    [Header("Runtime Variables (Read Only)")]
    public float currentHealth;
    public float currentAttackCooldown;
    public bool IsActionLocked = false;

    [Header("Thief Variables")]
    public Transform CurrentTarget { get; set; }
    public int CarriedScrap { get; set; } = 0;
    public Transform HomeZone { get; set; }

    // FSM
    private IEnemyState currentState;

    // Predefined States
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
            agent.stoppingDistance = enemyProfile.attackRange; // Stopp Agent at attackdistance
        }

        // Initialize on Start
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

    public void TakeDamage(DamageInfo damageInfo)
    {
        currentHealth -= damageInfo.amount;

        ShowFloatingText(damageInfo);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ShowFloatingText(DamageInfo damageInfo)
    {
        if (floatingTextPrefab == null) return;

        GameObject textObject = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity, transform);
    }

    private void Die()
    {
        if (CarriedScrap > 0 && enemyProfile != null && enemyProfile.dropPrefab != null)
        {
            for (int i = 0; i < CarriedScrap; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * 1.5f;

                Vector3 spawnPos = new Vector3(
                    transform.position.x + randomOffset.x,
                    0.5f,
                    transform.position.z + randomOffset.y
                    );

                ObjectPoolManager.SpawnObject(
                    enemyProfile.dropPrefab,
                    spawnPos,
                    Quaternion.identity,
                    ObjectPoolManager.PoolType.Collectibles
                    );
            }

            Debug.Log($"[EnemyBrain] Thief killed! {CarriedScrap} Scrap dropped.");
            CarriedScrap = 0;
        }

        enemyProfile.ExecuteDeath(this);
    }

    public void Initialize()
    {
        currentHealth = enemyProfile.maxHealth;
        CarriedScrap = 0; // Reset for Object Pooling!

        if (Player.Instance != null && Player.Instance.AvatarTransform != null)
        {
            PlayerTarget = Player.Instance.AvatarTransform;

            if (enemyProfile is not EnemyThiefProfile)
            {
                CurrentTarget = PlayerTarget;
                ChangeState(ChaseState);
            }

            if (enemyProfile is EnemyThiefProfile)
            {
                ChangeState(new ThiefApproachState());
            }
        }
    }
}
