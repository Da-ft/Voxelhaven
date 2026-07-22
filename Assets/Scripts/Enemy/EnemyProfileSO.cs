using UnityEngine;

public abstract class EnemyProfileSO : ScriptableObject
{
    public string enemyName;

    [Header("Base Stats")]
    [Tooltip("Defines the starting Maximum HP, scales with the 'level' modifier.")]
    public float maxHealth;
    [Tooltip("This should be a decimal Value: 1.25 = 1.25 HP per Second.")]
    public float healthRegen;
    [Tooltip("Defines the Movespeed of the selected Unit.")]
    public float moveSpeed;

    [Header("Combat Stats")]
    [Tooltip("Defines the starting Raw Damage without Armor Calc, scales with the 'level' modifier.")]
    public float damage;
    [Tooltip("Defines the Range of this Unit.")]
    public float attackRange;
    [Tooltip("This should be a decimal Value: 1.25 = 1.25 Attacks per Second.")]
    public float attackSpeed;

    [Tooltip("Defines the value of dropped XP, scales with 'level' modifier.")]
    public float xpToDrop;
    [Tooltip("Defines the starting power of the 'level' modifier.")]
    public int startingLevel;

    public abstract void ExecuteAttack(EnemyBrain enemy);
}
