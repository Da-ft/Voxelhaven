using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Enemies/Enemy Definition")]
public class EnemyDefinitionSO : ScriptableObject
{
    [Header("Base Stats")]
    [SerializeField] private float baseHealth = 20f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float xpReward = 5f;

    [Header("Wave Scaling")]
    [SerializeField] private bool participatesInWaveScaling = true;

    [Header("Ranged Behavior")]
    [SerializeField] private bool canReposition = false;

    // Wenn True: attackRange distanz zum spieler wird gehalten statt bis auf Kontaktdistanz heranzulaufen

    public float BaseHealth => baseHealth;
    public float MoveSpeed => moveSpeed;
    public float AttackSpeed => attackSpeed;
    public float AttackRange => attackRange;
    public float XpReward => xpReward;
    public bool ParticipatesInWaveScaling => participatesInWaveScaling;
    public bool CanReposition => canReposition;
}
