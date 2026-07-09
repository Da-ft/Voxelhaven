using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Enemies/Enemy Definition")]
public class EnemyDefinitionSO : ScriptableObject
{
    [Header("Base Stats")]
    [SerializeField] private float baseHealth = 20f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackSpeed = 1f; // Angriffe pro Sekunde
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float xpReward = 5f;

    [Header("Wave Scaling")]
    [SerializeField] private bool participatesInWaveScaling = true;

    [Header("Ranged Behaviour")]
    [SerializeField] private bool canReposition = false;
    // Wenn true: Gegner versucht, exakt auf 'attackRange' Distanz zum Spieler zu bleiben,
    // statt bis auf Kontaktdistanz heranzulaufen. Unabhängig davon, ob EnemyRangedAttack
    // verwendet wird - Bewegungsverhalten und Angriffsart sind bewusst getrennte Fähigkeiten.

    [Header("Projectile (nur bei EnemyRangedAttack relevant)")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;

    public float BaseHealth => baseHealth;
    public float MoveSpeed => moveSpeed;
    public float AttackSpeed => attackSpeed;
    public float AttackRange => attackRange;
    public float XpReward => xpReward;
    public bool ParticipatesInWaveScaling => participatesInWaveScaling;
    public bool CanReposition => canReposition;
    public GameObject ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => projectileSpeed;
}
